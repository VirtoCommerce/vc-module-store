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
                joinFulfillmentCenters(data.results);                
            });
        }

        $scope.refreshFulfillmentCenters = function ($select) {                      
            $scope.fetchNextFulfillmentCenters($select);
        }

        $scope.fetchNextFulfillmentCenters = function ($select) {            
            fulfillments.search({ searchPhrase: $select.search, take: $scope.pageSize, skip: $scope.fulfillmentCenters.length }, function (data) {
                joinFulfillmentCenters(data.results);
                console.log($scope.fulfillmentCenters.length);                
            }); 
        }        

        function loadFulfillmentCenters() {                        
            $scope.fulfillmentsIds = _.uniq($scope.fulfillmentsIds.concat($scope.blade.currentEntity.mainFulfillmentCenterId, $scope.blade.currentEntity.mainReturnsFulfillmentCenterId, $scope.blade.currentEntity.additionalFulfillmentCenterIds, $scope.blade.currentEntity.returnsFulfillmentCenterIds));
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
