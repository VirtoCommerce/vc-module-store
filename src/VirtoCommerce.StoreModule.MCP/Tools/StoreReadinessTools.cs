using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using VirtoCommerce.MCP.Core;
using VirtoCommerce.MCP.Core.Attributes;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.MCP.Models;
using VirtoCommerce.StoreModule.MCP.Services;
using StorePermissions = VirtoCommerce.StoreModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.StoreModule.MCP.Tools;

// What a store is missing before it goes live, and what a store's setup consists of on this installation.
[McpServerToolType]
[McpScope(McpScopes.Admin)]
[McpPermission(StorePermissions.Read)]
public class StoreReadinessTools(StoreAccess access, IStoreSetupService setupService, StoreBackOfficeLinks links, ILogger<StoreReadinessTools> logger)
{
    // The most choices listed per part; a part lists its most likely choices first.
    public const int MaxChoicesPerPart = 50;

    // The rules every section of a setup follows.
    private static readonly string[] _setupRules =
    [
        "A section that is left out changes nothing, and neither does a field that is left out.",
        "An empty text clears a field.",
        "A list is the complete new list: what it does not name is removed.",
        "A settings map sets the named settings and leaves the others as they are; null returns a setting to its default, " +
            "and a setting without a default takes a value instead.",
        "Secrets, such as payment keys, are never sent here: a person enters them in the back office.",
    ];

    [McpServerTool(Name = "get_store_readiness_report", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("What a store is missing before it can go live. Each finding says what is missing, why it matters, how " +
        "to fix it (a tool or a back-office screen) and whether it blocks opening the store. With a currency or a country, " +
        "it also says why that currency or country cannot check out, and can_check_out says whether customers can. " +
        "ready_to_open is the same with or without them, as open_store decides it. Use it before open_store and after " +
        "every setup change.")]
    public async Task<object> GetStoreReadinessReport(
        RequestContext<CallToolRequestParams> context,
        [Description("Store id, as list_stores returns it.")] string store_id,
        [Description("Currency code, such as EUR, to also check that customers can pay in it.")] string currency = null,
        [Description("Country code, such as DEU, to also check that customers there can check out.")] string country = null)
    {
        var store = await access.GetReadableStoreAsync(context.User, store_id);
        var findings = await setupService.CheckAsync(store, currency, country);
        var link = await links.GetStoresLinkAsync();
        // Going live is decided as open_store decides it, without a currency or a country; with them, the findings say
        // what keeps their customers from checking out, and can_check_out sums it up.
        var forMarket = !string.IsNullOrWhiteSpace(currency) || !string.IsNullOrWhiteSpace(country);
        var readyToOpen = !(forMarket ? await setupService.CheckAsync(store) : findings).Any(setupService.BlocksGoLive);
        // A part's findings show with its state: the others are counted, and still decide readiness.
        var (rows, notShown) = access.SplitReadableFindings(context.User, store.Id, StoreFinding.CreateList(findings, setupService, link));

        return new
        {
            store_id = store.Id,
            name = store.Name,
            state = StoreStateNames.ToName(store.StoreState),
            ready_to_open = readyToOpen,
            can_check_out = forMarket ? !findings.Any(setupService.BlocksGoLive) : (bool?)null,
            findings = rows,
            not_shown = notShown,
            checked_parts = setupService.GetParts().OfType<IStoreSetupCheck>().Select(x => x.Name).ToList(),
            back_office = link,
            next_step = GetNextStep(store, readyToOpen),
        };
    }

    [McpServerTool(Name = "get_store_setup_options", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("What a store's setup consists of on this installation: every part (the store's basics and settings, and " +
        "the parts of installed modules such as payment, shipping and tax), the fields of each part's section as a JSON " +
        "schema, and what can be chosen (languages, currencies, catalogs, method and provider codes with their settings). " +
        "A part's choices show with the part's read permission, at most 50 per part. Read it before setting up a store, " +
        "and send only what it lists.")]
    public async Task<object> GetStoreSetupOptions(RequestContext<CallToolRequestParams> context)
    {
        var parts = new List<object>();

        foreach (var part in setupService.GetParts())
        {
            var row = await GetOptionsAsync(context.User, part);

            if (row != null)
            {
                parts.Add(row);
            }
        }

        return new
        {
            parts,
            rules = _setupRules,
            next_step = "Fill the sections of the parts the store needs, then call create_configured_store for a new store or " +
                "apply_store_setup for an existing one, with dryRun first.",
        };
    }

    // A part's row: its section's schema and its choices, or null when it has neither. The choices are the part's data,
    // such as the catalogs, so they show with the part's read permission, as its state and findings do; without it, the
    // row says so. A part that cannot list its options is named with a reason, and the others are still listed.
    protected virtual async Task<object> GetOptionsAsync(ClaimsPrincipal user, IStoreSetupPart part)
    {
        try
        {
            var section = part as IStoreSetupSection;
            var withChoices = part as IStoreSetupChoices;
            var choicesShown = withChoices != null && StoreAccess.CanReadForSomeStore(user, part);
            var choices = choicesShown ? await withChoices.GetChoicesAsync() ?? [] : [];

            if (section == null && choices.Count == 0 && (withChoices == null || choicesShown))
            {
                return null;
            }

            return new
            {
                name = part.Name,
                title = part.Title,
                required_for_new_store = section?.IsRequiredForNewStore ?? false,
                section_schema = section == null ? (object)null : AIJsonUtilities.CreateJsonSchema(section.SectionType, serializerOptions: McpSerialization.Options),
                choices = choices.Take(MaxChoicesPerPart).Select(x => new
                {
                    field = x.Field,
                    value = x.Value,
                    title = x.Title,
                    description = x.Description,
                    settings = x.Settings?.Count > 0 ? x.Settings : null,
                }).ToList(),
                choices_cut = choices.Count > MaxChoicesPerPart ? $"{MaxChoicesPerPart} of {choices.Count} choices listed, the most likely first." : null,
                choices_not_shown = withChoices != null && !choicesShown ? $"needs {part.Permissions?.Read}" : null,
            };
        }
        catch (Exception ex)
        {
            // The answer never carries an exception's text: it may hold a third party's trace or a secret.
            logger.LogError(ex, "The store setup part {Part} could not list its options", part.Name);

            return new
            {
                name = part.Name,
                title = part.Title,
                unavailable = $"{part.Title} could not list its options; the platform log has the details. Its section cannot be sent until it can.",
            };
        }
    }

    private static string GetNextStep(Store store, bool readyToOpen)
    {
        if (!readyToOpen)
        {
            return "Fix the blocking findings first, then check again.";
        }

        return store.StoreState == StoreState.Open
            ? "The store is open, and nothing blocks it."
            : "Nothing blocks going live: open_store opens the store after approval.";
    }
}
