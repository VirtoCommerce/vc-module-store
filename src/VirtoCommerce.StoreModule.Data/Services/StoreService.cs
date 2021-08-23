using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.StoreModule.Core.Events;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.Model;
using VirtoCommerce.StoreModule.Data.Repositories;
using VirtoCommerce.StoreModule.Data.Services.Validation;

namespace VirtoCommerce.StoreModule.Data.Services
{
    public class StoreService : CrudService<Store, StoreEntity, StoreChangeEvent, StoreChangedEvent>, IStoreService
    {
        private readonly ISettingsManager _settingManager;

        public StoreService(Func<IStoreRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IEventPublisher eventPublisher, ISettingsManager settingManager)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _settingManager = settingManager;
        }

        public async Task<IEnumerable<string>> GetUserAllowedStoreIdsAsync(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var retVal = new List<string>();

            if (user.StoreId != null)
            {
                var stores = await GetByIdsAsync(new[] { user.StoreId }, StoreResponseGroup.StoreInfo.ToString());
                foreach (var store in stores)
                {
                    retVal.Add(store.Id);
                    if (!store.TrustedGroups.IsNullOrEmpty())
                    {
                        retVal.AddRange(store.TrustedGroups);
                    }
                }
            }
            return retVal;
        }


        protected override Task<IEnumerable<StoreEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return ((IStoreRepository)repository).GetByIdsAsync(ids);
        }

        protected override Store ProcessModel(string responseGroup, StoreEntity entity, Store model)
        {
            _settingManager.DeepLoadSettingsAsync(model).GetAwaiter().GetResult();
            return model;
        }

        protected override Task BeforeSaveChanges(IEnumerable<Store> models)
        {
            ValidateStoresProperties(models);
            return Task.CompletedTask;  
        }

        protected override async Task AfterSaveChangesAsync(IEnumerable<Store> models, IEnumerable<GenericChangedEntry<Store>> changedEntries)
        {
            await _settingManager.DeepSaveSettingsAsync(models);
        }

        protected override async Task AfterDeleteAsync(IEnumerable<Store> models, IEnumerable<GenericChangedEntry<Store>> changedEntries)
        {
            await _settingManager.DeepRemoveSettingsAsync(models);
        }

        private void ValidateStoresProperties(IEnumerable<Store> stores)
        {
            if (stores == null)
            {
                throw new ArgumentNullException(nameof(stores));
            }

            var validator = new StoreValidator();
            foreach (var store in stores)
            {
                validator.ValidateAndThrow(store);
            }
        }

    }
}