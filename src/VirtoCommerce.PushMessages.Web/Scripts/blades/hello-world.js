angular.module('PushMessages')
    .controller('PushMessages.helloWorldController', ['$scope', 'PushMessages.webApi', function ($scope, api) {
        var blade = $scope.blade;
        blade.title = 'PushMessages';

        blade.refresh = function () {
            api.get(function (data) {
                blade.title = 'PushMessages.blades.hello-world.title';
                blade.data = data.result;
                blade.isLoading = false;
            });
        };

        blade.refresh();
    }]);
