using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.Platform.Core.Bus;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.StoreModule.Core;
using VirtoCommerce.StoreModule.Core.Events;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Notifications;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.ExportImport;
using VirtoCommerce.StoreModule.Data.Handlers;
using VirtoCommerce.StoreModule.Data.Repositories;
using VirtoCommerce.StoreModule.Data.Services;
using VirtoCommerce.StoreModule.Web.Authorization;

namespace VirtoCommerce.StoreModule.Web
{
    public class Module : IModule, IExportSupport, IImportSupport
    {
        public ManifestModuleInfo ModuleInfo { get; set; }
        private IApplicationBuilder _appBuilder;

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<StoreDbContext>((provider, options) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                options.UseSqlServer(configuration.GetConnectionString(ModuleInfo.Id) ?? configuration.GetConnectionString("VirtoCommerce"));
            });

            serviceCollection.AddTransient<LogChangesChangedEventHandler>();
            serviceCollection.AddTransient<SendStoreUserVerificationEmailHandler>();
            serviceCollection.AddTransient<IStoreNotificationSender, StoreNotificationSender>();
            serviceCollection.AddTransient<IStoreRepository, StoreRepository>();
            serviceCollection.AddTransient<Func<IStoreRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetService<IStoreRepository>());
            serviceCollection.AddTransient<ICrudService<Store>, StoreService>();
            serviceCollection.AddTransient(x => (IStoreService)x.GetRequiredService<ICrudService<Store>>());
            serviceCollection.AddTransient<ISearchService<StoreSearchCriteria, StoreSearchResult, Store>, StoreSearchService>();
            serviceCollection.AddTransient(x => (IStoreSearchService)x.GetRequiredService<ISearchService<StoreSearchCriteria, StoreSearchResult, Store>>());
            serviceCollection.AddTransient<StoreExportImport>();
            serviceCollection.AddTransient<ISeoBySlugResolver, StoreSeoBySlugResolver>();
            serviceCollection.AddTransient<IAuthorizationHandler, StoreAuthorizationHandler>();
            serviceCollection.AddTransient<IStoreCurrencyResolver, StoreCurrencyResolver>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            _appBuilder = appBuilder;

            var dynamicPropertyRegistrar = appBuilder.ApplicationServices.GetRequiredService<IDynamicPropertyRegistrar>();
            dynamicPropertyRegistrar.RegisterType<Store>();

            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);
            //Register settings for type Store
            settingsRegistrar.RegisterSettingsForType(ModuleConstants.Settings.AllSettings, typeof(Store).Name);

            var permissionsProvider = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsProvider.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions.Select(x =>
                new Permission()
                {
                    GroupName = "Store",
                    ModuleId = ModuleInfo.Id,
                    Name = x
                }).ToArray());

            //Register Permission scopes
            AbstractTypeFactory<PermissionScope>.RegisterType<StoreSelectedScope>();
            permissionsProvider.WithAvailabeScopesForPermissions(new[] {
                                                                        ModuleConstants.Security.Permissions.Read,
                                                                        ModuleConstants.Security.Permissions.Update,
                                                                        ModuleConstants.Security.Permissions.Delete }, new StoreSelectedScope());

            //Events handlers registration
            var inProcessBus = appBuilder.ApplicationServices.GetService<IHandlerRegistrar>();
            inProcessBus.RegisterHandler<StoreChangedEvent>(async (message, token) => await appBuilder.ApplicationServices.GetService<LogChangesChangedEventHandler>().Handle(message));
            inProcessBus.RegisterHandler<UserVerificationEmailEvent>(async (message, token) => await appBuilder.ApplicationServices.GetService<SendStoreUserVerificationEmailHandler>().Handle(message));

            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<StoreDbContext>();
                dbContext.Database.MigrateIfNotApplied(MigrationName.GetUpdateV2MigrationName(ModuleInfo.Id));
                dbContext.Database.EnsureCreated();
                dbContext.Database.Migrate();
            }

            var registrar = appBuilder.ApplicationServices.GetService<INotificationRegistrar>();
            registrar.RegisterNotification<StoreDynamicEmailNotification>();
        }

        public void Uninstall()
        {
            //Nothing do here
        }

        public async Task ExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
            ICancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<StoreExportImport>().DoExportAsync(outStream,
                progressCallback, cancellationToken);
        }

        public async Task ImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
            ICancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<StoreExportImport>().DoImportAsync(inputStream,
                progressCallback, cancellationToken);
        }
    }
}
