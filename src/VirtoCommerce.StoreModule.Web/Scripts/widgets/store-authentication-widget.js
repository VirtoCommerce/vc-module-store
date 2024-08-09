angular.module('virtoCommerce.storeModule')
    .controller('virtoCommerce.storeModule.storeAuthenticationWidgetController', [
        '$scope', 'platformWebApp.bladeNavigationService',
        function ($scope, bladeNavigationService) {
            var blade = $scope.widget.blade;

            $scope.openBlade = function () {
                var newBlade = {
                    id: 'storeAuthenticationTypes',
                    controller: 'virtoCommerce.storeModule.storeAuthenticationController',
                    template: 'Modules/$(VirtoCommerce.Store)/Scripts/blades/store-authentication.html',
                    storeId: blade.currentEntity.id
                };

                bladeNavigationService.showBlade(newBlade, blade);
            };
        }]);
