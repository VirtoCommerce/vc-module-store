using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Data.MCP.StoreSetup;

public class StoreSetupService(
    IEnumerable<IStoreSetupPart> parts,
    IStoreService storeService,
    ILogger<StoreSetupService> logger)
    : IStoreSetupService
{
    // Store's own parts come first, basics before settings: the other parts attach to the store and read its planned
    // basics. The other parts do not depend on each other, so they follow by name, in an order no module has to choose.
    private static readonly string[] _storePartNames = [StoreBasicsSetupPart.PartName, StoreSettingsSetupPart.PartName];

    public virtual IList<IStoreSetupPart> GetParts()
    {
        return parts
            .OrderBy(x => GetStorePartIndex(x.Name))
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public virtual Task<IList<StoreSetupFinding>> CheckAsync(Store store, string currency = null, string country = null)
    {
        ArgumentNullException.ThrowIfNull(store);

        var context = AbstractTypeFactory<StoreSetupContext>.TryCreateInstance();
        context.Store = store.CloneTyped();
        context.Currency = currency;
        context.Country = country;

        return RunChecksAsync(context);
    }

    public virtual async Task<StoreSetupResult> PlanAsync(StoreSetupRequest request)
    {
        var (result, _) = await PlanRequestAsync(request);

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

        foreach (var partResult in result.Parts.Where(x => x.Status == StoreSetupPartStatus.WillChange))
        {
            if (failed)
            {
                partResult.Status = StoreSetupPartStatus.NotApplied;
                continue;
            }

            var part = sectionParts.First(x => x.Name.EqualsIgnoreCase(partResult.Part));

            try
            {
                await part.ApplyAsync(context, context.Plans[part.Name]);
                partResult.Status = StoreSetupPartStatus.Applied;
            }
            catch (Exception ex)
            {
                // The answer never carries an exception's text: it may hold a third party's trace or a secret.
                logger.LogError(ex, "The store setup part {Part} could not be applied to the store {StoreId}", part.Name, request.StoreId);
                partResult.Status = StoreSetupPartStatus.Failed;
                partResult.Error = $"{part.Title} could not be saved; the platform log has the details";
                failed = true;
            }
        }

        // What the store misses now, from the saved data. A new store whose basics failed does not exist.
        var savedStore = await storeService.GetByIdAsync(request.StoreId);
        result.Store = savedStore ?? result.Store;
        result.Findings = savedStore == null ? [] : await CheckAsync(savedStore);

        return result;
    }

    // Plans every section the request names, each seeing the plans before it, then checks the store as it would be.
    protected virtual async Task<(StoreSetupResult Result, StoreSetupContext Context)> PlanRequestAsync(StoreSetupRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = AbstractTypeFactory<StoreSetupResult>.TryCreateInstance();
        result.StoreId = request.StoreId;
        result.IsNewStore = request.IsNewStore;

        var store = await storeService.GetByIdAsync(request.StoreId);

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
        context.Store = store ?? CreateNewStore(request.StoreId);
        context.IsNewStore = request.IsNewStore;
        result.Store = context.Store;

        var allParts = GetParts();
        var sections = request.Sections ?? new Dictionary<string, object>();
        AddRequestErrors(result, allParts, sections, request.IsNewStore);

        foreach (var part in allParts.OfType<IStoreSetupSection>())
        {
            var name = sections.Keys.FirstOrDefault(x => x.EqualsIgnoreCase(part.Name));

            if (name != null)
            {
                await PlanPartAsync(result, context, part, sections[name]);
            }
        }

        if (result.Errors.Count == 0)
        {
            result.Findings = await RunChecksAsync(context);
            result.CheckedParts = allParts.OfType<IStoreSetupCheck>().Select(x => x.Name).ToList();
        }

        return (result, context);
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
            foreach (var part in allParts.OfType<IStoreSetupSection>().Where(x => x.IsRequiredForNewStore && !sections.Keys.Any(name => name.EqualsIgnoreCase(x.Name))))
            {
                result.Errors.Add($"A new store needs the '{part.Name}' section.");
            }
        }
    }

    protected virtual async Task PlanPartAsync(StoreSetupResult result, StoreSetupContext context, IStoreSetupSection part, object section)
    {
        var partResult = AbstractTypeFactory<StoreSetupPartResult>.TryCreateInstance();
        partResult.Part = part.Name;
        partResult.Title = part.Title;
        result.Parts.Add(partResult);

        if (section == null || !part.SectionType.IsInstanceOfType(section))
        {
            result.Errors.Add($"{part.Name}: the section is empty or does not match its schema. get_store_setup_options shows it.");
            return;
        }

        StoreSetupPartPlan plan;

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
        partResult.Status = plan.Changes.Count > 0 ? StoreSetupPartStatus.WillChange : StoreSetupPartStatus.NoChange;

        foreach (var error in plan.Errors)
        {
            result.Errors.Add($"{part.Name}: {error}");
        }
    }

    protected virtual async Task<IList<StoreSetupFinding>> RunChecksAsync(StoreSetupContext context)
    {
        var findings = new List<StoreSetupFinding>();

        foreach (var check in GetParts().OfType<IStoreSetupCheck>())
        {
            try
            {
                findings.AddRange(await check.CheckAsync(context) ?? []);
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

    protected virtual StoreSetupFinding CreateNotCheckedFinding(IStoreSetupPart part, Store store)
    {
        var finding = AbstractTypeFactory<StoreSetupFinding>.TryCreateInstance();
        finding.Id = $"{part.Name}-not-checked";
        finding.Part = part.Name;
        finding.Severity = StoreSetupFindingSeverity.Blocking;
        finding.Title = $"{part.Title} could not be checked";
        finding.Why = "A store goes live only when every part of its setup is checked.";
        finding.Evidence = "The check failed; the platform log has the details.";
        finding.Fix = "Review this part of the store in the back office, or check again later.";
        finding.Screen = $"Stores > {store.Name ?? store.Id}";
        finding.ObjectType = nameof(Store);
        finding.ObjectId = store.Id;
        finding.ObjectName = store.Name;

        return finding;
    }

    private static int GetStorePartIndex(string name)
    {
        var index = Array.FindIndex(_storePartNames, x => x.EqualsIgnoreCase(name));

        return index < 0 ? _storePartNames.Length : index;
    }
}
