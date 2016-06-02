using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RestSharp;
using VirtoCommerce.StoreModule.Client.Client;
using VirtoCommerce.StoreModule.Client.Model;

namespace VirtoCommerce.StoreModule.Client.Api
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IVirtoCommerceStoreApi : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Create store
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns>Store</returns>
        Store StoreModuleCreate(Store store);

        /// <summary>
        /// Create store
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns>ApiResponse of Store</returns>
        ApiResponse<Store> StoreModuleCreateWithHttpInfo(Store store);
        /// <summary>
        /// Delete stores
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Ids of store that needed to delete</param>
        /// <returns></returns>
        void StoreModuleDelete(List<string> ids);

        /// <summary>
        /// Delete stores
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Ids of store that needed to delete</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> StoreModuleDeleteWithHttpInfo(List<string> ids);
        /// <summary>
        /// Check if given contact has login on behalf permission
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="storeId">Store ID</param>
        /// <param name="id">Contact ID</param>
        /// <returns>LoginOnBehalfInfo</returns>
        LoginOnBehalfInfo StoreModuleGetLoginOnBehalfInfo(string storeId, string id);

        /// <summary>
        /// Check if given contact has login on behalf permission
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="storeId">Store ID</param>
        /// <param name="id">Contact ID</param>
        /// <returns>ApiResponse of LoginOnBehalfInfo</returns>
        ApiResponse<LoginOnBehalfInfo> StoreModuleGetLoginOnBehalfInfoWithHttpInfo(string storeId, string id);
        /// <summary>
        /// Get store by id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Store id</param>
        /// <returns>Store</returns>
        Store StoreModuleGetStoreById(string id);

        /// <summary>
        /// Get store by id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Store id</param>
        /// <returns>ApiResponse of Store</returns>
        ApiResponse<Store> StoreModuleGetStoreByIdWithHttpInfo(string id);
        /// <summary>
        /// Get all stores
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;Store&gt;</returns>
        List<Store> StoreModuleGetStores();

        /// <summary>
        /// Get all stores
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of List&lt;Store&gt;</returns>
        ApiResponse<List<Store>> StoreModuleGetStoresWithHttpInfo();
        /// <summary>
        /// Returns list of stores which user can sign in
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="userId"></param>
        /// <returns>List&lt;Store&gt;</returns>
        List<Store> StoreModuleGetUserAllowedStores(string userId);

        /// <summary>
        /// Returns list of stores which user can sign in
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="userId"></param>
        /// <returns>ApiResponse of List&lt;Store&gt;</returns>
        ApiResponse<List<Store>> StoreModuleGetUserAllowedStoresWithHttpInfo(string userId);
        /// <summary>
        /// Search stores
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria"></param>
        /// <returns>SearchResult</returns>
        SearchResult StoreModuleSearchStores(SearchCriteria criteria);

        /// <summary>
        /// Search stores
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria"></param>
        /// <returns>ApiResponse of SearchResult</returns>
        ApiResponse<SearchResult> StoreModuleSearchStoresWithHttpInfo(SearchCriteria criteria);
        /// <summary>
        /// Send dynamic notification (contains custom list of properties) an store or adminsitrator email
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="request"></param>
        /// <returns></returns>
        void StoreModuleSendDynamicNotificationAnStoreEmail(SendDynamicNotificationRequest request);

        /// <summary>
        /// Send dynamic notification (contains custom list of properties) an store or adminsitrator email
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="request"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> StoreModuleSendDynamicNotificationAnStoreEmailWithHttpInfo(SendDynamicNotificationRequest request);
        /// <summary>
        /// Update store
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns></returns>
        void StoreModuleUpdate(Store store);

        /// <summary>
        /// Update store
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> StoreModuleUpdateWithHttpInfo(Store store);
        #endregion Synchronous Operations
        #region Asynchronous Operations
        /// <summary>
        /// Create store
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns>Task of Store</returns>
        System.Threading.Tasks.Task<Store> StoreModuleCreateAsync(Store store);

        /// <summary>
        /// Create store
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns>Task of ApiResponse (Store)</returns>
        System.Threading.Tasks.Task<ApiResponse<Store>> StoreModuleCreateAsyncWithHttpInfo(Store store);
        /// <summary>
        /// Delete stores
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Ids of store that needed to delete</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task StoreModuleDeleteAsync(List<string> ids);

        /// <summary>
        /// Delete stores
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Ids of store that needed to delete</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> StoreModuleDeleteAsyncWithHttpInfo(List<string> ids);
        /// <summary>
        /// Check if given contact has login on behalf permission
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="storeId">Store ID</param>
        /// <param name="id">Contact ID</param>
        /// <returns>Task of LoginOnBehalfInfo</returns>
        System.Threading.Tasks.Task<LoginOnBehalfInfo> StoreModuleGetLoginOnBehalfInfoAsync(string storeId, string id);

        /// <summary>
        /// Check if given contact has login on behalf permission
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="storeId">Store ID</param>
        /// <param name="id">Contact ID</param>
        /// <returns>Task of ApiResponse (LoginOnBehalfInfo)</returns>
        System.Threading.Tasks.Task<ApiResponse<LoginOnBehalfInfo>> StoreModuleGetLoginOnBehalfInfoAsyncWithHttpInfo(string storeId, string id);
        /// <summary>
        /// Get store by id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Store id</param>
        /// <returns>Task of Store</returns>
        System.Threading.Tasks.Task<Store> StoreModuleGetStoreByIdAsync(string id);

        /// <summary>
        /// Get store by id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Store id</param>
        /// <returns>Task of ApiResponse (Store)</returns>
        System.Threading.Tasks.Task<ApiResponse<Store>> StoreModuleGetStoreByIdAsyncWithHttpInfo(string id);
        /// <summary>
        /// Get all stores
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of List&lt;Store&gt;</returns>
        System.Threading.Tasks.Task<List<Store>> StoreModuleGetStoresAsync();

        /// <summary>
        /// Get all stores
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (List&lt;Store&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<Store>>> StoreModuleGetStoresAsyncWithHttpInfo();
        /// <summary>
        /// Returns list of stores which user can sign in
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="userId"></param>
        /// <returns>Task of List&lt;Store&gt;</returns>
        System.Threading.Tasks.Task<List<Store>> StoreModuleGetUserAllowedStoresAsync(string userId);

        /// <summary>
        /// Returns list of stores which user can sign in
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="userId"></param>
        /// <returns>Task of ApiResponse (List&lt;Store&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<Store>>> StoreModuleGetUserAllowedStoresAsyncWithHttpInfo(string userId);
        /// <summary>
        /// Search stores
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria"></param>
        /// <returns>Task of SearchResult</returns>
        System.Threading.Tasks.Task<SearchResult> StoreModuleSearchStoresAsync(SearchCriteria criteria);

        /// <summary>
        /// Search stores
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria"></param>
        /// <returns>Task of ApiResponse (SearchResult)</returns>
        System.Threading.Tasks.Task<ApiResponse<SearchResult>> StoreModuleSearchStoresAsyncWithHttpInfo(SearchCriteria criteria);
        /// <summary>
        /// Send dynamic notification (contains custom list of properties) an store or adminsitrator email
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="request"></param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task StoreModuleSendDynamicNotificationAnStoreEmailAsync(SendDynamicNotificationRequest request);

        /// <summary>
        /// Send dynamic notification (contains custom list of properties) an store or adminsitrator email
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="request"></param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> StoreModuleSendDynamicNotificationAnStoreEmailAsyncWithHttpInfo(SendDynamicNotificationRequest request);
        /// <summary>
        /// Update store
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task StoreModuleUpdateAsync(Store store);

        /// <summary>
        /// Update store
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> StoreModuleUpdateAsyncWithHttpInfo(Store store);
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public class VirtoCommerceStoreApi : IVirtoCommerceStoreApi
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtoCommerceStoreApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="apiClient">An instance of ApiClient.</param>
        /// <returns></returns>
        public VirtoCommerceStoreApi(ApiClient apiClient)
        {
            ApiClient = apiClient;
            Configuration = apiClient.Configuration;
        }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public string GetBasePath()
        {
            return ApiClient.RestClient.BaseUrl.ToString();
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public Configuration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the API client object
        /// </summary>
        /// <value>An instance of the ApiClient</value>
        public ApiClient ApiClient { get; set; }

        /// <summary>
        /// Create store 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns>Store</returns>
        public Store StoreModuleCreate(Store store)
        {
             ApiResponse<Store> localVarResponse = StoreModuleCreateWithHttpInfo(store);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Create store 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns>ApiResponse of Store</returns>
        public ApiResponse<Store> StoreModuleCreateWithHttpInfo(Store store)
        {
            // verify the required parameter 'store' is set
            if (store == null)
                throw new ApiException(400, "Missing required parameter 'store' when calling VirtoCommerceStoreApi->StoreModuleCreate");

            var localVarPath = "/api/stores";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (store.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(store); // http body (model) parameter
            }
            else
            {
                localVarPostBody = store; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleCreate: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleCreate: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Store>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Store)ApiClient.Deserialize(localVarResponse, typeof(Store)));
            
        }

        /// <summary>
        /// Create store 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns>Task of Store</returns>
        public async System.Threading.Tasks.Task<Store> StoreModuleCreateAsync(Store store)
        {
             ApiResponse<Store> localVarResponse = await StoreModuleCreateAsyncWithHttpInfo(store);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Create store 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns>Task of ApiResponse (Store)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Store>> StoreModuleCreateAsyncWithHttpInfo(Store store)
        {
            // verify the required parameter 'store' is set
            if (store == null)
                throw new ApiException(400, "Missing required parameter 'store' when calling VirtoCommerceStoreApi->StoreModuleCreate");

            var localVarPath = "/api/stores";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (store.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(store); // http body (model) parameter
            }
            else
            {
                localVarPostBody = store; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleCreate: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleCreate: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Store>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Store)ApiClient.Deserialize(localVarResponse, typeof(Store)));
            
        }
        /// <summary>
        /// Delete stores 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Ids of store that needed to delete</param>
        /// <returns></returns>
        public void StoreModuleDelete(List<string> ids)
        {
             StoreModuleDeleteWithHttpInfo(ids);
        }

        /// <summary>
        /// Delete stores 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Ids of store that needed to delete</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> StoreModuleDeleteWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceStoreApi->StoreModuleDelete");

            var localVarPath = "/api/stores";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleDelete: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleDelete: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Delete stores 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Ids of store that needed to delete</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task StoreModuleDeleteAsync(List<string> ids)
        {
             await StoreModuleDeleteAsyncWithHttpInfo(ids);

        }

        /// <summary>
        /// Delete stores 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">Ids of store that needed to delete</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> StoreModuleDeleteAsyncWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceStoreApi->StoreModuleDelete");

            var localVarPath = "/api/stores";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleDelete: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleDelete: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Check if given contact has login on behalf permission 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="storeId">Store ID</param>
        /// <param name="id">Contact ID</param>
        /// <returns>LoginOnBehalfInfo</returns>
        public LoginOnBehalfInfo StoreModuleGetLoginOnBehalfInfo(string storeId, string id)
        {
             ApiResponse<LoginOnBehalfInfo> localVarResponse = StoreModuleGetLoginOnBehalfInfoWithHttpInfo(storeId, id);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Check if given contact has login on behalf permission 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="storeId">Store ID</param>
        /// <param name="id">Contact ID</param>
        /// <returns>ApiResponse of LoginOnBehalfInfo</returns>
        public ApiResponse<LoginOnBehalfInfo> StoreModuleGetLoginOnBehalfInfoWithHttpInfo(string storeId, string id)
        {
            // verify the required parameter 'storeId' is set
            if (storeId == null)
                throw new ApiException(400, "Missing required parameter 'storeId' when calling VirtoCommerceStoreApi->StoreModuleGetLoginOnBehalfInfo");
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceStoreApi->StoreModuleGetLoginOnBehalfInfo");

            var localVarPath = "/api/stores/{storeId}/accounts/{id}/loginonbehalf";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (storeId != null) localVarPathParams.Add("storeId", ApiClient.ParameterToString(storeId)); // path parameter
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetLoginOnBehalfInfo: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetLoginOnBehalfInfo: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<LoginOnBehalfInfo>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (LoginOnBehalfInfo)ApiClient.Deserialize(localVarResponse, typeof(LoginOnBehalfInfo)));
            
        }

        /// <summary>
        /// Check if given contact has login on behalf permission 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="storeId">Store ID</param>
        /// <param name="id">Contact ID</param>
        /// <returns>Task of LoginOnBehalfInfo</returns>
        public async System.Threading.Tasks.Task<LoginOnBehalfInfo> StoreModuleGetLoginOnBehalfInfoAsync(string storeId, string id)
        {
             ApiResponse<LoginOnBehalfInfo> localVarResponse = await StoreModuleGetLoginOnBehalfInfoAsyncWithHttpInfo(storeId, id);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Check if given contact has login on behalf permission 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="storeId">Store ID</param>
        /// <param name="id">Contact ID</param>
        /// <returns>Task of ApiResponse (LoginOnBehalfInfo)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<LoginOnBehalfInfo>> StoreModuleGetLoginOnBehalfInfoAsyncWithHttpInfo(string storeId, string id)
        {
            // verify the required parameter 'storeId' is set
            if (storeId == null)
                throw new ApiException(400, "Missing required parameter 'storeId' when calling VirtoCommerceStoreApi->StoreModuleGetLoginOnBehalfInfo");
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceStoreApi->StoreModuleGetLoginOnBehalfInfo");

            var localVarPath = "/api/stores/{storeId}/accounts/{id}/loginonbehalf";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (storeId != null) localVarPathParams.Add("storeId", ApiClient.ParameterToString(storeId)); // path parameter
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetLoginOnBehalfInfo: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetLoginOnBehalfInfo: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<LoginOnBehalfInfo>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (LoginOnBehalfInfo)ApiClient.Deserialize(localVarResponse, typeof(LoginOnBehalfInfo)));
            
        }
        /// <summary>
        /// Get store by id 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Store id</param>
        /// <returns>Store</returns>
        public Store StoreModuleGetStoreById(string id)
        {
             ApiResponse<Store> localVarResponse = StoreModuleGetStoreByIdWithHttpInfo(id);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get store by id 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Store id</param>
        /// <returns>ApiResponse of Store</returns>
        public ApiResponse<Store> StoreModuleGetStoreByIdWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceStoreApi->StoreModuleGetStoreById");

            var localVarPath = "/api/stores/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetStoreById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetStoreById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Store>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Store)ApiClient.Deserialize(localVarResponse, typeof(Store)));
            
        }

        /// <summary>
        /// Get store by id 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Store id</param>
        /// <returns>Task of Store</returns>
        public async System.Threading.Tasks.Task<Store> StoreModuleGetStoreByIdAsync(string id)
        {
             ApiResponse<Store> localVarResponse = await StoreModuleGetStoreByIdAsyncWithHttpInfo(id);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get store by id 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Store id</param>
        /// <returns>Task of ApiResponse (Store)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Store>> StoreModuleGetStoreByIdAsyncWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceStoreApi->StoreModuleGetStoreById");

            var localVarPath = "/api/stores/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetStoreById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetStoreById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Store>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Store)ApiClient.Deserialize(localVarResponse, typeof(Store)));
            
        }
        /// <summary>
        /// Get all stores 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;Store&gt;</returns>
        public List<Store> StoreModuleGetStores()
        {
             ApiResponse<List<Store>> localVarResponse = StoreModuleGetStoresWithHttpInfo();
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get all stores 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of List&lt;Store&gt;</returns>
        public ApiResponse<List<Store>> StoreModuleGetStoresWithHttpInfo()
        {

            var localVarPath = "/api/stores";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetStores: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetStores: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Store>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Store>)ApiClient.Deserialize(localVarResponse, typeof(List<Store>)));
            
        }

        /// <summary>
        /// Get all stores 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of List&lt;Store&gt;</returns>
        public async System.Threading.Tasks.Task<List<Store>> StoreModuleGetStoresAsync()
        {
             ApiResponse<List<Store>> localVarResponse = await StoreModuleGetStoresAsyncWithHttpInfo();
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get all stores 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (List&lt;Store&gt;)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<List<Store>>> StoreModuleGetStoresAsyncWithHttpInfo()
        {

            var localVarPath = "/api/stores";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetStores: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetStores: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Store>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Store>)ApiClient.Deserialize(localVarResponse, typeof(List<Store>)));
            
        }
        /// <summary>
        /// Returns list of stores which user can sign in 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="userId"></param>
        /// <returns>List&lt;Store&gt;</returns>
        public List<Store> StoreModuleGetUserAllowedStores(string userId)
        {
             ApiResponse<List<Store>> localVarResponse = StoreModuleGetUserAllowedStoresWithHttpInfo(userId);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Returns list of stores which user can sign in 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="userId"></param>
        /// <returns>ApiResponse of List&lt;Store&gt;</returns>
        public ApiResponse<List<Store>> StoreModuleGetUserAllowedStoresWithHttpInfo(string userId)
        {
            // verify the required parameter 'userId' is set
            if (userId == null)
                throw new ApiException(400, "Missing required parameter 'userId' when calling VirtoCommerceStoreApi->StoreModuleGetUserAllowedStores");

            var localVarPath = "/api/stores/allowed/{userId}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (userId != null) localVarPathParams.Add("userId", ApiClient.ParameterToString(userId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetUserAllowedStores: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetUserAllowedStores: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Store>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Store>)ApiClient.Deserialize(localVarResponse, typeof(List<Store>)));
            
        }

        /// <summary>
        /// Returns list of stores which user can sign in 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="userId"></param>
        /// <returns>Task of List&lt;Store&gt;</returns>
        public async System.Threading.Tasks.Task<List<Store>> StoreModuleGetUserAllowedStoresAsync(string userId)
        {
             ApiResponse<List<Store>> localVarResponse = await StoreModuleGetUserAllowedStoresAsyncWithHttpInfo(userId);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Returns list of stores which user can sign in 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="userId"></param>
        /// <returns>Task of ApiResponse (List&lt;Store&gt;)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<List<Store>>> StoreModuleGetUserAllowedStoresAsyncWithHttpInfo(string userId)
        {
            // verify the required parameter 'userId' is set
            if (userId == null)
                throw new ApiException(400, "Missing required parameter 'userId' when calling VirtoCommerceStoreApi->StoreModuleGetUserAllowedStores");

            var localVarPath = "/api/stores/allowed/{userId}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (userId != null) localVarPathParams.Add("userId", ApiClient.ParameterToString(userId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetUserAllowedStores: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleGetUserAllowedStores: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Store>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Store>)ApiClient.Deserialize(localVarResponse, typeof(List<Store>)));
            
        }
        /// <summary>
        /// Search stores 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria"></param>
        /// <returns>SearchResult</returns>
        public SearchResult StoreModuleSearchStores(SearchCriteria criteria)
        {
             ApiResponse<SearchResult> localVarResponse = StoreModuleSearchStoresWithHttpInfo(criteria);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Search stores 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria"></param>
        /// <returns>ApiResponse of SearchResult</returns>
        public ApiResponse<SearchResult> StoreModuleSearchStoresWithHttpInfo(SearchCriteria criteria)
        {
            // verify the required parameter 'criteria' is set
            if (criteria == null)
                throw new ApiException(400, "Missing required parameter 'criteria' when calling VirtoCommerceStoreApi->StoreModuleSearchStores");

            var localVarPath = "/api/stores/search";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (criteria.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(criteria); // http body (model) parameter
            }
            else
            {
                localVarPostBody = criteria; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleSearchStores: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleSearchStores: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<SearchResult>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (SearchResult)ApiClient.Deserialize(localVarResponse, typeof(SearchResult)));
            
        }

        /// <summary>
        /// Search stores 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria"></param>
        /// <returns>Task of SearchResult</returns>
        public async System.Threading.Tasks.Task<SearchResult> StoreModuleSearchStoresAsync(SearchCriteria criteria)
        {
             ApiResponse<SearchResult> localVarResponse = await StoreModuleSearchStoresAsyncWithHttpInfo(criteria);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Search stores 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria"></param>
        /// <returns>Task of ApiResponse (SearchResult)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<SearchResult>> StoreModuleSearchStoresAsyncWithHttpInfo(SearchCriteria criteria)
        {
            // verify the required parameter 'criteria' is set
            if (criteria == null)
                throw new ApiException(400, "Missing required parameter 'criteria' when calling VirtoCommerceStoreApi->StoreModuleSearchStores");

            var localVarPath = "/api/stores/search";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (criteria.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(criteria); // http body (model) parameter
            }
            else
            {
                localVarPostBody = criteria; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleSearchStores: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleSearchStores: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<SearchResult>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (SearchResult)ApiClient.Deserialize(localVarResponse, typeof(SearchResult)));
            
        }
        /// <summary>
        /// Send dynamic notification (contains custom list of properties) an store or adminsitrator email 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="request"></param>
        /// <returns></returns>
        public void StoreModuleSendDynamicNotificationAnStoreEmail(SendDynamicNotificationRequest request)
        {
             StoreModuleSendDynamicNotificationAnStoreEmailWithHttpInfo(request);
        }

        /// <summary>
        /// Send dynamic notification (contains custom list of properties) an store or adminsitrator email 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="request"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> StoreModuleSendDynamicNotificationAnStoreEmailWithHttpInfo(SendDynamicNotificationRequest request)
        {
            // verify the required parameter 'request' is set
            if (request == null)
                throw new ApiException(400, "Missing required parameter 'request' when calling VirtoCommerceStoreApi->StoreModuleSendDynamicNotificationAnStoreEmail");

            var localVarPath = "/api/stores/send/dynamicnotification";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (request.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(request); // http body (model) parameter
            }
            else
            {
                localVarPostBody = request; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleSendDynamicNotificationAnStoreEmail: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleSendDynamicNotificationAnStoreEmail: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Send dynamic notification (contains custom list of properties) an store or adminsitrator email 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="request"></param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task StoreModuleSendDynamicNotificationAnStoreEmailAsync(SendDynamicNotificationRequest request)
        {
             await StoreModuleSendDynamicNotificationAnStoreEmailAsyncWithHttpInfo(request);

        }

        /// <summary>
        /// Send dynamic notification (contains custom list of properties) an store or adminsitrator email 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="request"></param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> StoreModuleSendDynamicNotificationAnStoreEmailAsyncWithHttpInfo(SendDynamicNotificationRequest request)
        {
            // verify the required parameter 'request' is set
            if (request == null)
                throw new ApiException(400, "Missing required parameter 'request' when calling VirtoCommerceStoreApi->StoreModuleSendDynamicNotificationAnStoreEmail");

            var localVarPath = "/api/stores/send/dynamicnotification";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (request.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(request); // http body (model) parameter
            }
            else
            {
                localVarPostBody = request; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleSendDynamicNotificationAnStoreEmail: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleSendDynamicNotificationAnStoreEmail: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Update store 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns></returns>
        public void StoreModuleUpdate(Store store)
        {
             StoreModuleUpdateWithHttpInfo(store);
        }

        /// <summary>
        /// Update store 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> StoreModuleUpdateWithHttpInfo(Store store)
        {
            // verify the required parameter 'store' is set
            if (store == null)
                throw new ApiException(400, "Missing required parameter 'store' when calling VirtoCommerceStoreApi->StoreModuleUpdate");

            var localVarPath = "/api/stores";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (store.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(store); // http body (model) parameter
            }
            else
            {
                localVarPostBody = store; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleUpdate: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleUpdate: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Update store 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task StoreModuleUpdateAsync(Store store)
        {
             await StoreModuleUpdateAsyncWithHttpInfo(store);

        }

        /// <summary>
        /// Update store 
        /// </summary>
        /// <exception cref="VirtoCommerce.StoreModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="store">Store</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> StoreModuleUpdateAsyncWithHttpInfo(Store store)
        {
            // verify the required parameter 'store' is set
            if (store == null)
                throw new ApiException(400, "Missing required parameter 'store' when calling VirtoCommerceStoreApi->StoreModuleUpdate");

            var localVarPath = "/api/stores";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (store.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(store); // http body (model) parameter
            }
            else
            {
                localVarPostBody = store; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleUpdate: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling StoreModuleUpdate: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
    }
}
