using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using VirtoCommerce.MCP.Core;
using VirtoCommerce.MCP.Core.Attributes;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.MCP.Models;
using VirtoCommerce.StoreModule.MCP.Services;
using StorePermissions = VirtoCommerce.StoreModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.StoreModule.MCP.Tools;

// One call creates a configured store, and one call changes an existing store's setup. Every call waits for approval,
// and its preview spans every part the call names.
[McpServerToolType]
[McpScope(McpScopes.Admin)]
public class StoreSetupTools(
    StoreAccess access,
    StoreSetupLeftovers leftovers,
    StoreBackOfficeLinks links,
    IStoreService storeService,
    IStoreSearchService storeSearchService,
    IStoreSetupService setupService)
{
    // Sections are read with the names the agent sees. A field a section does not have is refused, not ignored, so a
    // misspelled field never changes nothing silently.
    private static readonly JsonSerializerOptions _sectionOptions = new(McpSerialization.Options)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    [McpServerTool(Name = StoreSetupLeftovers.CreateToolName, ReadOnly = false, Destructive = false, Idempotent = true)]
    [Authorize(StorePermissions.Create)]
    [McpRequiresApproval]
    [Description("Create a store with its whole setup in one call: the basics (required, with the name), the store " +
        "settings and the parts of the installed modules (such as payment, shipping and tax), each as a section that " +
        "get_store_setup_options describes. The store is created closed. Call it with dryRun first: the preview shows " +
        "everything it will create and what the store would still miss. When the preview says the store will not be " +
        "ready, ask the user before submitting. The change is submitted for approval: tell the user an approver must " +
        "approve it.")]
    public async Task<StoreSetupPreview> CreateConfiguredStore(
        RequestContext<CallToolRequestParams> context,
        [Description("Id of the new store: letters, digits, '_' and '-'. It cannot be changed later.")] string store_id,
        [Description("The setup: one section per part, keyed by the part name from get_store_setup_options, such as " +
            "basics, settings and payment.")] Dictionary<string, JsonElement> setup,
        bool dryRun = false)
    {
        if (string.IsNullOrWhiteSpace(store_id))
        {
            throw new McpException("store_id is required: the id of the new store.");
        }

        var user = context.User;
        var sections = await ReadSectionsAsync(setup);
        var warnings = new List<string>();
        var existingStore = await storeService.GetByIdAsync(store_id);
        var isNewStore = existingStore == null;

        if (!isNewStore)
        {
            // Only a store left closed by a failed create is continued; a store made by hand or by a create that
            // succeeded is changed with apply_store_setup.
            var failedChange = existingStore.StoreState == StoreState.Closed ? await leftovers.FindFailedCreateAsync(existingStore) : null;

            if (failedChange == null)
            {
                throw new McpException($"Store '{existingStore.Id}' already exists. Change its setup with apply_store_setup.");
            }

            // Continuing it changes the store: read and update access to it, as for every other change.
            await access.GetUpdatableStoreAsync(user, existingStore.Id);
            warnings.Add($"Continues the setup of the closed store '{existingStore.Id}' that the failed change {failedChange.Id} left.");
        }

        EnsurePartPermissions(user, sections, store_id, isNewStore);
        warnings.AddRange(await GetSameNameWarningsAsync(sections, store_id));

        return await RunAsync(user, "create", store_id, existingStore?.Name, isNewStore, sections, dryRun, warnings);
    }

    [McpServerTool(Name = "apply_store_setup", ReadOnly = false, Destructive = true, Idempotent = true)]
    [McpPermission(StorePermissions.Update)]
    [McpRequiresApproval]
    [Description("Change the setup of an existing store: one part or several in one call, each as a section that " +
        "get_store_setup_options describes (basics, settings, and parts such as payment, shipping and tax). A section " +
        "or a field that is left out changes nothing; a list is the complete new list. Call it with dryRun first: the " +
        "preview shows the difference from the current setup and what the store would miss afterwards. The change is " +
        "submitted for approval: tell the user an approver must approve it.")]
    public async Task<StoreSetupPreview> ApplyStoreSetup(
        RequestContext<CallToolRequestParams> context,
        [Description("Store id, as list_stores returns it.")] string store_id,
        [Description("The sections to change, keyed by the part name from get_store_setup_options.")] Dictionary<string, JsonElement> setup,
        bool dryRun = false)
    {
        var store = await access.GetUpdatableStoreAsync(context.User, store_id);
        var sections = await ReadSectionsAsync(setup);
        EnsurePartPermissions(context.User, sections, store.Id, isNewStore: false);
        // A rename gets the warning a new store's name gets.
        var warnings = await GetSameNameWarningsAsync(sections, store.Id, store.Name);

        return await RunAsync(context.User, "apply", store.Id, store.Name, isNewStore: false, sections, dryRun, warnings);
    }

    // storeName is the name of a store that exists, as saved: a rename shows as a row, not as the store's name.
    private async Task<StoreSetupPreview> RunAsync(ClaimsPrincipal user, string action, string storeId, string storeName, bool isNewStore, IDictionary<string, object> sections, bool dryRun, IList<string> warnings)
    {
        var request = AbstractTypeFactory<StoreSetupRequest>.TryCreateInstance();
        request.StoreId = storeId;
        request.IsNewStore = isNewStore;
        request.Sections = sections;

        var result = dryRun ? await setupService.PlanAsync(request) : await setupService.ApplyAsync(request);

        if (result.Errors.Count > 0)
        {
            throw await CreateRefusalAsync(result.Errors);
        }

        // An approval for nothing is not submitted.
        if (result.Parts.All(x => x.Status == McpSetupPartStatus.NoChange))
        {
            throw new McpException($"Nothing to change: store '{storeId}' already has this setup.");
        }

        if (result.Parts.Any(x => x.Status == McpSetupPartStatus.Failed))
        {
            throw new McpException(GetPartialFailureMessage(result));
        }

        return await CreatePreviewAsync(user, action, result, storeName, dryRun, warnings);
    }

    // Each section in the type of its part. A name no part takes stays in, without a section, so the setup service
    // names it in its refusal with every other error of the request.
    private async Task<IDictionary<string, object>> ReadSectionsAsync(Dictionary<string, JsonElement> setup)
    {
        if (setup == null || setup.Count == 0)
        {
            throw new McpException("setup is empty. Send one section per part; get_store_setup_options lists the parts.");
        }

        var sectionParts = setupService.GetParts().OfType<IStoreSetupSection>().ToList();
        var sections = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        var errors = new List<string>();

        foreach (var (name, json) in setup)
        {
            var part = sectionParts.FirstOrDefault(x => x.Name.EqualsIgnoreCase(name));

            if (part == null)
            {
                sections[name] = null;
                continue;
            }

            try
            {
                sections[part.Name] = json.ValueKind == JsonValueKind.Null ? null : json.Deserialize(part.SectionType, _sectionOptions);
            }
            catch (JsonException ex)
            {
                var path = string.IsNullOrEmpty(ex.Path) ? string.Empty : $" at {ex.Path}";
                errors.Add($"{part.Name}: the section does not match its schema{path}. get_store_setup_options shows it.");
            }
        }

        if (errors.Count > 0)
        {
            throw await CreateRefusalAsync(errors);
        }

        return sections;
    }

    // A part checks its own module's permission: to create, the part's Create permission; to change, its Update one.
    // The sections are keyed by part name, whatever its case (ReadSectionsAsync).
    private void EnsurePartPermissions(ClaimsPrincipal user, IDictionary<string, object> sections, string storeId, bool isNewStore)
    {
        foreach (var part in setupService.GetParts().Where(x => sections.ContainsKey(x.Name)))
        {
            var permission = isNewStore ? part.Permissions?.Create : part.Permissions?.Update;
            StoreAccess.EnsureAllowed(user, permission, part.Permissions?.ScopeType, storeId);
        }
    }

    // A warning when another store has the name the basics section sends. An unchanged name gets none.
    private async Task<IList<string>> GetSameNameWarningsAsync(IDictionary<string, object> sections, string storeId, string currentName = null)
    {
        var name = sections.Values.OfType<StoreBasicsSection>().FirstOrDefault()?.Name?.Trim();

        if (string.IsNullOrEmpty(name) || string.Equals(name, currentName, StringComparison.Ordinal))
        {
            return [];
        }

        // Every store the keyword finds: the one with the name may come after many whose names only hold it.
        var criteria = AbstractTypeFactory<StoreSearchCriteria>.TryCreateInstance();
        criteria.Keyword = name;
        var stores = await storeSearchService.SearchAllNoCloneAsync(criteria);

        return stores.Any(x => !x.Id.EqualsIgnoreCase(storeId) && x.Name.EqualsIgnoreCase(name))
            ? [$"Another store is already named '{name}'."]
            : [];
    }

    private async Task<StoreSetupPreview> CreatePreviewAsync(ClaimsPrincipal user, string action, StoreSetupResult result, string storeName, bool dryRun, IList<string> warnings)
    {
        var link = await links.GetStoresLinkAsync();
        // Before a create the store does not exist, so a missing section belongs in the same create call.
        var findings = StoreFinding.CreateList(result.Findings, setupService, link, inCreate: action == "create");
        // A part's findings show with its state; the others are counted, and still decide whether the store is ready.
        var (shown, notShown) = access.SplitReadableFindings(user, result.StoreId, findings);

        var preview = AbstractTypeFactory<StoreSetupPreview>.TryCreateInstance();
        preview.Action = action;
        preview.ObjectType = "store";
        preview.ObjectId = result.StoreId;
        preview.ObjectName = storeName ?? result.Store?.Name;
        preview.DryRun = dryRun;
        preview.Parts = result.Parts.Select(CreatePartRow).ToList();
        preview.MissingAfter = shown;
        preview.NotShown = notShown;
        preview.BackOffice = link;
        preview.NextStep = GetNextStep(action, dryRun, findings);

        foreach (var part in result.Parts)
        {
            foreach (var change in part.Changes)
            {
                preview.AddChange($"{part.Part}: {change.Field}", change.From, change.To);
            }
        }

        foreach (var warning in warnings)
        {
            preview.AddWarning(warning);
        }

        if (dryRun)
        {
            var (shownBlocking, notShownBlocking) = access.SplitReadableFindings(user, result.StoreId, findings.Where(x => x.BlocksGoLive));

            if (shownBlocking.Count + notShownBlocking.Count > 0)
            {
                var blocking = shownBlocking.Select(x => x.Title).Concat(notShownBlocking);
                preview.AddWarning($"The store will not be ready to open: {string.Join("; ", blocking)}.");
            }

            foreach (var finding in shown.Where(x => McpFindingSeverities.Warning.EqualsIgnoreCase(x.Severity)))
            {
                preview.AddWarning($"{finding.Title}.");
            }
        }
        else
        {
            // An applied change runs as its approver, and the requester reads its answer later (get_pending_change), so
            // it names no finding: they are counted by part, and get_store_readiness_report lists them for each reader.
            preview.MissingAfter = null;
            preview.NotShown = null;
            preview.MissingAfterByPart = CountByPart(findings);

            var blockingCount = findings.Count(x => x.BlocksGoLive);

            if (blockingCount > 0)
            {
                preview.AddWarning($"The store is not ready to open: {(blockingCount == 1 ? "1 finding blocks" : $"{blockingCount} findings block")} " +
                    "going live. get_store_readiness_report lists them.");
            }
        }

        foreach (var warning in result.Parts.SelectMany(x => x.Warnings))
        {
            preview.AddWarning(warning);
        }

        if (result.Store?.StoreState == StoreState.Open)
        {
            preview.AddWarning("The store is open: customers see each part as soon as it is applied.");
        }

        if (result.Parts.Count(x => x.Status is McpSetupPartStatus.WillChange or McpSetupPartStatus.Applied) > 1)
        {
            preview.AddWarning("Not a transaction: if a part fails, the parts before it stay applied; the answer says which, " +
                "and the same call repeated finishes the setup.");
        }

        return preview;
    }

    // A refusal names every error, then the Stores screen, where a person can also finish the setup: a part's error, such
    // as a catalog the store cannot change through a setup, names its screen without a link.
    private async Task<McpException> CreateRefusalAsync(IEnumerable<string> errors)
    {
        var link = await links.GetStoresLinkAsync();

        return new McpException($"The setup was refused, and nothing was changed. {string.Join(" ", errors)} Back office: {"Stores".WithLink(link)}.");
    }

    // "Tax providers: 2 findings, 1 blocking going live", one line per part with findings.
    private IList<string> CountByPart(IList<StoreFinding> findings)
    {
        var parts = setupService.GetParts();

        return findings
            .GroupBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
            .Select(x =>
            {
                var title = parts.FirstOrDefault(part => part.Name.EqualsIgnoreCase(x.Key))?.Title ?? x.Key;
                var count = x.Count() == 1 ? "1 finding" : $"{x.Count()} findings";
                var blocking = x.Count(finding => finding.BlocksGoLive);

                return blocking > 0 ? $"{title}: {count}, {blocking} blocking going live" : $"{title}: {count}";
            })
            .ToList();
    }

    private static StoreSetupPartRow CreatePartRow(McpSetupPartResult partResult)
    {
        var row = AbstractTypeFactory<StoreSetupPartRow>.TryCreateInstance();
        row.Part = partResult.Part;
        row.Status = partResult.Status.ToString().ToSnakeCase();
        row.Changes = partResult.Changes.Count;

        return row;
    }

    private static string GetNextStep(string action, bool dryRun, IList<StoreFinding> findings)
    {
        if (dryRun)
        {
            return "Show the user the changes and the warnings. A dry run is submitted by calling again without dryRun; the " +
                "change then waits in Agent approvals. When the store will not be ready, ask the user before submitting.";
        }

        if (findings.Any(x => x.BlocksGoLive))
        {
            return "Applied. The store is not ready to open yet: get_store_readiness_report says what to fix.";
        }

        return action == "create"
            ? "Applied; the store is closed. When the user is ready, open_store opens it after approval."
            : "Applied. get_store_readiness_report shows what the store still misses.";
    }

    private static string GetPartialFailureMessage(StoreSetupResult result)
    {
        var applied = result.Parts.Where(x => x.Status == McpSetupPartStatus.Applied).Select(x => x.Part).ToList();
        var notApplied = result.Parts
            .Where(x => x.Status is McpSetupPartStatus.Failed or McpSetupPartStatus.NotApplied)
            .Select(x => x.Status == McpSetupPartStatus.Failed ? $"{x.Part} (failed: {x.Error})" : $"{x.Part} (not started)");

        return $"Store '{result.StoreId}' is set up only in part. " +
            $"Applied: {(applied.Count == 0 ? "nothing" : string.Join(", ", applied))}. " +
            $"Not applied: {string.Join(", ", notApplied)}. " +
            "Nothing was rolled back. Repeat the same call to finish it: what is applied is not applied again. " +
            "Then check get_store_readiness_report.";
    }
}
