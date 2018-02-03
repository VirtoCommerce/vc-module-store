using System;
using System.Linq;
using System.Web.Http;
using Microsoft.Practices.Unity;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Notifications;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.StoreModule.Data.Notifications;
using VirtoCommerce.StoreModule.Data.Repositories;
using VirtoCommerce.StoreModule.Data.Services;
using VirtoCommerce.StoreModule.Web.ExportImport;
using VirtoCommerce.StoreModule.Web.JsonConverters;
using VirtoCommerce.StoreModule.Web.Security;

namespace VirtoCommerce.StoreModule.Web
{
    public class Module : ModuleBase, ISupportExportImportModule
    {
        private readonly string _connectionStringName = ConfigurationHelper.GetConnectionStringValue("{{ModuleId}}") ?? ConfigurationHelper.GetConnectionStringValue("VirtoCommerce");
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase()
        {
            using (var db = new StoreRepositoryImpl(_connectionStringName, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<StoreRepositoryImpl, Data.Migrations.Configuration>();

                initializer.InitializeDatabase(db);
            }
        }

        public override void Initialize()
        {
            _container.RegisterType<IStoreRepository>(new InjectionFactory(c => new StoreRepositoryImpl(_connectionStringName, new EntityPrimaryKeyGeneratorInterceptor(), _container.Resolve<AuditableInterceptor>())));
            _container.RegisterType<IStoreService, StoreServiceImpl>();
        }

        public override void PostInitialize()
        {

            //Register bounded security scope types
            var securityScopeService = _container.Resolve<IPermissionScopeService>();
            securityScopeService.RegisterSope(() => new StoreSelectedScope());

            var notificationManager = _container.Resolve<INotificationManager>();

            notificationManager.RegisterNotificationType(() => new StoreDynamicEmailNotification(_container.Resolve<IEmailNotificationSendingGateway>())
            {
                DisplayName = "Sending custom form from storefront",
                Description = "This notification sends by email to client when he complite some form on storefront, for example contact us form",
                NotificationTemplate = new NotificationTemplate
                {
                    Body = "",
                    Subject = ""
                }
            });

            //Next lines allow to use polymorph types in API controller methods
            var httpConfiguration = _container.Resolve<HttpConfiguration>();
            var storeJsonConverter = _container.Resolve<PolymorphicStoreJsonConverter>();
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(storeJsonConverter);
            //Temporary workaround
            //Because Platform 2.12.2 version has default global converter StringEnumConverter(camelCase: true) 
            //and current manager UI required settingValueType with capital letter.
            //This converter used only to override global JSON enum policy (camelCase: true) -> camelCase: false
            //And it may be removed when platform updated to new version witch  global camelCase: false enum serialization policy 
            var converters = httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters.ToArray();
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters.Clear();
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters = new []  { new SettingValueTypeEnumJsonConverter() }.Concat(converters).ToList(); 

        }
        #endregion

        #region ISupportExportModule Members

        public void DoExport(System.IO.Stream outStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<StoreExportImport>();
            exportJob.DoExport(outStream, progressCallback);
        }

        public void DoImport(System.IO.Stream inputStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<StoreExportImport>();
            exportJob.DoImport(inputStream, progressCallback);
        }

        public string ExportDescription
        {
            get
            {
                var settingManager = _container.Resolve<ISettingsManager>();
                return settingManager.GetValue("Stores.ExportImport.Description", String.Empty);
            }
        }

        #endregion
    }
}
