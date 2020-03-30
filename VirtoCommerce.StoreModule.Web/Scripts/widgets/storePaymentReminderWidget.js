angular.module('virtoCommerce.storeModule')
    .controller('virtoCommerce.storeModule.storePaymentReminderWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
        var blade = $scope.widget.blade;

        blade.showPaymentReminders = function () {
            var objectId = blade.currentEntity.id;
            var objectTypeId = 'Store';
            var newBlade = {
                id: 'storePaymentReminderWidgetChild',
                title: 'stores.widgets.storePaymentReminderWidget.blade-title',
                titleValues: { id: blade.currentEntity.id },
                objectId: objectId,
                objectTypeId: objectTypeId,
                languages: blade.currentEntity.languages,
                subtitle: 'stores.widgets.storePaymentReminderWidget.blade-subtitle',
                controller: 'Travel.OrderModule.storePaymentRemindersController',
                template: 'Modules/$(TCS.Orders)/Scripts/paymentReminder/paymentReminders-list.blade.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };
    }]);
