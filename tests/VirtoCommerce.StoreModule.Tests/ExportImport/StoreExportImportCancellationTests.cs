using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.ExportImport;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests.ExportImport
{
    public class StoreExportImportCancellationTests
    {
        private readonly Mock<IStoreService> _storeServiceMock;
        private readonly Mock<IStoreSearchService> _storeSearchServiceMock;
        private readonly StoreExportImport _exportImport;

        public StoreExportImportCancellationTests()
        {
            _storeServiceMock = new Mock<IStoreService>();
            _storeSearchServiceMock = new Mock<IStoreSearchService>();

            _storeSearchServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<StoreSearchCriteria>(), It.IsAny<bool>()))
                .ReturnsAsync(new StoreSearchResult());

            _exportImport = new StoreExportImport(
                _storeServiceMock.Object,
                _storeSearchServiceMock.Object,
                JsonSerializer.CreateDefault());
        }

        [Fact]
        public async Task DoExportAsync_PreCancelledToken_ThrowsOperationCanceledException()
        {
            //Arrange
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            //Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _exportImport.DoExportAsync(Stream.Null, _ => { }, cts.Token));
        }

        [Fact]
        public async Task DoImportAsync_PreCancelledToken_ThrowsOperationCanceledException()
        {
            //Arrange
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            //Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _exportImport.DoImportAsync(Stream.Null, _ => { }, cts.Token));
        }
    }
}
