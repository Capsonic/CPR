'use strict';

/**
 * @ngdoc service
 * @name iqsApp.baseControllers
 * @description
 * # baseControllers
 * Service in the iqsApp.
 */
angular.module('inspiracode.baseControllers', []).factory('formController', function($log, $activityIndicator, $routeParams, validatorService, appConfig, $timeout, $rootScope) {

    var log = $log;

    return function(oMainConfig) {

        //INIT CONFIG/////////////////////////////////////

        var scope = oMainConfig.scope;

        var _baseService = oMainConfig.baseService;
        if (!oMainConfig.entityName || oMainConfig.entityName == '') {
            oMainConfig.entityName = '';
        }

        //After Load callback
        var _afterLoadCallBack = oMainConfig.afterLoad;
        if (!_afterLoadCallBack || typeof _afterLoadCallBack != "function") {
            _afterLoadCallBack = function() {};
        }

        //After create entity callback
        var _afterCreateCallBack = oMainConfig.afterCreate;
        if (!_afterCreateCallBack || typeof _afterCreateCallBack != "function") {
            _afterCreateCallBack = function() {};
        }

        //END CONFIG/////////////////////////////////////


        //scope---------------------------------------------
        //let's use normal variables (without underscore) so they can be
        //accessed in view normally
        scope.isLoading = true;
        scope.remove = function(oEntity) {
            alertify.confirm(
                '¿Está seguro que quiere eliminar: ' + oMainConfig.entityName + '?',
                function() {
                    scope.$apply(function() {
                        $activityIndicator.startAnimating();
                        _baseService.remove(oEntity, scope.baseList).then(function(data) {
                            _updateList();
                        }, function() {
                            $activityIndicator.stopAnimating();
                        });
                    });
                });
        };
        scope.take = function(objToTake, toUser) {
            _baseService.take(objToTake, toUser).then(function(data) {
                objToTake.assignedTo = toUser.Value;
                objToTake.AssignationMade = false;
            });
        };

        scope.create = function() {
            $activityIndicator.startAnimating();
            _baseService.createEntity().then(function(oNewEntity) {
                scope.baseEntity = angular.copy(oNewEntity);
                _afterCreateCallBack(theCreatedEntity);
                $activityIndicator.stopAnimating();
            });
        };

        //Updating items:*******************************
        scope.save = function(oItem) {
            $activityIndicator.startAnimating();
            return _baseService.save(oItem).then(function(data) {
                $activityIndicator.stopAnimating();
            });
        };
        scope.on_input_change = function(oItem) {
            oItem.editMode = true;
        };
        scope.undo = function(oItem) {
            var originalItem = _baseService.getById(oItem.id);
            angular.copy(originalItem, oItem);
        };
        //end scope----------------------------------------

        var _afterLoad = function() {
            _afterLoadCallBack(scope.baseEntity);
            $activityIndicator.stopAnimating();
        };

        var _load = function(oEntityOrID) {
            $activityIndicator.startAnimating();
            alertify.closeAll();

            _baseService.loadDependencies().then(function(data) {
                _fillCatalogs();
                _refreshForm(oEntityOrID);
            });
        };

        var _fillCatalogs = function() {
            //for filters:

            for (var catalog in _baseService.catalogs) {
                if (_baseService.catalogs.hasOwnProperty(catalog)) {

                    var theCatalog = 'the' + capitalizeFirstLetter(catalog);
                    if (theCatalog.slice(-1) != 's') {
                        theCatalog += 's';
                    }
                    scope[theCatalog] = _baseService.catalogs[catalog].getAll();
                }
            }

        };

        function capitalizeFirstLetter(sWord) {
            var result = sWord.charAt(0).toUpperCase() + sWord.slice(1).toLowerCase();
            return result;
        }

        var _makeQueryParameters = function() {
            var result = '?';

            for (var prop in scope.filterOptions) {
                if (scope.filterOptions.hasOwnProperty(prop)) {
                    result += prop + '=' + scope.filterOptions[prop] + '&';
                }
            }

            return result;
        };

        var _refreshForm = function(oEntityOrID) {
            $activityIndicator.startAnimating();
            scope.isLoading = true;

            //Create
            if (!oEntityOrID) {
                _baseService.createEntity().then(function(oNewEntity) {
                    scope.baseEntity = angular.copy(oNewEntity);
                    _afterCreateCallBack(theCreatedEntity);
                    $activityIndicator.stopAnimating();
                });
            }
            //Update by ID
            else if (!isNaN(oEntityOrID) && oEntityOrID > 0) {
                scope.openingMode = 'id';
                _baseService.loadEntity(oEntityOrID).then(function(data) {
                    var theSelectedEntity = data;
                    if (!theSelectedEntity) {
                        alertify.alert('¡El registro no existe!').set('modal', true).set('closable', false);
                        scope.openingMode = 'error';
                        return;
                    }
                    scope.baseEntity = angular.copy(theSelectedEntity);
                    _afterLoad();
                    scope.isLoading = false;
                });
            } else if (oEntityOrID instanceof Object || typeof(oEntityOrID) == 'object') {
                scope.baseEntity = angular.copy(oEntityOrID);
                _afterLoad();
                scope.isLoading = false;
            }
        };

        // Public baseController API:////////////////////////////////////////////////////////////
        var oAPI = {
            load: _load,
            // unselectAll: _unselectAll
        };
        return oAPI;
    };

});
