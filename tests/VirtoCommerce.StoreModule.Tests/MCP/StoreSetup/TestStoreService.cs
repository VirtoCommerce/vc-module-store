using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// IStoreService over stores held in memory: it returns copies, as StoreService does, and saves copies.
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
                    stores.RemoveAll(x => x.Id.EqualsIgnoreCase(model.Id));
                    stores.Add(model.CloneTyped());
                }
            })
            .Returns(Task.CompletedTask);

        return storeService;
    }
}
