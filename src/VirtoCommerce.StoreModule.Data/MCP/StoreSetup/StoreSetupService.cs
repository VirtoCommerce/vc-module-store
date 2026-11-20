using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Data.MCP.StoreSetup;

public class StoreSetupService(
    IEnumerable<IStoreSetupPart> parts,
    IStoreService storeService,
    IStoreSetupFindings setupFindings,
    ILogger<StoreSetupService> logger)
    : IStoreSetupService
{
    // Store's own parts come first, basics before settings: the other parts attach to the store and read its planned
    // basics. The other parts do not depend on each other, so they follow by name, in an order no module has to choose.
    private static readonly string[] _storePartNames = [StoreBasicsSetupPart.PartName, StoreSettingsSetupPart.PartName];

    // One part per name. A module overrides a part by registering its subclass after the original, and the last
    // registration of a name wins, as for any service.
    public virtual IList<IStoreSetupPart> GetParts()
    {
        return parts
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Last())
            .OrderBy(x => GetStorePartIndex(x.Name))
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public virtual Task<IList<McpFinding>> CheckAsync(Store store, string currency = null, string country = null)
    {
        ArgumentNullException.ThrowIfNull(store);

        var context = AbstractTypeFactory<StoreSetupContext>.TryCreateInstance();
        context.Store = CopyStore(store);
        context.Currency = currency;
        context.Country = country;

        return RunChecksAsync(context);
    }

    // A critical finding blocks going live, whatever the case of its severity.
    public virtual bool BlocksGoLive(McpFinding finding)
    {
        ArgumentNullException.ThrowIfNull(finding);

        return McpFindingSeverities.Critical.EqualsIgnoreCase(finding.Severity);
    }

    public virtual async Task<StoreSetupResult> PlanAsync(StoreSetupRequest request)
    {
        var (result, context) = await PlanRequestAsync(request);

        // What the store would miss after the change.
        if (result.Errors.Count == 0)
        {
            result.Findings = await RunChecksAsync(context);
        }

        return result;
    }

    public virtual async Task<StoreSetupResult> ApplyAsync(StoreSetupRequest request)
    {
        var (result, context) = await PlanRequestAsync(request);

        // A request in which any part fails its checks changes nothing.
        if (result.Errors.Count > 0)
        {
            return result;
        }

        var sectionParts = GetParts().OfType<IStoreSetupSection>().ToList();
        var failed = false;

        foreach (var partResult in result.Parts.Where(x => x.Status == McpSetupPartStatus.WillChange))
        {
            if (failed)
            {
                partResult.Status = McpSetupPartStatus.NotApplied;
                continue;
            }

            var part = sectionParts.First(x => x.Name.EqualsIgnoreCase(partResult.Part));
            failed = !await ApplyPartAsync(context, part, partResult);
        }

        // What the store misses now, from the saved data. A new store whose basics failed does not exist.
        var savedStore = await storeService.GetByIdAsync(request.StoreId);
        result.Store = savedStore ?? result.Store;
        result.Findings = savedStore == null ? [] : await CheckAsync(savedStore);

        return result;
    }

    // Plans every section the request names, each seeing the plans before it. The readiness checks are not part of it:
    // they gate nothing, so a plan runs them on the planned store and an apply on the saved one.
    protected virtual async Task<(StoreSetupResult Result, StoreSetupContext Context)> PlanRequestAsync(StoreSetupRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = AbstractTypeFactory<StoreSetupResult>.TryCreateInstance();
        result.StoreId = request.StoreId;

        // The cached store: the parts get a copy of it (CopyStore).
        var store = await storeService.GetNoCloneAsync(request.StoreId);

        if (request.IsNewStore && store != null)
        {
            result.Errors.Add($"Store '{request.StoreId}' already exists.");
            return (result, null);
        }

        if (!request.IsNewStore && store == null)
        {
            result.Errors.Add($"Store '{request.StoreId}' was not found.");
            return (result, null);
        }

        var context = AbstractTypeFactory<StoreSetupContext>.TryCreateInstance();
        context.Store = store == null ? CreateNewStore(request.StoreId) : CopyStore(store);
        context.IsNewStore = request.IsNewStore;
        result.Store = context.Store;

        var allParts = GetParts();
        var sections = GetSections(result, request);
        AddRequestErrors(result, allParts, sections, request.IsNewStore);

        foreach (var part in allParts.OfType<IStoreSetupSection>())
        {
            if (sections.TryGetValue(part.Name, out var section))
            {
                await PlanPartAsync(result, context, part, section);
            }
        }

        // The store's row needs a catalog, which Store cannot create: the Catalog module's part plans it. Without that
        // part, a new store would fail only when it is saved, after the approval.
        if (request.IsNewStore && result.Errors.Count == 0 && string.IsNullOrEmpty(context.Store.Catalog))
        {
            result.Errors.Add("A new store needs a catalog, and no installed module creates one. Update the Catalog module: " +
                "its 'catalog' section creates the store's catalog.");
        }

        return (result, context);
    }

    // The request's sections keyed by part name whatever its case, whichever dictionary the caller passed. A name sent
    // twice, in two cases, is refused: neither of its sections would be the one applied.
    protected virtual IDictionary<string, object> GetSections(StoreSetupResult result, StoreSetupRequest request)
    {
        var sections = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var (name, section) in request.Sections ?? new Dictionary<string, object>())
        {
            if (!sections.TryAdd(name, section))
            {
                result.Errors.Add($"The '{name}' section is sent twice, in two cases of its name. Send it once.");
            }
        }

        return sections;
    }

    protected virtual void AddRequestErrors(StoreSetupResult result, IList<IStoreSetupPart> allParts, IDictionary<string, object> sections, bool isNewStore)
    {
        foreach (var name in sections.Keys)
        {
            var part = allParts.FirstOrDefault(x => x.Name.EqualsIgnoreCase(name));

            if (part == null)
            {
                result.Errors.Add($"No installed module takes a '{name}' section. get_store_setup_options lists the parts.");
            }
            else if (part is not IStoreSetupSection)
            {
                result.Errors.Add($"The '{part.Name}' part takes no section.");
            }
        }

        if (isNewStore)
        {
            foreach (var part in allParts.OfType<IStoreSetupSection>().Where(x => x.IsRequiredForNewStore && !sections.ContainsKey(x.Name)))
            {
                result.Errors.Add($"A new store needs the '{part.Name}' section.");
            }
        }
    }

    protected virtual async Task PlanPartAsync(StoreSetupResult result, StoreSetupContext context, IStoreSetupSection part, object section)
    {
        var partResult = AbstractTypeFactory<McpSetupPartResult>.TryCreateInstance();
        partResult.Part = part.Name;
        result.Parts.Add(partResult);

        if (section == null || !part.SectionType.IsInstanceOfType(section))
        {
            result.Errors.Add($"{part.Name}: the section is empty or does not match its schema. get_store_setup_options shows it.");
            return;
        }

        McpSetupPartPlan plan;

        try
        {
            plan = await part.PlanAsync(context, section);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The store setup part {Part} could not plan the store {StoreId}", part.Name, context.Store.Id);
            result.Errors.Add($"{part.Name}: {part.Title} could not be planned. Review the store in the back office, or try again later.");
            return;
        }

        context.Plans[part.Name] = plan;
        partResult.Changes = plan.Changes;
        partResult.Warnings = plan.Warnings;
        partResult.RequiredPermissions = plan.RequiredPermissions;
        partResult.Status = plan.Changes.Count > 0 ? McpSetupPartStatus.WillChange : McpSetupPartStatus.NoChange;

        foreach (var error in plan.Errors)
        {
            result.Errors.Add($"{part.Name}: {error}");
        }
    }

    // Applies one part's plan and says whether it was saved. A part that fails leaves the parts after it not applied.
    protected virtual async Task<bool> ApplyPartAsync(StoreSetupContext context, IStoreSetupSection part, McpSetupPartResult partResult)
    {
        try
        {
            await part.ApplyAsync(context, context.Plans[part.Name]);
            partResult.Status = McpSetupPartStatus.Applied;

            return true;
        }
        catch (Exception ex)
        {
            // The answer never carries an exception's text: it may hold a third party's trace or a secret.
            logger.LogError(ex, "The store setup part {Part} could not be applied to the store {StoreId}", part.Name, context.Store.Id);
            partResult.Status = McpSetupPartStatus.Failed;
            partResult.Error = $"{part.Title} could not be saved; the platform log has the details";

            return false;
        }
    }

    // A copy of the store for the parts: Store.Clone copies the settings and SEO infos but keeps the lists of the store it
    // copies, which may be the cached one, so the lists of codes and ids are copied too. A list that is null stays null:
    // a save leaves it alone.
    protected virtual Store CopyStore(Store store)
    {
        var copy = store.CloneTyped();
        copy.Languages = copy.Languages?.ToList();
        copy.Currencies = copy.Currencies?.ToList();
        copy.AdditionalFulfillmentCenterIds = copy.AdditionalFulfillmentCenterIds?.ToList();
        copy.ReturnsFulfillmentCenterIds = copy.ReturnsFulfillmentCenterIds?.ToList();
        copy.TrustedGroups = copy.TrustedGroups?.ToList();

        return copy;
    }

    protected virtual async Task<IList<McpFinding>> RunChecksAsync(StoreSetupContext context)
    {
        var findings = new List<McpFinding>();

        foreach (var check in GetParts().OfType<IStoreSetupCheck>())
        {
            try
            {
                // A finding belongs to the part that found it, whatever category it names: the part's read permission
                // decides who sees it.
                foreach (var finding in await check.CheckAsync(context) ?? [])
                {
                    finding.Category = check.Name;
                    findings.Add(finding);
                }
            }
            catch (Exception ex)
            {
                // The answer never carries an exception's text: it may hold a third party's trace or a secret.
                logger.LogError(ex, "The store setup part {Part} could not check the store {StoreId}", check.Name, context.Store.Id);
                findings.Add(CreateNotCheckedFinding(check, context.Store));
            }
        }

        return findings;
    }

    // A store that a setup creates is closed until open_store opens it.
    protected virtual Store CreateNewStore(string storeId)
    {
        var store = AbstractTypeFactory<Store>.TryCreateInstance();
        store.Id = storeId;
        store.StoreState = StoreState.Closed;
        store.Languages = [];
        store.Currencies = [];
        store.AdditionalFulfillmentCenterIds = [];
        store.ReturnsFulfillmentCenterIds = [];
        store.TrustedGroups = [];
        store.SeoInfos = [];
        store.DynamicProperties = [];
        store.Settings = [];

        return store;
    }

    protected virtual McpFinding CreateNotCheckedFinding(IStoreSetupPart part, Store store)
    {
        var finding = setupFindings.ForStore(store, part.Name, $"{part.Name}-not-checked", McpFindingSeverities.Critical);
        finding.Title = $"{part.Title} could not be checked";
        finding.Why = "A store goes live only when every part of its setup is checked.";
        finding.Evidence = "The check failed; the platform log has the details.";
        finding.Fix = "Review this part of the store in the back office, or check again later.";

        return finding;
    }

    protected virtual int GetStorePartIndex(string name)
    {
        var index = Array.FindIndex(_storePartNames, x => x.EqualsIgnoreCase(name));

        return index < 0 ? _storePartNames.Length : index;
    }
}
