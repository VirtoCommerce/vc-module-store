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
    private const int SameNameSearchSize = 20;

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
        [Description("Id of the new store: letters, digits, '_' and '-'.")] string store_id,
        [Description("The setup: one section per part, keyed by the part name from get_store_setup_options, such as " +
            "basics, settings and payment.")] Dictionary<string, JsonElement> setup,
        bool dryRun = false)
    {
        if (string.IsNullOrWhiteSpace(store_id))
        {
            throw new McpException("store_id is required: the id of the new store.");
        }

        var user = context.User;
        var sections = ReadSections(setup);
        var warnings = new List<string>();
        var existingStore = await storeService.GetByIdAsync(store_id);
        var isNewStore = existingStore == null;

        if (!isNewStore)
        {
            // Only a store left closed by a failed create is continued; a store made by hand or by a create that
            // succeeded is changed with apply_store_setup.
            var failedChange = existingStore.StoreState == StoreState.Closed ? await leftovers.FindFailedCreateAsync(existingStore.Id) : null;

            if (failedChange == null)
            {
                throw new McpException($"Store '{existingStore.Id}' already exists. Change its setup with apply_store_setup.");
            }

            StoreAccess.EnsureAllowed(user, StorePermissions.Update, StoreAccess.StoreScopeType, existingStore.Id);
            warnings.Add($"Continues the setup of the closed store '{existingStore.Id}' that the failed change {failedChange.Id} left.");
        }

        EnsurePartPermissions(user, sections, store_id, isNewStore);
        warnings.AddRange(await GetSameNameWarningsAsync(sections, store_id));

        return await RunAsync("create", store_id, isNewStore, sections, dryRun, warnings);
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
        var sections = ReadSections(setup);
        EnsurePartPermissions(context.User, sections, store.Id, isNewStore: false);

        return await RunAsync("apply", store.Id, isNewStore: false, sections, dryRun, []);
    }

    private async Task<StoreSetupPreview> RunAsync(string action, string storeId, bool isNewStore, IDictionary<string, object> sections, bool dryRun, IList<string> warnings)
    {
        var request = AbstractTypeFactory<StoreSetupRequest>.TryCreateInstance();
        request.StoreId = storeId;
        request.IsNewStore = isNewStore;
        request.Sections = sections;

        var result = dryRun ? await setupService.PlanAsync(request) : await setupService.ApplyAsync(request);

        if (result.Errors.Count > 0)
        {
            throw new McpException($"The setup was refused, and nothing was changed. {string.Join(" ", result.Errors)}");
        }

        // An approval for nothing is not submitted.
        if (result.Parts.All(x => x.Status == StoreSetupPartStatus.NoChange))
        {
            throw new McpException($"Nothing to change: store '{storeId}' already has this setup.");
        }

        if (result.Parts.Any(x => x.Status == StoreSetupPartStatus.Failed))
        {
            throw new McpException(GetPartialFailureMessage(result));
        }

        return await CreatePreviewAsync(action, result, dryRun, warnings);
    }

    // Each section in the type of its part. A name no part takes stays in, without a section, so the setup service
    // names it in its refusal with every other error of the request.
    private IDictionary<string, object> ReadSections(Dictionary<string, JsonElement> setup)
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
            throw new McpException($"The setup was refused, and nothing was changed. {string.Join(" ", errors)}");
        }

        return sections;
    }

    // A part checks its own module's permission: to create, the part's Create permission; to change, its Update one.
    // The sections are keyed by part name, whatever its case (ReadSections).
    private void EnsurePartPermissions(ClaimsPrincipal user, IDictionary<string, object> sections, string storeId, bool isNewStore)
    {
        foreach (var part in setupService.GetParts().Where(x => sections.ContainsKey(x.Name)))
        {
            var permission = isNewStore ? part.Permissions?.Create : part.Permissions?.Update;
            StoreAccess.EnsureAllowed(user, permission, part.Permissions?.StoreScopeType, storeId);
        }
    }

    private async Task<IList<string>> GetSameNameWarningsAsync(IDictionary<string, object> sections, string storeId)
    {
        var name = sections.Values.OfType<StoreBasicsSection>().FirstOrDefault()?.Name?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            return [];
        }

        var criteria = AbstractTypeFactory<StoreSearchCriteria>.TryCreateInstance();
        criteria.Keyword = name;
        criteria.Take = SameNameSearchSize;
        var result = await storeSearchService.SearchAsync(criteria);

        return result.Results.Any(x => !x.Id.EqualsIgnoreCase(storeId) && x.Name.EqualsIgnoreCase(name))
            ? [$"Another store is already named '{name}'."]
            : [];
    }

    private async Task<StoreSetupPreview> CreatePreviewAsync(string action, StoreSetupResult result, bool dryRun, IList<string> warnings)
    {
        var link = await links.GetStoresLinkAsync();
        var findings = StoreFinding.CreateList(result.Findings, setupService.GetParts(), link);

        var preview = new StoreSetupPreview
        {
            Action = action,
            ObjectType = "store",
            ObjectId = result.StoreId,
            ObjectName = result.Store?.Name,
            DryRun = dryRun,
            Parts = result.Parts.Select(x => new StoreSetupPartRow { Part = x.Part, Status = x.Status.ToString().ToSnakeCase(), Changes = x.Changes.Count }).ToList(),
            MissingAfter = findings,
            BackOffice = link,
            NextStep = GetNextStep(action, dryRun, findings),
        };

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

        var blocking = findings.Where(x => x.BlocksGoLive).Select(x => x.Title).ToList();

        if (blocking.Count > 0)
        {
            preview.AddWarning($"The store will not be ready to open: {string.Join("; ", blocking)}.");
        }

        foreach (var finding in findings.Where(x => x.Severity == McpFindingSeverities.Warning))
        {
            preview.AddWarning($"{finding.Title}.");
        }

        foreach (var warning in result.Parts.SelectMany(x => x.Warnings))
        {
            preview.AddWarning(warning);
        }

        if (result.Store?.StoreState == StoreState.Open)
        {
            preview.AddWarning("The store is open: customers see each part as soon as it is applied.");
        }

        if (result.Parts.Count(x => x.Status is StoreSetupPartStatus.WillChange or StoreSetupPartStatus.Applied) > 1)
        {
            preview.AddWarning("Not a transaction: if a part fails, the parts before it stay applied; the answer says which, " +
                "and the same call repeated finishes the setup.");
        }

        return preview;
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
        var applied = result.Parts.Where(x => x.Status == StoreSetupPartStatus.Applied).Select(x => x.Part).ToList();
        var notApplied = result.Parts
            .Where(x => x.Status is StoreSetupPartStatus.Failed or StoreSetupPartStatus.NotApplied)
            .Select(x => x.Status == StoreSetupPartStatus.Failed ? $"{x.Part} (failed: {x.Error})" : $"{x.Part} (not started)");

        return $"Store '{result.StoreId}' is set up only in part. " +
            $"Applied: {(applied.Count == 0 ? "nothing" : string.Join(", ", applied))}. " +
            $"Not applied: {string.Join(", ", notApplied)}. " +
            "Nothing was rolled back. Repeat the same call to finish it: what is applied is not applied again. " +
            "Then check get_store_readiness_report.";
    }
}
