using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.NotificationsModule.Core.Model;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.StoreModule.Core;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Notifications;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.Authorization;
using VirtoCommerce.StoreModule.Web.Model;

namespace VirtoCommerce.StoreModule.Web.Controllers.Api
{
    [Route("api/stores")]
    [Authorize]
    public class StoreModuleController : Controller
    {
        private readonly IStoreService _storeService;
        private readonly ICrudService<Store> _storeCrudService;
        private readonly ISearchService<StoreSearchCriteria, StoreSearchResult, Store> _storeSearchService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly INotificationSearchService _notificationSearchService;
        private readonly INotificationSender _notificationSender;

        public StoreModuleController(
            IStoreService storeService,
            IStoreSearchService storeSearchService,
            UserManager<ApplicationUser> userManager,
            INotificationSearchService notificationSearchService,
            INotificationSender notificationSender,
            SignInManager<ApplicationUser> signInManager,
            IAuthorizationService authorizationService)
        {
            _storeService = storeService;
            _storeCrudService = (ICrudService<Store>)storeService;
            _storeSearchService = (ISearchService<StoreSearchCriteria, StoreSearchResult, Store>)storeSearchService;
            _userManager = userManager;
            _notificationSearchService = notificationSearchService;
            _notificationSender = notificationSender;
            _signInManager = signInManager;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Search stores
        /// </summary>
        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<StoreSearchResult>> SearchStores([FromBody] StoreSearchCriteria criteria)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, criteria, new StoreAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            if (string.IsNullOrEmpty(criteria.ResponseGroup))
            {
                criteria.ResponseGroup = StoreResponseGroup.StoreInfo.ToString();
            }
            var result = await _storeSearchService.SearchAsync(criteria);
            return result;
        }

        /// <summary>
        /// Get all stores
        /// </summary>
        [HttpGet]
        [Route("")]
        [Obsolete("Use POST api/stores/search instead")]
        public async Task<ActionResult<Store[]>> GetStores()
        {
            var criteria = new StoreSearchCriteria
            {
                Skip = 0,
                Take = int.MaxValue
            };

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, criteria, new StoreAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var result = await _storeSearchService.SearchAsync(criteria);
            return result.Stores.ToArray();
        }

        /// <summary>
        /// Get store by id
        /// </summary>
        /// <param name="id">Store id</param>
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Store>> GetStoreById(string id)
        {
            var store = await _storeService.GetByIdAsync(id, StoreResponseGroup.Full.ToString());

            if (store == null)
            {
                return null;
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, store, new StoreAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return Ok(store);
        }


        /// <summary>
        /// Create store
        /// </summary>
        /// <param name="store">Store</param>
        [HttpPost]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public async Task<ActionResult<Store>> CreateStore([FromBody] Store store)
        {
            await _storeCrudService.SaveChangesAsync(new[] { store });
            return Ok(store);
        }

        /// <summary>
        /// Update store
        /// </summary>
        /// <param name="store">Store</param>
        [HttpPut]
        [Route("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateStore([FromBody] Store store)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, store, new StoreAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            await _storeCrudService.SaveChangesAsync(new[] { store });
            return NoContent();
        }

        /// <summary>
        /// Delete stores
        /// </summary>
        /// <param name="ids">Ids of store that needed to delete</param>
        [HttpDelete]
        [Route("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteStore([FromQuery] string[] ids)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, ids, new StoreAuthorizationRequirement(ModuleConstants.Security.Permissions.Delete));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            await _storeCrudService.DeleteAsync(ids);
            return NoContent();
        }

        /// <summary>
        /// Send dynamic notification (contains custom list of properties) to store or administrator email 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("send/dynamicnotification")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> SendDynamicNotificationAnStoreEmail([FromBody] SendDynamicNotificationRequest request)
        {
            var store = await _storeCrudService.GetByIdAsync(request.StoreId);

            if (store == null)
            {
                throw new InvalidOperationException(string.Concat("Store not found. StoreId: ", request.StoreId));
            }

            if (string.IsNullOrEmpty(store.Email) && string.IsNullOrEmpty(store.AdminEmail))
            {
                throw new InvalidOperationException(string.Concat("Both store email and admin email are empty. StoreId: ", request.StoreId));
            }

            var notificationsSearchResult = await _notificationSearchService.SearchNotificationsAsync(
                new NotificationSearchCriteria()
                {
                    NotificationType = nameof(StoreDynamicEmailNotification),
                    TenantId = request.StoreId,
                    TenantType = nameof(Store),
                    IsActive = true
                }
                );
            if (notificationsSearchResult.TotalCount == 0)
            {
                throw new InvalidOperationException(string.Concat("There is no active notifications of type StoreDynamicEmailNotification. StoreId: ", request.StoreId));
            }

            var user = await _userManager.GetUserAsync(User);
            var notification = (StoreDynamicEmailNotification)notificationsSearchResult.Results.FirstOrDefault();
            notification.To = store.EmailWithName ?? store.AdminEmailWithName;
            notification.From = user.Email;
            notification.FormType = request.Type;
            notification.Fields = request.Fields;
            notification.LanguageCode = request.Language;

            await _notificationSender.SendNotificationAsync(notification);

            return NoContent();
        }

        /// <summary>
        /// Check if given contact has login on behalf permission
        /// </summary>
        /// <param name="storeId">Store ID</param>
        /// <param name="id">Contact ID</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{storeId}/accounts/{id}/loginonbehalf")]
        public async Task<ActionResult<LoginOnBehalfInfo>> GetLoginOnBehalfInfo(string storeId, string id)
        {
            var result = new LoginOnBehalfInfo
            {
                UserName = id
            };
            var store = await _storeCrudService.GetByIdAsync(storeId);
            if (store != null)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    var userPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
                    // VP-6462: Here we intentionally use inlined platform "platform:security:loginOnBehalf" permission to not create a dependency on the latest platform
                    var authorizationResult = await _authorizationService.AuthorizeAsync(userPrincipal, store, new StoreAuthorizationRequirement("platform:security:loginOnBehalf"));
                    result.CanLoginOnBehalf = authorizationResult.Succeeded;
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns list of stores which user can sign in
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("allowed/{userId}")]
        public async Task<ActionResult<Store[]>> GetUserAllowedStores(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var storeIds = await _storeService.GetUserAllowedStoreIdsAsync(user);
                var stores = await _storeCrudService.GetByIdsAsync(storeIds.ToArray(), StoreResponseGroup.StoreInfo.ToString());
                return Ok(stores);
            }

            return NotFound();
        }

    }
}
