'use strict';

/**
 * @ngdoc service
 * @name inspiracode.baseControllers.listController
 * @description
 * # listController
 * Factory in the inspiracode.baseControllers.
 */
angular.module('inspiracode.baseControllers').factory('listController', function($log, $activityIndicator, $q, localStorageService, $location) {

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
        if (!_afterLoadCallBack || typeof _afterLoadCallBack != 'function') {
            _afterLoadCallBack = function() {};
        }

        //After create entity callback
        var _afterCreateCallBack = oMainConfig.afterCreate;
        if (!_afterCreateCallBack || typeof _afterCreateCallBack != 'function') {
            _afterCreateCallBack = function() {};
        }

        //On Open entity callback
        var _onOpenCallBack = oMainConfig.onOpenItem;
        if (!_onOpenCallBack || typeof _onOpenCallBack != 'function') {
            _onOpenCallBack = function() {};
        }



        var _filterStorageName = _baseService.entityName + $location.path();
        var _filters = oMainConfig.filters || [];
        var _perPage = oMainConfig.perPage || 10;
        //END CONFIG/////////////////////////////////////


        //scope---------------------------------------------
        //let's use normal variables (without underscore) so they can be
        //accessed in view normally
        scope.isLoading = true;
        scope.removeItem = function(oEntity) {
            alertify.confirm(
                '¿Está seguro que quiere eliminar: ' + oMainConfig.entityName + '?',
                function() {
                    scope.$apply(function() {
                        $activityIndicator.startAnimating();
                        _baseService.removeEntity(oEntity, scope.baseList).then(function(data) {
                            _updateList();
                        }, function() {
                            $activityIndicator.stopAnimating();
                        });
                    });
                });
        };
        scope.openItem = function(oEntity) {
            _onOpenCallBack(oEntity);
        };
        scope.take = function(objToTake, toUser) {
            _baseService.take(objToTake, toUser).then(function(data) {
                objToTake.assignedTo = toUser.Value;
                objToTake.AssignationMade = false;
            });
        };
        scope.checkItem = function(bSelected) {
            bSelected ? ++scope.selectedCount : --scope.selectedCount;
        };
        scope.deleteSelected = function() {
            if (scope.selectedCount > 0) {
                alertify.confirm(
                    '¿Está seguro que quiere eliminar todos los registros seleccionados?',
                    function() {
                        scope.$apply(function() {
                            _baseService.removeSelected(scope.baseList).then(function(data) {
                                _updateList();
                            });
                        });
                    });
            }
        };
        scope.create = function() {
            $activityIndicator.startAnimating();
            _baseService.createEntity().then(function(oNewEntity) {
                _afterCreateCallBack(oNewEntity);
                $activityIndicator.stopAnimating();
                // _baseService.save(oNewEntity).then(function(data) {
                //     var theCreatedEntity = angular.copy(data);
                //     scope.baseList.push(theCreatedEntity);
                // });

            });
        };

        scope.pageChanged = function(newPage) {
            scope.filterOptions.page = newPage;
            _updateList();
        };

        //Updating items:*******************************
        scope.addQty = 1;
        scope.addItems = function(addQty) {
            $activityIndicator.startAnimating();
            _baseService.addBatch(addQty, scope.baseList).then(function(data) {
                scope.selectedCount = _getSelectedCount();
                $activityIndicator.stopAnimating();
            });
        };
        scope.saveItem = function(oItem) {
            $activityIndicator.startAnimating();
            _baseService.save(oItem).then(function(data) {
                scope.selectedCount = _getSelectedCount();
                $activityIndicator.stopAnimating();
            });
        };
        scope.on_input_change = function(oItem) {
            oItem.editMode = true;
        };
        scope.undoItem = function(oItem) {
            var originalItem = _baseService.getById(oItem.id);
            angular.copy(originalItem, oItem);
        };
        scope.refresh = function() {
            if (!scope.filterOptions || scope.filterOptions.perPage == undefined) {
                scope.clearFilters();
            } else {
                _updateList();
            }
        };
        //end scope----------------------------------------

        var _getSelectedCount = function() {
            var result = 0;
            var arrItems = scope.baseList || [];
            arrItems.forEach(function(oEntity) {
                if (oEntity.checked) {
                    result++;
                }
            });
            return result;
        };

        //todo unselectAll and filters should be done in listcontroller
        scope.unselectAll = function() {
            var arrItems = scope.baseList || [];
            arrItems.forEach(function(oEntity) {
                oEntity.checked = false;
            });
            scope.selectedCount = 0;
        };

        var _afterLoad = function() {
            _afterLoadCallBack(scope.baseList);
            scope.selectedCount = _getSelectedCount();
            $activityIndicator.stopAnimating();
        };

        var _staticQParams = '';
        var _load = function(qParams) {
            _staticQParams = qParams;
            scope.isLoading = true;
            $activityIndicator.startAnimating();
            alertify.closeAll();
            return _baseService.loadDependencies().then(function(data) {
                _setFilterOptions();
                _fillCatalogs();
                return _updateList();

                // scope.$evalAsync(function() {
                // });
            });
        };


        var _loadByParentKey = function(parentType, parentKey) {
            $activityIndicator.startAnimating();
            alertify.closeAll();

            return _baseService.loadDependencies().then(function(data) {
                _setFilterOptionsByParent(parentType, parentKey);
                _fillCatalogs();
                return _updateList();

                // scope.$evalAsync(function() {
                // });
            });
        };

        var _fillCatalogs = function() {
            //for filters:
            _filters.forEach(function(filter) {

                var theCatalog = 'the' + capitalizeFirstLetter(filter);
                if (theCatalog.slice(-1) != 's') {
                    theCatalog += 's';
                }
                if (_baseService.catalogs[filter]) {
                    scope[theCatalog] = _baseService.catalogs[filter].getAll();
                    scope[theCatalog].unshift({
                        id: null,
                        Value: 'All'
                    });
                }

            });
        };

        function capitalizeFirstLetter(sWord) {
            var result = sWord.charAt(0).toUpperCase() + sWord.slice(1).toLowerCase();
            return result;
        }

        var _setFilterOptions = function() {

            scope.filterOptions = localStorageService.get(_filterStorageName);

            if (!scope.filterOptions) {
                scope.clearFilters();
            } else {
                scope.filterOptions = JSON.parse(scope.filterOptions);
            }

        };

        var _setFilterOptionsByParent = function(parentType, parentKey) {
            _setFilterOptions();
            scope.filterOptions.parentField = parentType + 'Key';
            scope.filterOptions.parentKey = parentKey;
        };

        var _persistFilter = function() {
            localStorageService.set(_filterStorageName, JSON.stringify(scope.filterOptions));
        };

        var _makeQueryParameters = function() {
            var result = '?';

            for (var prop in scope.filterOptions) {
                if (scope.filterOptions.hasOwnProperty(prop)) {
                    result += prop + '=' + scope.filterOptions[prop] + '&';
                }
            }

            result += _staticQParams;

            return result;
        };

        var _updateList = function() {
            $activityIndicator.startAnimating();
            scope.isLoading = true;

            var perPage = scope.filterOptions.perPage;
            var page = scope.filterOptions.page;

            var queryParameters = _makeQueryParameters();

            return _baseService.getFilteredPage(perPage, page, queryParameters).then(function(data) {
                scope.baseList = data.Result;

                scope.filterOptions.itemsCount = data.AdditionalData.total_filtered_items;
                scope.filterOptions.totalItems = data.AdditionalData.total_items;
                _persistFilter();

                for (var i = 0; i < scope.baseList.length; i++) {
                    var current = scope.baseList[i];
                    current.itemNumber = (scope.filterOptions.page - 1) * scope.filterOptions.perPage + i + 1;
                }
                _afterLoad();
                scope.isLoading = false;
            });

        };

        scope.clearFilters = function() {
            scope.filterOptions = {
                perPage: _perPage,
                page: 1,
                itemsCount: 0
            };

            _filters.forEach(function(filter) {
                scope.filterOptions['filter' + capitalizeFirstLetter(filter)] = null;
            });

            _persistFilter();
            // _updateList();
        };

        // Public baseController API:////////////////////////////////////////////////////////////
        var oAPI = {
            load: _load,
            loadByParentKey: _loadByParentKey
                // unselectAll: _unselectAll
        };
        return oAPI;
    };
});
