angular.module('PushMessages')
    .factory('PushMessages.webApi', ['$resource', function ($resource) {
        return $resource('api/push-messages');
    }]);
