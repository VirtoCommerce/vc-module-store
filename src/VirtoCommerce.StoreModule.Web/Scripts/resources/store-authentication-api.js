angular.module('virtoCommerce.storeModule')
    .factory('virtoCommerce.storeModule.storeAuthenticationApi', ['$resource', function ($resource) {
        return $resource('api/store-authentication-schemes/:storeId', { storeId: '@storeId' }, {
            get: { method: 'GET', isArray: true },
            update: { method: 'PUT' }
        });
    }]);
