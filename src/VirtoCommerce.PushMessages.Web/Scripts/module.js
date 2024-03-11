// Call this to register your module to main application
var moduleName = 'PushMessages';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider',
        function ($stateProvider) {
            $stateProvider
                .state('workspace.PushMessagesState', {
                    url: '/PushMessages',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        'platformWebApp.bladeNavigationService',
                        function (bladeNavigationService) {
                            var newBlade = {
                                id: 'blade1',
                                controller: 'PushMessages.helloWorldController',
                                template: 'Modules/$(VirtoCommerce.PushMessages)/Scripts/blades/hello-world.html',
                                isClosingDisabled: true,
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])
    .run(['platformWebApp.mainMenuService', '$state',
        function (mainMenuService, $state) {
            //Register module in main menu
            var menuItem = {
                path: 'browse/PushMessages',
                icon: 'fa fa-cube',
                title: 'PushMessages',
                priority: 100,
                action: function () { $state.go('workspace.PushMessagesState'); },
                permission: 'PushMessages:access',
            };
            mainMenuService.addMenuItem(menuItem);
        }
    ]);
