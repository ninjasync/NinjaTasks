/// <reference path="../lib/angular/angular.d.ts" />
/// <reference path="../lib/angular/angular-ui-router.d.ts" />

var ninjaTasksApp = angular.module('ninjaTasksApp', [
    'ui.router',
    'ninjaTasksControllers'
]);
ninjaTasksApp.config(['$stateProvider', '$urlRouterProvider',
    ($stateProvider, $urlRouterProvider) => {
        $urlRouterProvider.otherwise("/");
        $stateProvider
            .state("home", {
                url: "/:listId",
                views: {
                    '': { templateUrl: "/todo/partials/todo-app.html" },

                    'lists@home':{
                        templateUrl: "/todo/partials/todo-lists.html",
                        controller: "ListsController"
                    },

                    'tasks@home': {
                        templateUrl: '/todo/partials/todo-tasks1.html',
                        controller: 'TasksController'
                    }
                }
            })
            //.state('task', {
            //    url: '/tasks/:taskId',
            //    templateUrl: '/todo-app/partials/todo-task.html',
            //    controller: 'TaskController'
            //})
        ;
    }]);

ninjaTasksApp.run([
        '$rootScope', '$state', '$stateParams',
        ($rootScope, $state, $stateParams) => {

            // It's very handy to add references to $state and $stateParams to the $rootScope
            // so that you can access them from any scope within your applications.For example,
            // <li ng-class="{ active: $state.includes('contacts.list') }"> will set the <li>
            // to active whenever 'contacts.list' or one of its decendents is active.
            $rootScope.$state = $state;
            $rootScope.$stateParams = $stateParams;
        }
    ]
);

var ninjaTasksControllers = angular.module('ninjaTasksControllers', []);


ninjaTasksControllers.controller('ListsController', [
             '$scope','$http', '$log',
    function ($scope, $http, $log) {
        console.log("ListsContoller instantiated");
        $http.get('/api/todo/lists').success(function (data) {
            $scope.lists = data;
        });
    }]);
ninjaTasksControllers.controller('TasksController', [
             '$scope','$stateParams','$http',
    function ($scope,  $stateParams,  $http) {
        $scope.listId = $stateParams.listId;

        $http.get('/api/todo/lists/' + $scope.listId + "/tasks").success(function (data) {
            $scope.tasks = data;
        });
    }]);
