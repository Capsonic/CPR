'use strict';

/**
 * @ngdoc overview
 * @name appApp
 * @description
 * # appApp
 *
 * Main module of the application.
 */
angular.module('appApp', [
    'ngAnimate',
    'ngRoute',
    'ngSanitize',
    'ngTouch',
    'ngActivityIndicator',
    'LocalStorageModule',
    'inspiracode.baseControllers',
    'CommonDirectives',
], function($httpProvider) {
    $httpProvider.defaults.headers.post['Content-Type'] = 'application/x-www-form-urlencoded;charset=utf-8';
    $httpProvider.defaults.headers.put['Content-Type'] = 'application/x-www-form-urlencoded;charset=utf-8';
}).config(function($routeProvider, appConfig, $activityIndicatorProvider, $httpProvider, localStorageServiceProvider) {

    localStorageServiceProvider.setPrefix(appConfig.APP_NAME);

    $routeProvider
        .when('/reports', {
            templateUrl: 'views/reports.html',
            controller: 'ReportsCtrl',
            controllerAs: 'reports'
        })
        .when('/GrossMarginFutureByFiscalPeriod', {
          templateUrl: 'views/grossmarginfuturebyfiscalperiod.html',
          controller: 'GrossmarginfuturebyfiscalperiodCtrl',
          controllerAs: 'GrossMarginFutureByFiscalPeriod'
        })
        .otherwise({
            redirectTo: '/reports'
        });

    $activityIndicatorProvider.setActivityIndicatorStyle('CircledWhite');
    alertify.set('notifier', 'position', 'top-left');
    alertify.set('notifier', 'delay', 2);

}).run(function($rootScope, $location) {

    // register listener to watch route changes
    $rootScope.$on('$routeChangeSuccess', function(event, next, current) {
        alertify.closeAll();
        $('.modal').modal('hide');
        $('.modal-backdrop.fade.in').remove();
    });

    $rootScope.$on('$routeChangeSuccess', function() {
        $rootScope.activePath = $location.path();
    });

});
