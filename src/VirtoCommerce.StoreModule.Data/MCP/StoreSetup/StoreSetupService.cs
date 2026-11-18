using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Data.MCP.StoreSetup;

public class StoreSetupService(IEnumerable<IStoreSetupPart> parts, ILogger<StoreSetupService> logger) : IStoreSetupService
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

    public virtual async Task<IList<StoreSetupFinding>> CheckAsync(Store store, string currency = null, string country = null)
    {
        ArgumentNullException.ThrowIfNull(store);

        var context = AbstractTypeFactory<StoreSetupContext>.TryCreateInstance();
        context.Store = store.CloneTyped();
        context.Currency = currency;
        context.Country = country;

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
                logger.LogError(ex, "The store setup part {Part} could not check the store {StoreId}", check.Name, store.Id);
                findings.Add(CreateNotCheckedFinding(check, store));
            }
        }

        return findings;
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
