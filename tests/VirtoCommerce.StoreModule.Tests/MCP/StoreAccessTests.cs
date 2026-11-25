using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.MCP.Services;
using Xunit;
using static VirtoCommerce.StoreModule.Tests.MCP.StoreToolsTestContext;

namespace VirtoCommerce.StoreModule.Tests.MCP;

public class StoreAccessTests
{
    // Ids the criteria name already are narrowed to the stores the user may read, not replaced by them.
    [Fact]
    public async Task SearchReadableStoresAsync_NarrowsTheIdsTheCriteriaName()
    {
        //Arrange
        var access = new StoreAccess(Mock.Of<IStoreService>(), new InMemoryStoreSearchService(CreateStores()), Mock.Of<IStoreSetupService>());
        var storeAAdministrator = StoreAAdministrator();

        //Act
        var both = await access.SearchReadableStoresAsync(storeAAdministrator, new StoreSearchCriteria { ObjectIds = [StoreA, StoreB], Take = 10 });
        var otherOnly = await access.SearchReadableStoresAsync(storeAAdministrator, new StoreSearchCriteria { ObjectIds = [StoreB], Take = 10 });

        //Assert
        Assert.Equal([StoreA], both.Results.Select(x => x.Id));
        Assert.Empty(otherOnly.Results);
    }
}
