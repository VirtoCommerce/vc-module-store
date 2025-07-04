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
    public class StoreService : OuterEntityService<Store, StoreEntity, StoreChangeEvent, StoreChangedEvent>, IStoreService
    {
        private readonly Func<IStoreRepository> _repositoryFactory;
        private readonly IEventPublisher _eventPublisher;
        private readonly ISettingsManager _settingsManager;

        public StoreService(
            Func<IStoreRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher,
            ISettingsManager settingsManager)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _repositoryFactory = repositoryFactory;
            _eventPublisher = eventPublisher;
            _settingsManager = settingsManager;
        }

        public async Task<IList<string>> GetUserAllowedStoreIdsAsync(ApplicationUser user)
        {
            var retVal = new List<string>();

            if (user?.StoreId != null)
            {
                var store = await this.GetNoCloneAsync(user.StoreId, nameof(StoreResponseGroup.StoreInfo));
                if (store != null)
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

        public override async Task DeleteAsync(IList<string> ids, bool softDelete = false)
        {
            using var repository = _repositoryFactory();
            var changedEntries = new List<GenericChangedEntry<Store>>();
            var stores = await GetAsync(ids, nameof(StoreResponseGroup.StoreInfo));
            var dbStores = await repository.GetByIdsAsync(ids);

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

        protected override Task<IList<StoreEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return ((IStoreRepository)repository).GetByIdsAsync(ids, responseGroup);
        }

        protected override IQueryable<StoreEntity> GetEntitiesQuery(IRepository repository)
        {
            return ((IStoreRepository)repository).Stores;
        }

        protected override Store ProcessModel(string responseGroup, StoreEntity entity, Store model)
        {
            _settingsManager.DeepLoadSettingsAsync(model).GetAwaiter().GetResult();
            return model;
        }

        protected override Task BeforeSaveChanges(IList<Store> models)
        {
            ValidateStoresProperties(models);
            return Task.CompletedTask;
        }

        protected override Task AfterSaveChangesAsync(IList<Store> models, IList<GenericChangedEntry<Store>> changedEntries)
        {
            return _settingsManager.DeepSaveSettingsAsync(models);
        }

        private static void ValidateStoresProperties(IEnumerable<Store> stores)
        {
            ArgumentNullException.ThrowIfNull(stores);

            var validator = new StoreValidator();
            foreach (var store in stores)
            {
                validator.ValidateAndThrow(store);
            }
        }
    }
}
