using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.Services;

namespace VirtoCommerce.StoreModule.Tests
{
    public class StoreNotificationSenderUnitTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly string _token = "tokenWithSlashes/andPlusSigns+andDoubleEquals==";
        private readonly string _tokenEncoded = "tokenWithSlashes%2FandPlusSigns%2BandDoubleEquals%3D%3D";

        public StoreNotificationSenderUnitTests()
        {
            _mockUserManager = new Mock<UserManager<ApplicationUser>>();

        }

        // TODO: finish the draft, add other scenarios depending on User and Store
        //[Fact]
        //public async Task DeserializeObject_DeserializeExtendedSetting()
        //{
        //    //Arrange
        //    var sender = GetStoreNotificationSender();
        //    var user = GetUser();

        //    //Act
        //    var link = await sender.GenerateEmailVerificationLink(user, GetStore());

        //    //Assert
        //    Assert.NotNull(link);
        //    Assert.Contains(_tokenEncoded, link);
        //    Assert.Contains(_token, HttpUtility.UrlDecode(link));
        //}

        private ApplicationUser GetUser()
        {
            return new ApplicationUser
            {
                StoreId = "StoreId"
            };
        }

        private Store GetStore()
        {
            return new Store
            {
                Email = "",
                //Settings =
                Url = "https://store.virtocommerce.com/"
            };
        }

        private IStoreNotificationSender GetStoreNotificationSender()
        {
            _mockUserManager.Setup(ss => ss.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>())).Returns(Task.FromResult(_token));
            UserManager<ApplicationUser> factory() => _mockUserManager.Object;

            var result = new StoreNotificationSender(null, null, null, factory);
            return result;

        }
    }
}
