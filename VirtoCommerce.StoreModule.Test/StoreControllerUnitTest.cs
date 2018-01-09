using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Platform.Data.DynamicProperties;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.Platform.Data.Repositories;
using VirtoCommerce.StoreModule.Data.Repositories;
using VirtoCommerce.StoreModule.Data.Services;
using VirtoCommerce.StoreModule.Web.Controllers.Api;
using Xunit;
using Moq;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.StoreModule.Test
{
    public class StoreControllerUnitTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IStoreRepository> _mockStoreRepository;
        private readonly Mock<IPlatformRepository> _mockPlatformRepository;
        private readonly Mock<ICommerceService> _mockCommerceService;
        private readonly Mock<ISettingsManager> _mockSettingsManager;

        public StoreControllerUnitTest()
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
            var controller = GetStoreController();
            var store = new Store
            {
                Id = code,
                Name = "testStore",
                Catalog = "catalog",
                Currencies = new[] { "USD", "RUB" },
                DefaultCurrency = "USD",
                Languages = new[] { "ru-ru", "en-us" },
                DefaultLanguage = "ru-ru",
                FulfillmentCenter = new FulfillmentCenter
                {
                    City = "New York",
                    CountryCode = "USA",
                    Line1 = "line1",
                    DaytimePhoneNumber = "+821291921",
                    CountryName = "USA",
                    Name = "Name",
                    StateProvince = "State",
                    PostalCode = "code"
                },
                //PaymentGateways = new string[] { "PayPal", "Clarna" },
                StoreState = Domain.Store.Model.StoreState.Open,
            };

            Action act = () =>
            {
                var result = controller.Create(store) as OkNegotiatedContentResult<Store>;
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
            var controller = GetStoreController();
            var store = new Store
            {
                Id = code,
                Name = "testStore",
                Catalog = "catalog",
                Currencies = new[] { "USD", "RUB" },
                DefaultCurrency = "USD",
                Languages = new[] { "ru-ru", "en-us" },
                DefaultLanguage = "ru-ru",
                FulfillmentCenter = new FulfillmentCenter
                {
                    City = "New York",
                    CountryCode = "USA",
                    Line1 = "line1",
                    DaytimePhoneNumber = "+821291921",
                    CountryName = "USA",
                    Name = "Name",
                    StateProvince = "State",
                    PostalCode = "code"
                },
                //PaymentGateways = new string[] { "PayPal", "Clarna" },
                StoreState = Domain.Store.Model.StoreState.Open,
            };

            var result = controller.Create(store) as OkNegotiatedContentResult<Store>;

            Assert.NotNull(result.Content);
        }

        private StoreModuleController GetStoreController()
        {
            _mockStoreRepository.Setup(ss => ss.UnitOfWork).Returns(_mockUnitOfWork.Object);
            _mockPlatformRepository.Setup(ss => ss.UnitOfWork).Returns(_mockUnitOfWork.Object);
            
            Func<IPlatformRepository> platformRepositoryFactory = () => _mockPlatformRepository.Object;
            Func<IStoreRepository> repositoryFactory = () => _mockStoreRepository.Object;

            var dynamicPropertyService = new DynamicPropertyService(platformRepositoryFactory);

            var storeService = new StoreServiceImpl(repositoryFactory, _mockCommerceService.Object, _mockSettingsManager.Object, dynamicPropertyService, null, null, null);

            var controller = new StoreModuleController(storeService, null, null, null, null, null, null);
            return controller;
        }
    }
}
