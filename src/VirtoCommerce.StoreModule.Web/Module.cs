using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.EnvironmentsCompare.Core.Services;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.StoreModule.Core;
using VirtoCommerce.StoreModule.Core.Events;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Notifications;
using VirtoCommerce.StoreModule.Core.Security;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.ExportImport;
using VirtoCommerce.StoreModule.Data.Handlers;
using VirtoCommerce.StoreModule.Data.MySql;
using VirtoCommerce.StoreModule.Data.PostgreSql;
using VirtoCommerce.StoreModule.Data.Repositories;
using VirtoCommerce.StoreModule.Data.Services;
using VirtoCommerce.StoreModule.Data.SqlServer;
using VirtoCommerce.StoreModule.Web.Authorization;

using ISeoResolver = VirtoCommerce.Seo.Core.Services.ISeoResolver;

namespace VirtoCommerce.StoreModule.Web
{
    public class Module : IModule, IExportSupport, IImportSupport, IHasConfiguration
    {
        public ManifestModuleInfo ModuleInfo { get; set; }
        private IApplicationBuilder _appBuilder;
        public IConfiguration Configuration { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<StoreDbContext>(options =>
            {
                var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
                var connectionString = Configuration.GetConnectionString(ModuleInfo.Id) ?? Configuration.GetConnectionString("VirtoCommerce");

                switch (databaseProvider)
                {
                    case "MySql":
                        options.UseMySqlDatabase(connectionString);
                        break;
                    case "PostgreSql":
                        options.UsePostgreSqlDatabase(connectionString);
                        break;
                    default:
                        options.UseSqlServerDatabase(connectionString);
                        break;
                }
            });

            serviceCollection.AddTransient<LogChangesChangedEventHandler>();
            serviceCollection.AddTransient<SendStoreUserVerificationEmailHandler>();
            serviceCollection.AddTransient<IStoreNotificationSender, StoreNotificationSender>();
            serviceCollection.AddTransient<IStoreRepository, StoreRepository>();
            serviceCollection.AddTransient<Func<IStoreRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetService<IStoreRepository>());
            serviceCollection.AddTransient<IStoreService, StoreService>();
            serviceCollection.AddTransient<IStoreSearchService, StoreSearchService>();
            serviceCollection.AddTransient<StoreExportImport>();
            serviceCollection.AddTransient<ISeoResolver, StoreSeoResolver>();
#pragma warning disable VC0010 // Obsolete interface
            serviceCollection.AddTransient<ISeoBySlugResolver, StoreSeoBySlugResolver>();
#pragma warning restore VC0010 // Obsolete interface
            serviceCollection.AddTransient<IAuthorizationHandler, StoreAuthorizationHandler>();
            serviceCollection.AddTransient<IStoreCurrencyResolver, StoreCurrencyResolver>();
            serviceCollection.AddTransient<IPublicStoreSettings, PublicStoreSettings>();
            serviceCollection.AddTransient<IStoreAuthenticationService, StoreAuthenticationService>();
            serviceCollection.AddTransient<IStoreAuthenticationSchemeService, StoreAuthenticationSchemeService>();
            serviceCollection.AddTransient<IStoreAuthenticationSchemeSearchService, StoreAuthenticationSchemeSearchService>();

            serviceCollection.AddTransient<IComparableSettingsProvider, ComparableStoreSettingsProvider>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            _appBuilder = appBuilder;

            var dynamicPropertyRegistrar = appBuilder.ApplicationServices.GetRequiredService<IDynamicPropertyRegistrar>();
            dynamicPropertyRegistrar.RegisterType<Store>();

            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);
            // Register settings for type Store
            settingsRegistrar.RegisterSettingsForType(ModuleConstants.Settings.AllSettings, nameof(Store));

            var permissionsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsRegistrar.RegisterPermissions(ModuleInfo.Id, "Store", ModuleConstants.Security.Permissions.AllPermissions);

            // Register permission scopes
            AbstractTypeFactory<PermissionScope>.RegisterType<StoreSelectedScope>();

            permissionsRegistrar.WithAvailabeScopesForPermissions(
                [
                    ModuleConstants.Security.Permissions.Read,
                    ModuleConstants.Security.Permissions.Update,
                    ModuleConstants.Security.Permissions.Delete,
                ],
                new StoreSelectedScope());

            // Register event handlers
            appBuilder.RegisterEventHandler<StoreChangedEvent, LogChangesChangedEventHandler>();
            appBuilder.RegisterEventHandler<UserVerificationEmailEvent, SendStoreUserVerificationEmailHandler>();

            var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");

            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<StoreDbContext>();

                if (databaseProvider == "SqlServer")
                {
                    dbContext.Database.MigrateIfNotApplied(MigrationName.GetUpdateV2MigrationName(ModuleInfo.Id));
                }

                dbContext.Database.Migrate();
            }

            var registrar = appBuilder.ApplicationServices.GetService<INotificationRegistrar>();
            registrar.RegisterNotification<StoreDynamicEmailNotification>();
        }

        public void Uninstall()
        {
            //Nothing do here
        }

        public Task ExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
            ICancellationToken cancellationToken)
        {
            return _appBuilder.ApplicationServices.GetRequiredService<StoreExportImport>().DoExportAsync(outStream,
                progressCallback, cancellationToken);
        }

        public Task ImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
            ICancellationToken cancellationToken)
        {
            return _appBuilder.ApplicationServices.GetRequiredService<StoreExportImport>().DoImportAsync(inputStream,
                progressCallback, cancellationToken);
        }
    }
}
