using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// IStoreService over stores held in memory: it returns copies, as StoreService does, and saves copies as the platform
// saves a store. The store table refuses a store without a name or a catalog and texts longer than their columns, and the
// settings save skips a null for a setting without allowed values, which keeps the value it had.
public static class TestStoreService
{
    public static Mock<IStoreService> Create(List<Store> stores)
    {
        var storeService = new Mock<IStoreService>();

        storeService
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((IList<string> ids, string _, bool _) => stores.Where(x => ids.Any(id => id.EqualsIgnoreCase(x.Id))).Select(x => x.CloneTyped()).ToList());

        storeService
            .Setup(x => x.SaveChangesAsync(It.IsAny<IList<Store>>()))
            .Callback((IList<Store> models) =>
            {
                foreach (var model in models)
                {
                    EnsureTheTableTakesIt(model);

                    var saved = model.CloneTyped();
                    saved.Settings = SaveSettings(saved.Settings, stores.FirstOrDefault(x => x.Id.EqualsIgnoreCase(model.Id))?.Settings);

                    stores.RemoveAll(x => x.Id.EqualsIgnoreCase(model.Id));
                    stores.Add(saved);
                }
            })
            .Returns(Task.CompletedTask);

        return storeService;
    }

    private static void EnsureTheTableTakesIt(Store store)
    {
        var columns = new (string Name, string Value, int MaxLength, bool Required)[]
        {
            (nameof(Store.Name), store.Name, 128, true),
            (nameof(Store.Catalog), store.Catalog, 128, true),
            (nameof(Store.Description), store.Description, 256, false),
            (nameof(Store.Url), store.Url, 256, false),
            (nameof(Store.SecureUrl), store.SecureUrl, 128, false),
            (nameof(Store.Email), store.Email, 128, false),
            (nameof(Store.AdminEmail), store.AdminEmail, 128, false),
            (nameof(Store.TimeZone), store.TimeZone, 128, false),
            (nameof(Store.Country), store.Country, 128, false),
            (nameof(Store.Region), store.Region, 128, false),
            (nameof(Store.MainFulfillmentCenterId), store.MainFulfillmentCenterId, 128, false),
        };

        foreach (var (name, value, maxLength, required) in columns.Where(x => (x.Required && string.IsNullOrEmpty(x.Value)) || x.Value?.Length > x.MaxLength))
        {
            throw new InvalidOperationException($"The store table refuses the store's {name}: '{value}'.");
        }
    }

    private static List<ObjectSettingEntry> SaveSettings(ICollection<ObjectSettingEntry> settings, ICollection<ObjectSettingEntry> saved)
    {
        return settings?
            .Select(x => x.Value == null && x.AllowedValues.IsNullOrEmpty() ? saved?.FirstOrDefault(y => y.Name.EqualsIgnoreCase(x.Name)) : x)
            .Where(x => x != null)
            .ToList();
    }
}
