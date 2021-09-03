using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ISettingsManager _settingsManager;

        public StoreService(Func<IStoreRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IEventPublisher eventPublisher, ISettingsManager settingsManager)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _settingsManager = settingsManager;
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

        public override async Task DeleteAsync(IEnumerable<string> ids, bool softDelete = false) {
            using (var repository = _repositoryFactory())
            {
                var changedEntries = new List<GenericChangedEntry<Store>>();
                var stores = await GetByIdsAsync(ids, StoreResponseGroup.StoreInfo.ToString());
                var dbStores = await ((IStoreRepository)repository).GetByIdsAsync(ids);

                foreach (var store in stores)
                {
                    var dbStore = dbStores.FirstOrDefault(x => x.Id == store.Id);
                    if (dbStore != null)
                    {
                        repository.Remove(dbStore);
                        changedEntries.Add(new GenericChangedEntry<Store>(store, EntryState.Deleted));
                    }
                }
                await repository.UnitOfWork.CommitAsync();
                await _settingsManager.DeepRemoveSettingsAsync(stores);
                ClearCache(stores);
                await _eventPublisher.Publish(new StoreChangedEvent(changedEntries));
            }
        }

        protected override Task<IEnumerable<StoreEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return ((IStoreRepository)repository).GetByIdsAsync(ids, responseGroup);
        }

        protected override Store ProcessModel(string responseGroup, StoreEntity entity, Store model)
        {
            _settingsManager.DeepLoadSettingsAsync(model).GetAwaiter().GetResult();
            return model;
        }

        protected override Task BeforeSaveChanges(IEnumerable<Store> models)
        {
            ValidateStoresProperties(models);
            return Task.CompletedTask;  
        }

        protected override async Task AfterSaveChangesAsync(IEnumerable<Store> models, IEnumerable<GenericChangedEntry<Store>> changedEntries)
        {
            await _settingsManager.DeepSaveSettingsAsync(models);
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

        #region IStoreService compatibility
        public async Task<Store[]> GetByIdsAsync(string[] ids, string responseGroup = null)
        {
            return (await GetByIdsAsync((IEnumerable<string>)ids, responseGroup)).ToArray();
        }

        public Task SaveChangesAsync(Store[] stores)
        {
            return SaveChangesAsync((IEnumerable<Store>)stores);
        }

        public Task DeleteAsync(string[] ids)
        {
            return DeleteAsync((IEnumerable<string>)ids);
        }
        #endregion
    }
}