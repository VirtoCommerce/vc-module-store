angular.module('virtoCommerce.storeModule')
.factory('virtoCommerce.storeModule.catalogs', ['$resource', function ($resource) {
    return $resource('api/catalog/catalogs/:id', { id: '@Id' }, {
        get: { method: 'GET' },
        getCatalogs: { method: 'GET', isArray: true },
        search: { method: 'POST', url: 'api/catalog/catalogs/search'},
    });
}]);
