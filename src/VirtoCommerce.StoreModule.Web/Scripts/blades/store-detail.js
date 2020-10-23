angular.module('virtoCommerce.storeModule')
.controller('virtoCommerce.storeModule.storeDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.storeModule.stores', 'virtoCommerce.storeModule.catalogs', 'platformWebApp.settings', 'platformWebApp.settings.helper', 'platformWebApp.dialogService', 'virtoCommerce.coreModule.currency.currencyUtils',
    function ($scope, bladeNavigationService, stores, catalogs, settings, settingsHelper, dialogService, currencyUtils) {
        var blade = $scope.blade;
        $scope.pageSize = 20;
        blade.updatePermission = 'store:update';
        blade.subtitle = 'stores.blades.store-detail.subtitle';

        blade.refresh = function (parentRefresh) {
            blade.isLoading = true;
            stores.get({ id: blade.currentEntityId }, (data) => {
                initializeBlade(data);
                if (parentRefresh) {
                    blade.parentBlade.refresh();
                }
            })
        }


        function initializeBlade(data) {

            getCatalog(data.catalog); 

            data.additionalLanguages = _.without(data.languages, data.defaultLanguage);
            data.additionalCurrencies = _.without(data.currencies, data.defaultCurrency);

            blade.currentEntityId = data.id;
            blade.title = data.name;

            settingsHelper.fixValues(data.settings);

            if (data.shippingMethods) {
                data.shippingMethods.sort(function (a, b) { return a.priority - b.priority; });
            }

            if (data.paymentMethods) {
                data.paymentMethods.sort(function (a, b) { return a.priority - b.priority; });
            }

            _.each(data.shippingMethods, function (x) { settingsHelper.fixValues(x.settings); });
            _.each(data.paymentMethods, function (x) { settingsHelper.fixValues(x.settings); });
            _.each(data.taxProviders, function (x) { settingsHelper.fixValues(x.settings); });

            blade.currentEntity = angular.copy(data);
            blade.origEntity = data;
            blade.isLoading = false;

            //sets security scopes for scope bounded ACL
            if (blade.currentEntity.scopes && angular.isArray(blade.currentEntity.scopes)) {
                blade.scopes = blade.currentEntity.scopes;
            }
        }

        async function getCatalog(catalogId) {
            $scope.catalogs = [];

            let catalog = await catalogs.get({ id: catalogId }).$promise;
            $scope.catalogs.push(catalog)
        }

        function isDirty() {
            return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
        }

        function canSave() {
            return isDirty() && $scope.formScope && $scope.formScope.$valid;
        }

        $scope.saveChanges = function () {
            blade.isLoading = true;

            var entityToSave = angular.copy(blade.currentEntity);
            entityToSave.languages = _.union([entityToSave.defaultLanguage], entityToSave.additionalLanguages);
            entityToSave.currencies = _.union([entityToSave.defaultCurrency], entityToSave.additionalCurrencies);

            settingsHelper.toApiFormat(entityToSave.settings);
            _.each(entityToSave.shippingMethods, function (x) { settingsHelper.toApiFormat(x.settings); });
            _.each(entityToSave.paymentMethods, function (x) { settingsHelper.toApiFormat(x.settings); });
            _.each(entityToSave.taxProviders, function (x) { settingsHelper.toApiFormat(x.settings); });

            stores.update({}, entityToSave, function (data) {
                blade.refresh(true);
            }, function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
        };

        function deleteEntry() {
            var dialog = {
                id: "confirmDelete",
                title: "stores.dialogs.store-delete.title",
                message: "stores.dialogs.store-delete.message",
                callback: function (remove) {
                    if (remove) {
                        blade.isLoading = true;

                        stores.remove({ ids: blade.currentEntityId }, function () {
                            $scope.bladeClose();
                            blade.parentBlade.refresh();
                        }, function (error) {
                            bladeNavigationService.setError('Error ' + error.status, blade);
                        });
                    }
                }
            }
            dialogService.showConfirmationDialog(dialog);
        }

        $scope.fetchCatalogs = ($select) => {
            $select.page = 0;
            $scope.catalogs = [];
            $scope.fetchNextCatalogs($select);
        }
    
        $scope.fetchNextCatalogs = ($select) => {
            let criteria = {
                SearchPhrase: $select.search,
                take: $scope.pageSize,
                skip: $select.page * $scope.pageSize
            }

            catalogs.search(criteria, (data) => {
                $scope.catalogs = $scope.catalogs.concat(data.results);
                $select.page++;
            });
        }

        $scope.setForm = function (form) { $scope.formScope = form; };

        blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "stores.dialogs.store-save.title", "stores.dialogs.store-save.message");
        };

        blade.headIcon = 'fa-archive';
        blade.toolbarCommands = [
            {
                name: "platform.commands.save",
                icon: 'fa fa-save',
                executeMethod: $scope.saveChanges,
                canExecuteMethod: canSave,
                permission: blade.updatePermission
            },
            {
                name: "platform.commands.reset",
                icon: 'fa fa-undo',
                executeMethod: function () {
                    angular.copy(blade.origEntity, blade.currentEntity);
                },
                canExecuteMethod: isDirty,
                permission: blade.updatePermission
            },
            {
                name: "platform.commands.open-browser", icon: 'fa fa-external-link',
                executeMethod: function () {
                    window.open(blade.currentEntity.url, '_blank');
                },
                canExecuteMethod: function () {
                    return blade.currentEntity && blade.currentEntity.url;
                }
            },
            {
                name: "platform.commands.delete", icon: 'fa fa-trash-o',
                executeMethod: deleteEntry,
                canExecuteMethod: function () { return true; },
                permission: 'store:delete'
            }
        ];

        $scope.openLanguagesDictionarySettingManagement = function () {
            var newBlade = {
                id: 'settingDetailChild',
                isApiSave: true,
                currentEntityId: 'VirtoCommerce.Core.General.Languages',
                parentRefresh: function (data) { $scope.languages = data; },
                controller: 'platformWebApp.settingDictionaryController',
                template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.openStatesDictionarySettingManagement = function () {
            var newBlade = {
                id: 'settingDetailChild',
                isApiSave: true,
                currentEntityId: 'Stores.States',
                parentRefresh: function (data) { $scope.storeStates = data; },
                controller: 'platformWebApp.settingDictionaryController',
                template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.$on("refresh-entity-by-id", function (event, id) {
            if (blade.currentEntityId === id) {
                bladeNavigationService.closeChildrenBlades(blade, blade.refresh);
            }
        });

        blade.refresh();
        
        $scope.storeStates = settings.getValues({ id: 'Stores.States' });
        $scope.languages = settings.getValues({ id: 'VirtoCommerce.Core.General.Languages' });
        $scope.allStores = stores.query();
        $scope.currencyUtils = currencyUtils;
    }]);
