using Microsoft.Extensions.Caching.Memory;
using Moq;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Repositories;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.Repositories;
using VirtoCommerce.StoreModule.Data.Services;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests
{
    public class StoreServiceUnitTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IStoreRepository> _mockStoreRepository;
        private readonly Mock<IPlatformRepository> _mockPlatformRepository;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<IPlatformMemoryCache> _platformMemoryCacheMock;
        private readonly Mock<ICacheEntry> _cacheEntryMock;
        private readonly Mock<ISettingsManager> _mockSettingsManager;

        public StoreServiceUnitTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockStoreRepository = new Mock<IStoreRepository>();
            _mockPlatformRepository = new Mock<IPlatformRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockStoreRepository.Setup(ss => ss.UnitOfWork).Returns(_mockUnitOfWork.Object);
            _mockSettingsManager = new Mock<ISettingsManager>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _platformMemoryCacheMock = new Mock<IPlatformMemoryCache>();
            _cacheEntryMock = new Mock<ICacheEntry>();
        }

        [Theory]
        [InlineData("test Store")]
        [InlineData("!@#$%^&*()")]
        [InlineData("TestCode!")]
        [InlineData("Test@Code")]
        [InlineData("TestCode123 ")]
        [InlineData("TestCode 123")]
        [InlineData("TestCode 123!!!")]
        [InlineData("TestCode 1 2 3 !!!")]
        [InlineData("$$TestCode$$$")]
        public virtual void CanTryCreateNewStoreWithInvalidCode_ThrowsValidationException(string code)
        {
            var service = GetStoreService();
            var store = new Store
            {
                Id = code,
                Name = "testStore",
                Catalog = "catalog",
                Currencies = new[] { "USD", "RUB" },
                DefaultCurrency = "USD",
                Languages = new[] { "ru-ru", "en-us" },
                DefaultLanguage = "ru-ru",
                MainFulfillmentCenterId = "center",
                //PaymentGateways = new string[] { "PayPal", "Clarna" },
                StoreState = StoreState.Open,
            };

            var cacheKey = CacheKey.With(service.GetType(), "GetByIdsAsync", string.Join("-", code), null);
            _platformMemoryCacheMock.Setup(pmc => pmc.CreateEntry(cacheKey)).Returns(_cacheEntryMock.Object);

            var act = () =>
            {
                return service.SaveChangesAsync(new[] { store });
            };

            Assert.ThrowsAsync<FluentValidation.ValidationException>(act);
        }

        [Theory]
        [InlineData("test_Store1")]
        [InlineData("TestCode123")]
        [InlineData("testcode")]
        [InlineData("test_code")]
        [InlineData("test1code1")]
        [InlineData("1test1code1")]
        public virtual void CanTryCreateNewStoreWithValidCode(string code)
        {
            var service = GetStoreService();
            var store = new Store
            {
                Id = code,
                Name = "testStore",
                Catalog = "catalog",
                Currencies = new[] { "USD", "RUB" },
                DefaultCurrency = "USD",
                Languages = new[] { "ru-ru", "en-us" },
                DefaultLanguage = "ru-ru",
                MainFulfillmentCenterId = "center",
                StoreState = StoreState.Open,
            };

            var cacheKey = CacheKey.With(service.GetType(), "GetByIdsAsync", string.Join("-", code), null);
            _platformMemoryCacheMock.Setup(pmc => pmc.CreateEntry(cacheKey)).Returns(_cacheEntryMock.Object);

            service.SaveChangesAsync(new[] { store }).GetAwaiter().GetResult();
        }

        private IStoreService GetStoreService()
        {
            _mockStoreRepository.Setup(ss => ss.UnitOfWork).Returns(_mockUnitOfWork.Object);
            _mockPlatformRepository.Setup(ss => ss.UnitOfWork).Returns(_mockUnitOfWork.Object);

            var factory = () => _mockStoreRepository.Object;

            var storeService = new StoreService(factory, _platformMemoryCacheMock.Object, _eventPublisherMock.Object, _mockSettingsManager.Object);
            return storeService;
        }
    }
}
