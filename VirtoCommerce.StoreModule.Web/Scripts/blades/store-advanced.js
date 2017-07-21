angular.module('virtoCommerce.storeModule')
.controller('virtoCommerce.storeModule.storeAdvancedController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.storeModule.stores', 'virtoCommerce.coreModule.fulfillment.fulfillments', 'platformWebApp.common.countries', 'platformWebApp.common.timeZones', function ($scope, bladeNavigationService, stores, fulfillments, countries, timeZones) {
    $scope.saveChanges = function () {
        angular.copy($scope.blade.currentEntity, $scope.blade.origEntity);
        $scope.bladeClose();
    };

    $scope.setForm = function (form) {
        $scope.formScope = form;
    }

    $scope.isValid = function () {
        return $scope.formScope && $scope.formScope.$valid;
    }

    $scope.cancelChanges = function () {
        $scope.bladeClose();
    }

    $scope.blade.refresh = function () {
        getFulfillmentCenters();
    }

    $scope.openFulfillmentCentersList = function () {
        var newBlade = {
            id: 'fulfillmentCenterList',
            controller: 'virtoCommerce.coreModule.fulfillment.fulfillmentListController',
            template: 'Modules/$(VirtoCommerce.Core)/Scripts/fulfillment/blades/fulfillment-center-list.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, $scope.blade);
    }

    $scope.blade.headIcon = 'fa-archive';

    $scope.setAdditionalFulfillmentCentersList = function (fulfillmentCenters) {
        var defaultFulfillmentCenter = _.find(fulfillmentCenters, function (fc) { return angular.equals(fc, $scope.blade.currentEntity.fulfillmentCenter); });
        $scope.additionalFulfillmentCenters = _.without(fulfillmentCenters, defaultFulfillmentCenter);
    }

    $scope.setAdditionalReturnsFulfillmentCentersList = function (fulfillmentCenters) {
        var defaultReturnsFulfillmentCenter = _.find(fulfillmentCenters, function (fc) { return angular.equals(fc, $scope.blade.currentEntity.returnsFulfillmentCenter); });
        $scope.additionalReturnsFulfillmentCenters = _.without(fulfillmentCenters, defaultReturnsFulfillmentCenter);
    }

    fulfillments.query(function (response) {
        $scope.fulfillmentCenters = response;
        $scope.setAdditionalFulfillmentCentersList(response);
    });

    getFulfillmentCenters();

    $scope.blade.isLoading = false;
    $scope.blade.currentEntity = angular.copy($scope.blade.entity);
    $scope.blade.origEntity = $scope.blade.entity;
    $scope.countries = countries.query();
    $scope.timeZones = timeZones.query();

    function getFulfillmentCenters() {
        fulfillments.query(function (response) {
            $scope.fulfillmentCenters = response;
            $scope.setAdditionalFulfillmentCentersList(response);
            $scope.setAdditionalReturnsFulfillmentCentersList(response);
        });
    }
}]);