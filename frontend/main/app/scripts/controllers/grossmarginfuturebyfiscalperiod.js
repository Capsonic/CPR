'use strict';

/**
 * @ngdoc function
 * @name appApp.controller:GrossmarginfuturebyfiscalperiodCtrl
 * @description
 * # GrossmarginfuturebyfiscalperiodCtrl
 * Controller of the appApp
 */
angular.module('appApp').controller('GrossmarginfuturebyfiscalperiodCtrl', function($scope, appConfig, $rootScope, $http) {
    $scope.baseURL = appConfig.API_URL;

    $http({
        method: 'GET',
        url: appConfig.API_URL + 'Ping/auth',
        withCredentials: true,
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        }
    }).then(function(reponse) {
        $rootScope.currentUser = reponse.data;
        $rootScope.userName = $rootScope.currentUser.UserName.replace('CAPSONIC\\', '');
    }, function() {
        alertify.alert('Incorrect Login');
    });

    $scope.clearFilters = function() {
        $scope.filter_dateFrom = null;
        $scope.filter_dateTo = null;
    };
});
