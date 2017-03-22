'use strict';

/**
 * @ngdoc function
 * @name appApp.controller:MainCtrl
 * @description
 * # MainCtrl
 * Controller of the appApp
 */
angular.module('appApp').controller('MainCtrl', function($scope, localStorageService, $rootScope) {

    $scope.bootstraps = [
        { name: 'Default', url: 'theme-default' },
        { name: 'Cerulean', url: 'theme-cerulean' },
        { name: 'Cosmo', url: 'theme-cosmo' },
        { name: 'Cyborg', url: 'theme-cyborg' },
        { name: 'Darkly', url: 'theme-darkly' },
        { name: 'Flatly', url: 'theme-flatly' },
        { name: 'Journal', url: 'theme-journal' },
        { name: 'Lumen', url: 'theme-lumen' },
        { name: 'Paper', url: 'theme-paper' },
        { name: 'Readable', url: 'theme-readable' },
        { name: 'Simplex', url: 'theme-simplex' },
        { name: 'Slate', url: 'theme-slate' },
        { name: 'Spacelab', url: 'theme-spacelab' },
        { name: 'Standstone', url: 'theme-standstone' },
        { name: 'Superhero', url: 'theme-superhero' },
        { name: 'United', url: 'theme-united' },
        { name: 'Yeti', url: 'theme-Yeti' }
    ];

    $scope.setTheme = function(themeIndex) {
        $rootScope.currentThemeIndex = themeIndex;
        $rootScope.theme = $scope.bootstraps[$rootScope.currentThemeIndex];
        localStorageService.set('theme', themeIndex);
    };

    var themeIndexSaved = localStorageService.get('theme');

    if (themeIndexSaved) {
        $scope.setTheme(themeIndexSaved);
    } else {
        $scope.setTheme(0);
    }

});
