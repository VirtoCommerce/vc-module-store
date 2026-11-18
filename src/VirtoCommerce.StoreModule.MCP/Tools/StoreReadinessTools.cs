using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
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
public class StoreReadinessTools(StoreAccess access, IStoreSetupService setupService, StoreBackOfficeLinks links)
{
    // The rules every section of a setup follows.
    private static readonly string[] _setupRules =
    [
        "A section that is left out changes nothing, and neither does a field that is left out.",
        "A list is the complete new list: what it does not name is removed.",
        "A settings map sets the named settings and leaves the others as they are; null returns a setting to its default.",
        "Secrets, such as payment keys, are never sent here: a person enters them in the back office.",
    ];

    [McpServerTool(Name = "get_store_readiness_report", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("What a store is missing before it can go live. Each finding says what is missing, why it matters, how " +
        "to fix it (a tool or a back-office screen) and whether it blocks opening the store. With a currency or a country, " +
        "it also says why that currency or country cannot check out. Use it before open_store and after every setup change.")]
    public async Task<object> GetStoreReadinessReport(
        RequestContext<CallToolRequestParams> context,
        [Description("Store id, as list_stores returns it.")] string store_id,
        [Description("Currency code, such as EUR, to also check that customers can pay in it.")] string currency = null,
        [Description("Country code, such as DEU, to also check that customers there can check out.")] string country = null)
    {
        var store = await access.GetReadableStoreAsync(context.User, store_id);
        var findings = await setupService.CheckAsync(store, currency, country);
        var link = await links.GetStoresLinkAsync();
        var parts = setupService.GetParts();
        var sectionParts = parts.OfType<IStoreSetupSection>().Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rows = findings
            .Select(x => StoreFinding.Create(x, sectionParts.Contains(x.Part), link))
            .OrderBy(x => McpFindingSeverities.GetRank(x.Severity))
            .ToList();
        var readyToOpen = rows.TrueForAll(x => !x.BlocksGoLive);

        return new
        {
            store_id = store.Id,
            name = store.Name,
            state = StoreStateNames.ToName(store.StoreState),
            ready_to_open = readyToOpen,
            findings = rows,
            checked_parts = parts.OfType<IStoreSetupCheck>().Select(x => x.Name).ToList(),
            back_office = link,
            next_step = GetNextStep(store, readyToOpen),
        };
    }

    [McpServerTool(Name = "get_store_setup_options", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("What a store's setup consists of on this installation: every part (the store's basics and settings, and " +
        "the parts of installed modules such as payment, shipping and tax), the fields of each part's section as a JSON " +
        "schema, and what can be chosen (languages, currencies, method and provider codes with their settings). Read it " +
        "before setting up a store, and send only what it lists.")]
    public async Task<object> GetStoreSetupOptions()
    {
        var parts = new List<object>();

        foreach (var part in setupService.GetParts())
        {
            var section = part as IStoreSetupSection;
            var choices = part is IStoreSetupChoices withChoices ? await withChoices.GetChoicesAsync() ?? [] : [];

            if (section == null && choices.Count == 0)
            {
                continue;
            }

            parts.Add(new
            {
                name = part.Name,
                title = part.Title,
                required_for_new_store = section?.IsRequiredForNewStore ?? false,
                section_schema = section == null ? (object)null : AIJsonUtilities.CreateJsonSchema(section.SectionType, serializerOptions: McpSerialization.Options),
                choices = choices.Select(x => new
                {
                    field = x.Field,
                    value = x.Value,
                    title = x.Title,
                    description = x.Description,
                    settings = x.Settings?.Count > 0 ? x.Settings : null,
                }).ToList(),
            });
        }

        return new
        {
            parts,
            rules = _setupRules,
            next_step = "Fill the sections of the parts the store needs, then call create_configured_store for a new store or " +
                "apply_store_setup for an existing one, with dryRun first.",
        };
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
