angular.module('virtoCommerce.storeModule')
    .directive('vaStoreSelector', ['$q', 'virtoCommerce.storeModule.stores', function ($q, stores) {
        const defaultPageSize = 25;
        return {
            restrict: 'E',
            require: 'ngModel',
            replace: true,
            scope: {
                multiple: '=?',
                pageSize: '=?',
                placeholder: '=?',
                required: '=?',
                storesToHide: '=?'
            },
            templateUrl: 'Modules/$(VirtoCommerce.Store)/Scripts/directives/vaStoreSelector.tpl.html',
            link: function ($scope, element, attrs, ngModelController) {
                $scope.context = {
                    modelValue: null,
                    required: angular.isDefined(attrs.required) && (attrs.required === '' || attrs.required.toLowerCase() === 'true'),
                    multiple: angular.isDefined(attrs.multiple) && (attrs.multiple === '' || attrs.multiple.toLowerCase() === 'true')
                };

                // PageSize amount must be enough to show scrollbar in dropdown list container.
                // If scrollbar doesn't appear auto loading won't work.
                var pageSize = $scope.pageSize || defaultPageSize;

                $scope.choices = [];
                $scope.isNoChoices = true;
                var lastSearchPhrase = '';
                var totalCount = 0;

                $scope.fetchStores = function ($select) {
                    $q.all([loadEntityStores(), $scope.fetchNextStores($select)]);
                };

                function loadEntityStores() {
                    var storeIds = $scope.context.multiple ? $scope.context.modelValue : [$scope.context.modelValue];

                    if ($scope.isNoChoices && _.any(storeIds)) {
                        return stores.search({
                            storeIds: storeIds,
                            take: storeIds.length,
                            responseGroup: 'none'
                        }, (data) => {
                            joinStores(data.results);
                        }).$promise;
                    }

                    return $q.resolve();
                }

                $scope.fetchNextStores = ($select) => {
                    $select.page = $select.page || 0;

                    if (lastSearchPhrase !== $select.search) {
                        lastSearchPhrase = $select.search;
                        $select.page = 0;
                    }

                    if ($select.page === 0 || totalCount > $scope.choices.length) {
                        return stores.search(
                            {
                                searchPhrase: $select.search,
                                take: pageSize,
                                skip: $select.page * pageSize,
                                responseGroup: 'none'
                            }, (data) => {
                                joinStores(data.results);
                                $select.page++;

                                if ($select.page * pageSize < data.totalCount) {
                                    $scope.$broadcast('scrollCompleted');
                                }

                                totalCount = Math.max(totalCount, data.totalCount);
                            }).$promise;
                    }

                    return $q.resolve();
                };

                function joinStores(newItems) {
                    newItems = _.reject(newItems, x => _.indexOf($scope.storesToHide, x.id) > -1);
                    $scope.choices = _.union($scope.choices, newItems);
                    $scope.isNoChoices = $scope.choices.length === 0;
                }

                $scope.$watch('context.modelValue', function (newValue, oldValue) {
                    if (newValue !== oldValue) {
                        ngModelController.$setViewValue($scope.context.modelValue);
                    }
                });

                ngModelController.$render = function () {
                    $scope.context.modelValue = ngModelController.$modelValue;
                };
            }
        }
    }]);
