using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.ExternalSignIn;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.Model;

namespace VirtoCommerce.StoreModule.Data.Services;

public class StoreAuthenticationService : IStoreAuthenticationService
{
    private readonly IStoreAuthenticationSchemeService _crudService;
    private readonly IStoreAuthenticationSchemeSearchService _searchService;
    private readonly PasswordLoginOptions _passwordLoginOptions;
    private readonly Func<SignInManager<ApplicationUser>> _signInManagerFactory;
    private readonly IEnumerable<ExternalSignInProviderConfiguration> _externalSigninProviderConfigs;
    private readonly IEnumerable<IStoreAuthenticationTypeProvider> _authenticationTypeProviders;

    public StoreAuthenticationService(
        IStoreAuthenticationSchemeService crudService,
        IStoreAuthenticationSchemeSearchService searchService,
        IOptions<PasswordLoginOptions> passwordOptions,
        Func<SignInManager<ApplicationUser>> signInManagerFactory,
        IEnumerable<ExternalSignInProviderConfiguration> externalSigninProviderConfigs,
        IEnumerable<IStoreAuthenticationTypeProvider> authenticationTypeProviders)
    {
        _passwordLoginOptions = passwordOptions.Value;
        _crudService = crudService;
        _searchService = searchService;
        _signInManagerFactory = signInManagerFactory;
        _externalSigninProviderConfigs = externalSigninProviderConfigs;
        _authenticationTypeProviders = authenticationTypeProviders;
    }

    public Task SaveStoreSchemesAsync(string storeId, IList<StoreAuthenticationScheme> models)
    {
        if (string.IsNullOrEmpty(storeId))
        {
            throw new ArgumentNullException(nameof(storeId));
        }

        return SaveStoreSchemesInternalAsync(storeId, models);
    }

    private async Task SaveStoreSchemesInternalAsync(string storeId, IList<StoreAuthenticationScheme> models)
    {
        var position = 0;
        models.Apply(x =>
        {
            x.StoreId = storeId;
            x.Position = position++;
        });

        var providerModels = models
            .Select(x => (Model: x, Provider: GetAuthenticationTypeProvider(x.Name)))
            .Where(x => x.Provider != null)
            .ToList();

        await _crudService.SaveChangesAsync(models.Except(providerModels.Select(x => x.Model)).ToList());

        foreach (var (model, provider) in providerModels)
        {
            if (await provider.IsActiveAsync(storeId) != model.IsActive)
            {
                await provider.SetIsActiveAsync(storeId, model.IsActive);
            }
        }
    }

    public async Task<IList<StoreAuthenticationScheme>> GetStoreAuthenticationTypesAsync(string storeId)
    {
        var result = (await GetStoreSchemesAsync(storeId)).ToList();
        if (string.IsNullOrEmpty(storeId))
        {
            return result;
        }

        var index = result.FindIndex(x => x.Name.EqualsIgnoreCase(_passwordLoginOptions.AuthenticationType)) + 1;

        foreach (var provider in _authenticationTypeProviders)
        {
            var type = AbstractTypeFactory<StoreAuthenticationScheme>.TryCreateInstance();
            type.StoreId = storeId;
            type.Name = provider.Name;
            type.DisplayName = provider.DisplayName;
            type.IsActive = await provider.IsActiveAsync(storeId);

            result.Insert(index++, type);
        }

        return result;
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
        var globalSchemes = await GetGlobalAuthenticationSchemes();

        foreach (var scheme in storeSchemes.ToList())
        {
            var globalScheme = globalSchemes.FirstOrDefault(x => x.Name.EqualsIgnoreCase(scheme.Name));
            if (globalScheme is null)
            {
                storeSchemes.Remove(scheme);
            }
            else
            {
                scheme.DisplayName = globalScheme.DisplayName;
            }
        }

        foreach (var globalScheme in globalSchemes.Where(scheme => !storeSchemes.Any(x => x.Name.EqualsIgnoreCase(scheme.Name))))
        {
            var storeScheme = AbstractTypeFactory<StoreAuthenticationScheme>.TryCreateInstance();
            storeScheme.StoreId = storeId;
            storeScheme.Name = globalScheme.Name;
            storeScheme.DisplayName = globalScheme.DisplayName;
            storeScheme.IsActive = true;

            storeSchemes.Add(storeScheme);
        }

        return storeSchemes;
    }

    private IStoreAuthenticationTypeProvider GetAuthenticationTypeProvider(string name)
    {
        return _authenticationTypeProviders.FirstOrDefault(x => x.Name.EqualsIgnoreCase(name));
    }

    private async Task<List<GlobalAuthenticationScheme>> GetGlobalAuthenticationSchemes()
    {
        var signInManager = _signInManagerFactory();
        var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();

        var result = schemes
            .Select(scheme => new GlobalAuthenticationScheme
            {
                Name = scheme.Name,
                DisplayName = scheme.DisplayName,
                Priority = _externalSigninProviderConfigs.FirstOrDefault(x => x.AuthenticationType.EqualsIgnoreCase(scheme.Name))?.Provider.Priority ?? 0,
            })
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.DisplayName)
            .ToList();

        if (_passwordLoginOptions.Enabled)
        {
            result.Insert(0, new GlobalAuthenticationScheme
            {
                Name = _passwordLoginOptions.AuthenticationType,
                DisplayName = _passwordLoginOptions.AuthenticationType,
            });
        }

        return result;
    }

    private sealed class GlobalAuthenticationScheme
    {
        public string Name { get; init; }
        public string DisplayName { get; init; }
        public int Priority { get; init; }
    }
}
