'use strict';

/**
 * @ngdoc function
 * @name appApp.controller:ReportsCtrl
 * @description
 * # ReportsCtrl
 * Controller of the appApp
 */
angular.module('appApp').controller('ReportsCtrl', function($scope, appConfig, catWorkOrderService, catAreaService, userService) {
    $scope.baseURL = appConfig.API_URL;

    $scope.userService = userService;
    $scope.catWorkOrderService = catWorkOrderService;
    $scope.catAreaService = catAreaService;

    $scope.clearFilters = function() {
        $scope.filter_dateFrom = null;
        $scope.filter_dateTo = null;
        $scope.filter_workOrder = null;
        $scope.filter_area = null;
        $scope.filter_user = null;
    };

});
