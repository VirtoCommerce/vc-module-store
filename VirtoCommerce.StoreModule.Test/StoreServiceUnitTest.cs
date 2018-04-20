using System;
using Moq;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.DynamicProperties;
using VirtoCommerce.Platform.Data.Repositories;
using VirtoCommerce.StoreModule.Data.Repositories;
using VirtoCommerce.StoreModule.Data.Services;
using Xunit;

namespace VirtoCommerce.StoreModule.Test
{
    public class StoreServiceUnitTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IStoreRepository> _mockStoreRepository;
        private readonly Mock<IPlatformRepository> _mockPlatformRepository;
        private readonly Mock<ICommerceService> _mockCommerceService;
        private readonly Mock<ISettingsManager> _mockSettingsManager;

        public StoreServiceUnitTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockStoreRepository = new Mock<IStoreRepository>();
            _mockPlatformRepository = new Mock<IPlatformRepository>();
            _mockCommerceService = new Mock<ICommerceService>();
            _mockSettingsManager = new Mock<ISettingsManager>();
        }

        [CLSCompliant(false)]
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
                StoreState = Domain.Store.Model.StoreState.Open,
            };

            Action act = () =>
            {
                var result = service.Create(store);
            };

            Assert.Throws<FluentValidation.ValidationException>(act);
        }

        [CLSCompliant(false)]
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
                StoreState = Domain.Store.Model.StoreState.Open,
            };

            var result = service.Create(store);

            Assert.NotNull(result);
        }

        private IStoreService GetStoreService()
        {
            _mockStoreRepository.Setup(ss => ss.UnitOfWork).Returns(_mockUnitOfWork.Object);
            _mockPlatformRepository.Setup(ss => ss.UnitOfWork).Returns(_mockUnitOfWork.Object);

            IPlatformRepository platformRepositoryFactory() => _mockPlatformRepository.Object;
            IStoreRepository repositoryFactory() => _mockStoreRepository.Object;

            var dynamicPropertyService = new DynamicPropertyService(platformRepositoryFactory);

            var storeService = new StoreServiceImpl(repositoryFactory, _mockCommerceService.Object, _mockSettingsManager.Object, dynamicPropertyService, null, null, null);
            return storeService;
          
        }
    }
}
