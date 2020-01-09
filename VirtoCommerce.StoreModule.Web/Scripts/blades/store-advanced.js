angular.module('virtoCommerce.storeModule')
    .controller('virtoCommerce.storeModule.storeAdvancedController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.storeModule.stores', 'virtoCommerce.inventoryModule.fulfillments', 'platformWebApp.common.countries', 'platformWebApp.common.timeZones', function ($scope, bladeNavigationService, stores, fulfillments, countries, timeZones) {
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
            initialize();
        }

        $scope.openFulfillmentCentersList = function () {
            var newBlade = {
                id: 'fulfillmentCenterList',
                controller: 'virtoCommerce.inventoryModule.fulfillmentListController',
                template: 'Modules/$(VirtoCommerce.Inventory)/Scripts/blades/fulfillment-center-list.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, $scope.blade);
        }

        $scope.blade.headIcon = 'fa-archive';
        // pageSize amount must be enough to show scrollbar in dropdown list counter.
        // If scrollbar doesn't appear then auto loading won't work.
        $scope.pageSize = 50;

        $scope.blade.isLoading = false;
        $scope.blade.currentEntity = angular.copy($scope.blade.entity);
        $scope.blade.origEntity = $scope.blade.entity;
        $scope.countries = countries.query();
        $scope.timeZones = timeZones.query();

        function initialize() {
            $scope.fulfillmentCenters = [];
            $scope.fulfillmentsIds = [];
            loadFulfillmentCenters();
            fulfillments.search({ take: $scope.pageSize }, function (data) {
                // Redefine pageSize based on totalCount of fulfillment centers count so scroll will work correctly
                $scope.pageSize = Math.round(data.totalCount * 0.65);
                joinFulfillmentCenters(data.results);
            });
        }

        $scope.refreshFulfillmentCenters = function ($select) {
            $scope.fulfillmentCenters = [];
            $scope.fulfillmentsIds = [];
            loadFulfillmentCenters();
            $scope.fetchNextFulfillmentCenters($select);
        }

        $scope.fetchNextFulfillmentCenters = function ($select) {
            fulfillments.search({ searchPhrase: $select.search, take: $scope.pageSize, skip: $scope.fulfillmentCenters.length }, function (data) {
                joinFulfillmentCenters(data.results);
                console.log($scope.fulfillmentCenters.length);
            });
        }

        function loadFulfillmentCenters() {
            var mainFulfillmentCenterId = $scope.blade.currentEntity.mainFulfillmentCenterId;
            var mainReturnsFulfillmentCenterId = $scope.blade.currentEntity.mainReturnsFulfillmentCenterId;
            var additionalFulfillmentCenterIds = $scope.blade.currentEntity.additionalFulfillmentCenterIds;
            var returnsFulfillmentCenterIds = $scope.blade.currentEntity.returnsFulfillmentCenterIds;
            $scope.fulfillmentsIds = $scope.fulfillmentsIds.concat(mainFulfillmentCenterId, mainReturnsFulfillmentCenterId, additionalFulfillmentCenterIds, returnsFulfillmentCenterIds);
            $scope.fulfillmentsIds = _.uniq($scope.fulfillmentsIds);
            $scope.fulfillmentsIds = _.filter($scope.fulfillmentsIds, function (value) {
                return value != undefined;
            });
            if ($scope.fulfillmentsIds.length > 0) {
                fulfillments.getByIds($scope.fulfillmentsIds, function (data) {
                    joinFulfillmentCenters(data);
                });
            }
        }

        function joinFulfillmentCenters(centers) {
            $scope.fulfillmentCenters = _.uniq($scope.fulfillmentCenters.concat(centers), 'id');
        }

        initialize();
        
    }]);
