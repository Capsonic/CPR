'use strict';

/**
 * @ngdoc function
 * @name appApp.controller:GrossmarginfuturebyfiscalperiodCtrl
 * @description
 * # GrossmarginfuturebyfiscalperiodCtrl
 * Controller of the appApp
 */
angular.module('appApp').controller('GrossmarginfuturebyfiscalperiodCtrl', function($scope, appConfig) {
    $scope.baseURL = appConfig.API_URL;

    $scope.currentUser = {
        UserName: 'ReportsPortal'
    };

    $scope.clearFilters = function() {
        $scope.filter_dateFrom = null;
        $scope.filter_dateTo = null;
    };
});
