using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security.SignInMethods;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.Model;

namespace VirtoCommerce.StoreModule.Data.Services;

public class StoreAuthenticationService : IStoreAuthenticationService
{
    private readonly IStoreAuthenticationSchemeService _crudService;
    private readonly IStoreAuthenticationSchemeSearchService _searchService;
    private readonly ISignInMethodService _signInMethodService;

    public StoreAuthenticationService(
        IStoreAuthenticationSchemeService crudService,
        IStoreAuthenticationSchemeSearchService searchService,
        ISignInMethodService signInMethodService)
    {
        _crudService = crudService;
        _searchService = searchService;
        _signInMethodService = signInMethodService;
    }

    public Task SaveStoreSchemesAsync(string storeId, IList<StoreAuthenticationScheme> models)
    {
        if (string.IsNullOrEmpty(storeId))
        {
            throw new ArgumentNullException(nameof(storeId));
        }

        var position = 0;
        models.Apply(x =>
        {
            x.StoreId = storeId;
            x.Position = position++;
        });

        return _crudService.SaveChangesAsync(models);
    }

    public async Task<IList<StoreAuthenticationScheme>> GetStoreSchemesAsync(string storeId, bool clone = true)
    {
        if (string.IsNullOrEmpty(storeId))
        {
            return [];
        }

        var criteria = AbstractTypeFactory<StoreAuthenticationSchemeSearchCriteria>.TryCreateInstance();
        criteria.StoreId = storeId;
        criteria.Sort = nameof(StoreAuthenticationSchemeEntity.Position);

        var storeSchemes = await _searchService.SearchAllAsync(criteria, clone);
        var signInMethods = await _signInMethodService.GetSignInMethodsAsync();

        foreach (var scheme in storeSchemes.ToList())
        {
            var method = signInMethods.FirstOrDefault(x => x.Name.EqualsIgnoreCase(scheme.Name));
            if (method is null)
            {
                storeSchemes.Remove(scheme);
            }
            else
            {
                scheme.DisplayName = method.DisplayName;
            }
        }

        foreach (var method in signInMethods.Where(method => !storeSchemes.Any(scheme => scheme.Name.EqualsIgnoreCase(method.Name))))
        {
            var storeScheme = AbstractTypeFactory<StoreAuthenticationScheme>.TryCreateInstance();
            storeScheme.StoreId = storeId;
            storeScheme.Name = method.Name;
            storeScheme.DisplayName = method.DisplayName;
            storeScheme.IsActive = true;

            storeSchemes.Add(storeScheme);
        }

        return storeSchemes;
    }
}
