using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.NotificationsModule.Core.Model;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
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
    public class StoreModuleController(
        IStoreService storeService,
        IStoreSearchService storeSearchService,
        UserManager<ApplicationUser> userManager,
        INotificationSearchService notificationSearchService,
        INotificationSender notificationSender,
        SignInManager<ApplicationUser> signInManager,
        IAuthorizationService authorizationService,
        IPublicStoreSettings publicStoreSettings
        ) : Controller
    {
        /// <summary>
        /// Search stores
        /// </summary>
        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<StoreSearchResult>> SearchStores([FromBody] StoreSearchCriteria criteria)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(User, criteria, new StoreAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            if (string.IsNullOrEmpty(criteria.ResponseGroup))
            {
                criteria.ResponseGroup = nameof(StoreResponseGroup.StoreInfo);
            }
            var result = await storeSearchService.SearchNoCloneAsync(criteria);
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

            var authorizationResult = await authorizationService.AuthorizeAsync(User, criteria, new StoreAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var result = await storeSearchService.SearchNoCloneAsync(criteria);
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
            var store = await storeService.GetNoCloneAsync(id, nameof(StoreResponseGroup.Full));

            if (store == null)
            {
                return null;
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, store, new StoreAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return Ok(store);
        }

        /// <summary>
        /// Gets store by outer id.
        /// </summary>
        /// <remarks>Gets store by outer id (integration key) with full information loaded</remarks>
        /// <param name="outerId">Store outer id</param>
        [HttpGet]
        [Route("outer/{outerId}")]
        public async Task<ActionResult<Store>> GetStoreByOuterId(string outerId)
        {
            var store = await storeService.GetByOuterIdNoCloneAsync(outerId, nameof(StoreResponseGroup.Full));
            if (store == null)
            {
                return NotFound();
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, store, new StoreAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
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
            await storeService.SaveChangesAsync([store]);
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
            var authorizationResult = await authorizationService.AuthorizeAsync(User, store, new StoreAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            await storeService.SaveChangesAsync([store]);
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
            var authorizationResult = await authorizationService.AuthorizeAsync(User, ids, new StoreAuthorizationRequirement(ModuleConstants.Security.Permissions.Delete));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            await storeService.DeleteAsync(ids);
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
        public async Task<ActionResult> SendDynamicNotificationToStoreEmail([FromBody] SendDynamicNotificationRequest request)
        {
            var store = await storeService.GetNoCloneAsync(request.StoreId);

            if (store == null)
            {
                throw new InvalidOperationException(string.Concat("Store not found. StoreId: ", request.StoreId));
            }

            if (string.IsNullOrEmpty(store.Email) && string.IsNullOrEmpty(store.AdminEmail))
            {
                throw new InvalidOperationException(string.Concat("Both store email and admin email are empty. StoreId: ", request.StoreId));
            }

            var notificationsSearchResult = await notificationSearchService.SearchNotificationsAsync(
                new NotificationSearchCriteria
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

            var user = await userManager.GetUserAsync(User);
            var notification = (StoreDynamicEmailNotification)notificationsSearchResult.Results.FirstOrDefault();
            if (notification != null)
            {
                notification.To = store.EmailWithName ?? store.AdminEmailWithName;
                notification.From = user!.Email;
                notification.FormType = request.Type;
                notification.Fields = request.Fields;
                notification.LanguageCode = request.Language;

                await notificationSender.SendNotificationAsync(notification);
            }

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
            var store = await storeService.GetNoCloneAsync(storeId);
            if (store != null)
            {
                var user = await userManager.FindByIdAsync(id);
                if (user != null)
                {
                    var userPrincipal = await signInManager.CreateUserPrincipalAsync(user);
                    // VP-6462: Here we intentionally use inlined platform "platform:security:loginOnBehalf" permission to not create a dependency on the latest platform
                    var authorizationResult = await authorizationService.AuthorizeAsync(userPrincipal, store, new StoreAuthorizationRequirement("platform:security:loginOnBehalf"));
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
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var storeIds = await storeService.GetUserAllowedStoreIdsAsync(user);
                var stores = await storeService.GetNoCloneAsync(storeIds, nameof(StoreResponseGroup.StoreInfo));
                return Ok(stores);
            }

            return NotFound();
        }

        [HttpGet]
        [Route("{id}/public-settings")]
        [AllowAnonymous]
        public async Task<ActionResult<ModulePublicStoreSettings[]>> GetStorePublicSettingsById(string id)
        {
            var settings = await publicStoreSettings.GetSettings(id);
            return settings.ToArray();
        }

        /// <summary>
        /// Partial update for the specified store by id
        /// </summary>
        /// <param name="id">Store id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        [HttpPatch]
        [Route("{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatchStore(string id, [FromBody] JsonPatchDocument<Store> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var store = await storeService.GetByIdAsync(id);
            if (store == null)
            {
                return NotFound();
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, store, new StoreAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            patchDocument.ApplyTo(store, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await storeService.SaveChangesAsync([store]);

            return NoContent();
        }
    }
}
