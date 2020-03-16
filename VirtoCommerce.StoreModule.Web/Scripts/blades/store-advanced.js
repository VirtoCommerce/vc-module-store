angular.module('virtoCommerce.storeModule')
    .controller('virtoCommerce.storeModule.storeAdvancedController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.storeModule.stores', 'virtoCommerce.inventoryModule.fulfillments', 'platformWebApp.common.countries', 'platformWebApp.common.timeZones', function ($scope, bladeNavigationService, stores, fulfillments, countries, timeZones) {
        $scope.saveChanges = function () {
            angular.copy($scope.blade.currentEntity, $scope.blade.origEntity);
            $scope.bladeClose();
        };

        $scope.setForm = function(form) {
            $scope.formScope = form;
        };

        $scope.isValid = function() {
            return $scope.formScope && $scope.formScope.$valid;
        };

        $scope.cancelChanges = function() {
            $scope.bladeClose();
        };

        $scope.blade.refresh = function() {
            initialize();
        };

        $scope.openFulfillmentCentersList = function() {
            var newBlade = {
                id: 'fulfillmentCenterList',
                controller: 'virtoCommerce.inventoryModule.fulfillmentListController',
                template: 'Modules/$(VirtoCommerce.Inventory)/Scripts/blades/fulfillment-center-list.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, $scope.blade);
        };

        $scope.blade.headIcon = 'fa-archive';
        $scope.pageSize = 20;

        $scope.blade.isLoading = false;
        $scope.blade.currentEntity = angular.copy($scope.blade.entity);
        $scope.blade.origEntity = $scope.blade.entity;
        $scope.countries = countries.query();
        $scope.timeZones = timeZones.query();


        $scope.fullFillmentCenters = {
            common: [],
            default: [],
            defaultReturn:[],
            available: [],
            availableReturn:[]

        };

        function initialize() {

            loadSelectedFulfillmentCenter();

            fulfillments.search({ searchPhrase: '', skip: 0, take: $scope.pageSize }, function (data) {
                $scope.fullFillmentCenters.common = data.results;
            });

        }

        $scope.fetchFirstFulfillmentCenters = function($select, listName) {
            $select.page = 0;
            $scope.fullFillmentCenters[listName] = [];

            return $scope.fetchNextFulfillmentCenters($select, listName);
        };

        $scope.fetchNextFulfillmentCenters = function ($select, listName) {
            const countToSkip = $select.page * $scope.pageSize;
            const countToTake = $scope.pageSize;

            if ($select.page === 0 && $select.search === '') {
                $scope.fullFillmentCenters[listName] = _.uniq($scope.fullFillmentCenters[listName].concat($scope.fullFillmentCenters.common), 'id');
                $select.page++;
                $scope.$broadcast('scrollCompleted');
            } else {
                fulfillments.search({ searchPhrase: $select.search, skip: countToSkip, take: countToTake }, function (data) {
                    $select.page++;
                    $scope.fullFillmentCenters[listName] = _.uniq($scope.fullFillmentCenters[listName].concat(data.results), 'id');

                    if ($scope.fullFillmentCenters[listName].length < data.totalCount) {
                        // Reset scrolling for the when-scrolled directive, so it could trigger this method for next page.
                        $scope.$broadcast('scrollCompleted');
                    }
                });
            }

        };


        function loadSelectedFulfillmentCenter() {
            var selectedIds = _.flatten ([$scope.blade.currentEntity.mainFulfillmentCenterId,
                $scope.blade.currentEntity.mainReturnsFulfillmentCenterId,
                $scope.blade.currentEntity.additionalFulfillmentCenterIds,
                $scope.blade.currentEntity.returnsFulfillmentCenterIds]);
            selectedIds = _.uniq(selectedIds);
            selectedIds = _.filter(selectedIds, function (value) {
                   return value !== undefined;
                });
            if (selectedIds.length > 0) {
                fulfillments.getByIds(selectedIds, function (data) {
                    const selectedItems = _.uniq($scope.fullFillmentCenters.common.concat(data), 'id');
                    $scope.fullFillmentCenters.default = _.filter(selectedItems, function (item) { return $scope.blade.currentEntity.mainFulfillmentCenterId === item.id; });
                    $scope.fullFillmentCenters.defaultReturn = _.filter(selectedItems, function (item) { return $scope.blade.currentEntity.mainReturnsFulfillmentCenterId === item.id; });
                    $scope.fullFillmentCenters.available = _.filter(selectedItems, function (item) { return $scope.blade.currentEntity.additionalFulfillmentCenterIds.includes(item.id); });
                    $scope.fullFillmentCenters.availableReturn = _.filter(selectedItems, function (item) { return $scope.blade.currentEntity.returnsFulfillmentCenterIds.includes(item.id); });

                });
            }
        }

        initialize();
        
    }]);
