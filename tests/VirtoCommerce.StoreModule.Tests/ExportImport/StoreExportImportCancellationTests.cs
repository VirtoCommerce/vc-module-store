using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;
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

        [Fact]
#pragma warning disable VC0014
        public async Task DoExportAsync_LegacyOverload_DropsCancellation()
#pragma warning restore VC0014
        {
            //Arrange — mock token that would throw if consulted
            var mockToken = new Mock<ICancellationToken>();
            mockToken.Setup(t => t.ThrowIfCancellationRequested())
                .Throws<OperationCanceledException>();

            //Act
            using var outStream = new MemoryStream();
#pragma warning disable VC0014
            await _exportImport.DoExportAsync(outStream, _ => { }, mockToken.Object);
#pragma warning restore VC0014

            //Assert — shim delegates to CancellationToken.None, mock token never consulted
            mockToken.Verify(t => t.ThrowIfCancellationRequested(), Times.Never);
        }

        [Fact]
#pragma warning disable VC0014
        public async Task DoImportAsync_LegacyOverload_DropsCancellation()
#pragma warning restore VC0014
        {
            //Arrange — mock token that would throw if consulted
            var mockToken = new Mock<ICancellationToken>();
            mockToken.Setup(t => t.ThrowIfCancellationRequested())
                .Throws<OperationCanceledException>();

            //Act
            using var inputStream = new MemoryStream();
#pragma warning disable VC0014
            await _exportImport.DoImportAsync(inputStream, _ => { }, mockToken.Object);
#pragma warning restore VC0014

            //Assert — shim delegates to CancellationToken.None, mock token never consulted
            mockToken.Verify(t => t.ThrowIfCancellationRequested(), Times.Never);
        }
    }
}
