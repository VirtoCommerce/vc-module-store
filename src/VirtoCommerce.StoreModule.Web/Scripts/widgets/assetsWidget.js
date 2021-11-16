angular.module('virtoCommerce.storeModule')
.controller('virtoCommerce.storeModule.assetsWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
    var blade = $scope.blade;

    $scope.openBlade = function () {
        var newBlade = {
            id: "storeAssetList",
            subtitle: blade.title,
            controller: 'virtoCommerce.assetsModule.assetListController',
            template: 'Modules/$(VirtoCommerce.Assets)/Scripts/blades/asset-list.tpl.html',
            currentEntity: { url: '/stores/' + blade.currentEntityId }
        };

        bladeNavigationService.showBlade(newBlade, blade);
    };
}]);
