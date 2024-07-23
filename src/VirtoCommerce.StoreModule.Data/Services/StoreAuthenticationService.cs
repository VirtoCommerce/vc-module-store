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
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEnumerable<ExternalSignInProviderConfiguration> _externalSigninProviderConfigs;

    public StoreAuthenticationService(
        IStoreAuthenticationSchemeService crudService,
        IStoreAuthenticationSchemeSearchService searchService,
        IOptions<PasswordLoginOptions> passwordOptions,
        SignInManager<ApplicationUser> signInManager,
        IEnumerable<ExternalSignInProviderConfiguration> externalSigninProviderConfigs)
    {
        _passwordLoginOptions = passwordOptions.Value;
        _crudService = crudService;
        _searchService = searchService;
        _signInManager = signInManager;
        _externalSigninProviderConfigs = externalSigninProviderConfigs;
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

    private async Task<List<GlobalAuthenticationScheme>> GetGlobalAuthenticationSchemes()
    {
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();

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

    private class GlobalAuthenticationScheme
    {
        public string Name { get; init; }
        public string DisplayName { get; init; }
        public int Priority { get; init; }
    }
}
