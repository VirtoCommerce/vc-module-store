using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.StoreModule.Core.Events;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.Model;
using VirtoCommerce.StoreModule.Data.Repositories;

namespace VirtoCommerce.StoreModule.Data.Services;

public class StoreAuthenticationSchemeService : CrudService<StoreAuthenticationScheme, StoreAuthenticationSchemeEntity, StoreAuthenticationSchemeChangingEvent, StoreAuthenticationSchemeChangedEvent>, IStoreAuthenticationSchemeService
{
    public StoreAuthenticationSchemeService(
        Func<IStoreRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
    }

    protected override Task<IList<StoreAuthenticationSchemeEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((IStoreRepository)repository).GetStoreAuthenticationSchemesByIdsAsync(ids, responseGroup);
    }
}
