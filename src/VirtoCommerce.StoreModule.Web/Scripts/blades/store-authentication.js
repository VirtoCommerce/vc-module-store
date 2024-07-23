angular.module('virtoCommerce.storeModule')
    .controller('virtoCommerce.storeModule.storeAuthenticationController', [
        '$scope', 'virtoCommerce.storeModule.storeAuthenticationApi',
        function ($scope, storeAuthenticationApi) {
            var blade = $scope.blade;
            blade.headIcon = 'fas fa-key';
            blade.title = 'stores.blades.store-authentication.title';

            blade.refresh = function () {
                blade.isLoading = true;

                storeAuthenticationApi.get({ storeId: blade.storeId }).$promise.then(function (data) {
                    blade.originalEntities = data;
                    blade.currentEntities = angular.copy(blade.originalEntities);
                    blade.isLoading = false;
                });
            }

            blade.toolbarCommands = [
                {
                    name: "platform.commands.save",
                    icon: 'fas fa-save',
                    executeMethod: saveChanges,
                    canExecuteMethod: canSave,
                    permission: blade.updatePermission
                },
                {
                    name: "platform.commands.reset",
                    icon: 'fa fa-undo',
                    executeMethod: reset,
                    canExecuteMethod: isDirty,
                    permission: blade.updatePermission
                }
            ];

            function saveChanges() {
                blade.isLoading = true;
                storeAuthenticationApi.update({ storeId: blade.storeId }, blade.currentEntities, blade.refresh);
            };

            function reset() {
                angular.copy(blade.originalEntities, blade.currentEntities);
            }

            function canSave() {
                return isDirty();
            }

            function isDirty() {
                return !angular.equals(blade.currentEntities, blade.originalEntities) && blade.hasUpdatePermission();
            }

            blade.refresh();
        }]);
