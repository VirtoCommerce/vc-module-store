//Call this to register our module to main application
var moduleName = "virtoCommerce.storeModule";

if (AppDependencies != undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [
    'ngSanitize'
])
    .config(
        ['$stateProvider', function ($stateProvider) {
            $stateProvider
                .state('workspace.storeModule', {
                    url: '/store',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        '$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
                            var blade = {
                                id: 'store',
                                title: 'stores.blades.stores-list.title',
                                controller: 'virtoCommerce.storeModule.storesListController',
                                template: 'Modules/$(VirtoCommerce.Store)/Scripts/blades/stores-list.tpl.html',
                                isClosingDisabled: true
                            };
                            bladeNavigationService.showBlade(blade);
                        }
                    ]
                });
        }]
    )
    .run(
        ['platformWebApp.bladeNavigationService', 'platformWebApp.metaFormsService', 'platformWebApp.loginOfBehalfUrlResolver', '$q', 'platformWebApp.mainMenuService', 'platformWebApp.widgetService', '$state', 'platformWebApp.permissionScopeResolver', 'virtoCommerce.storeModule.stores',
            function (bladeNavigationService, metaFormsService, loginOfBehalfUrlResolver, $q, mainMenuService, widgetService, $state, scopeResolver, stores) {
                //Register module in main menu
                var menuItem = {
                    path: 'browse/store',
                    icon: 'fa fa-archive',
                    title: 'stores.main-menu-title',
                    priority: 110,
                    action: function () { $state.go('workspace.storeModule'); },
                    permission: 'store:access'
                };
                mainMenuService.addMenuItem(menuItem);

                metaFormsService.registerMetaFields('accountDetails', [
                    {
                        name: 'storeId',
                        templateUrl: 'Modules/$(VirtoCommerce.Store)/Scripts/blades/templates/accountDetails/storeId.html',
                        priority: 100
                    }
                ]);
                metaFormsService.registerMetaFields('storeDetails', []);

                //Register widgets in store details
                widgetService.registerWidget({
                    controller: 'virtoCommerce.storeModule.assetsWidgetController',
                    template: 'Modules/$(VirtoCommerce.Store)/Scripts/widgets/assetsWidget.tpl.html'
                }, 'storeDetail');
                widgetService.registerWidget({
                    controller: 'virtoCommerce.coreModule.seo.seoWidgetController',
                    template: 'Modules/$(VirtoCommerce.Core)/Scripts/SEO/widgets/seoWidget.tpl.html',
                    objectType: 'Store',
                    getFixedStoreId: function (blade) { return blade.currentEntity.id; },
                    getDefaultContainerId: function (blade) { return blade.currentEntity.id; },
                    getLanguages: function (blade) { return blade.currentEntity.languages; }
                }, 'storeDetail');
                widgetService.registerWidget({
                    controller: 'virtoCommerce.storeModule.storeAdvancedWidgetController',
                    template: 'Modules/$(VirtoCommerce.Store)/Scripts/widgets/storeAdvancedWidget.tpl.html'
                }, 'storeDetail');
                widgetService.registerWidget({
                    controller: 'platformWebApp.dynamicPropertyWidgetController',
                    template: '$(Platform)/Scripts/app/dynamicProperties/widgets/dynamicPropertyWidget.tpl.html'
                }, 'storeDetail');
                widgetService.registerWidget({
                    //controller: 'virtoCommerce.storeModule.storeSettingsWidgetController',
                    //template: 'Modules/$(VirtoCommerce.Store)/Scripts/widgets/storeSettingsWidget.tpl.html'
                    controller: 'platformWebApp.entitySettingsWidgetController',
                    template: '$(Platform)/Scripts/app/settings/widgets/entitySettingsWidget.tpl.html'
                }, 'storeDetail');

                widgetService.registerWidget({
                    controller: 'virtoCommerce.storeModule.storeNotificationsWidgetController',
                    template: 'Modules/$(VirtoCommerce.Store)/Scripts/widgets/storeNotificationsWidget.tpl.html'
                }, 'storeDetail');
                widgetService.registerWidget({
                    controller: 'virtoCommerce.storeModule.storeNotificationsLogWidgetController',
                    template: 'Modules/$(VirtoCommerce.Store)/Scripts/widgets/storeNotificationsLogWidget.tpl.html'
                }, 'storeDetail');

                widgetService.registerWidget({
                    controller: 'virtoCommerce.storeModule.storeAuthenticationWidgetController',
                    template: 'Modules/$(VirtoCommerce.Store)/Scripts/widgets/store-authentication-widget.html'
                }, 'storeDetail');

                //Register permission scopes templates used for scope bounded definition in role management ui
                var selectedStoreScope = {
                    type: 'StoreSelectedScope',
                    title: 'Only for selected stores',
                    selectFn: function (blade, callback) {
                        var newBlade = {
                            id: 'store-pick',
                            title: this.title,
                            subtitle: 'Select stores',
                            currentEntity: this,
                            onChangesConfirmedFn: callback,
                            dataService: stores,
                            controller: 'platformWebApp.security.scopeValuePickFromSimpleListController',
                            template: '$(Platform)/Scripts/app/security/blades/common/scope-value-pick-from-simple-list.tpl.html'
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    }
                };
                scopeResolver.register(selectedStoreScope);

                // resolver for login-on-behalf functionality in platform account blade
                loginOfBehalfUrlResolver.register((user) => {
                    return $q(function (resolve) {
                        if (user.storeId) {
                            stores.get({ id: user.storeId },
                                (data) => resolve(data.url)
                            );
                        } else {
                            resolve(null);
                        }
                    });
                });
            }]);
