/// <reference path="../lib/angular/angular.d.ts" />
/// <reference path="../lib/angular/angular-ui-router.d.ts" />

var ninjaTasksApp = angular.module('ninjaTasksApp', [
    'ngRoute',
    'ninjaTasksControllers'
]);

ninjaTasksApp.config(['$routeProvider',
    $routeProvider => {
        $routeProvider.
            when('/lists', {
                templateUrl: '/todo-app/partials/todo-lists.html',
                controller: 'ListsController'
            }).
            when('/lists/:listId', {
                templateUrl: '/todo-app/partials/todo-tasks.html',
                controller: 'TasksController'
            }).
            otherwise({
                redirectTo: '/lists'
            });
    }]);

var ninjaTasksControllers = angular.module('ninjaTasksControllers', []);

ninjaTasksControllers.controller('ListsController', ['$scope', '$http',
    ($scope, $http) => {
    
        $http.get('/api/todo/lists').success(data => {
            $scope.lists = data;
        });
    }]);

ninjaTasksControllers.controller('TasksController', ['$scope', '$routeParams', '$http',
    ($scope, $routeParams, $http) => {
        $scope.listId = $routeParams.listId;
    }]);
