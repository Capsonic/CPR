'use strict';

/**
 * @ngdoc service
 * @name mainApp.CRUDServices
 * @description
 * # CRUDServices
 * Service in the mainApp.
 */
angular.module('ScanPack.CRUDServices', [])

.service('userService', function(crudFactory) {
    var crudInstance = new crudFactory({
        //Entity Name = WebService/API to call:
        entityName: 'User',

        catalogs: [],

        adapter: function(theEntity) {
            return theEntity;
        },

        adapterIn: function(theEntity) {},

        adapterOut: function(theEntity, self) {
            theEntity.Identicon = "";
            theEntity.Identicon64 = "";
        },

        dependencies: [

        ]
    });

    crudInstance.getByUserName = function(sUserName) {
        var _arrAllRecords = crudInstance.getAll();
        for (var i = 0; i < _arrAllRecords.length; i++) {
            if (_arrAllRecords[i].UserName == sUserName) {
                return _arrAllRecords[i];
            }
        }
        return {
            id: -1,
            Value: ''
        };
    };

    crudInstance.getUsersInRoles = function(arrRoles) {
        var _arrAllRecords = crudInstance.getAll();
        var result = [];
        for (var i = 0; i < _arrAllRecords.length; i++) {
            if (arrRoles.indexOf(_arrAllRecords[i].Role) > -1) {
                result.push(_arrAllRecords[i]);
            }
        }
        result.push(_arrAllRecords[0]);
        return result;
    };

    crudInstance.getByRole = function(sRole) {
        return crudInstance.customGet('getByRole/' + sRole);
    };

    return crudInstance;
})

.service('utilsService', function($filter) {
    // prueba
    var service = {};

    service.getFormattedValue = function(value, format) {
        if (value != null && value != '') {
            switch (format) {
                case 1: //Numeric
                    return $filter('number')(value, 2);
                case 2: //Currency
                    return '$ ' + $filter('number')(value, 2);
                case 3: //Percentage
                    return $filter('number')(value, 2) + '%';
                default:
                    return value;
            }
        }
        return value;
    };

    service.getFormattedEquality = function(equality) {
        switch (equality) {
            case 1:
                return '>';
            case 2:
                return '<';
            case 3:
                return '+/-';
            default:
                return '';
        }
    };

    service.toJavascriptDate = function(sISO_8601_Date) {
        return sISO_8601_Date ? moment(sISO_8601_Date, moment.ISO_8601).toDate() : null;
    };

    service.toServerDate = function(oDate) {
        var momentDate = moment(oDate);
        if (momentDate.isValid()) {
            momentDate.local();
            return momentDate.format();
        }
        return null;
    };

    service.adaptHiddenForDashboards = function(theEntity) {
        var result = [];
        if (theEntity.HiddenForDashboardsTags) {
            for (var i = 0; i < theEntity.HiddenForDashboardsTags.length; i++) {
                var current = theEntity.HiddenForDashboardsTags[i];
                result.push(current.id);
            }
        }
        theEntity.HiddenForDashboardsTags = [];
        return result.join(',');
    };

    service.getDashboardsFromIds = function(sIDs, dashboardsCatalog) {
        if (sIDs != null && sIDs.length > 0) {
            var arrIDs = sIDs.split(',');
            return arrIDs.map(function(sID) {
                return dashboardsCatalog.getById(sID);
            });
        } else {
            return [];
        }
    };

    service.adaptUsersTags = function(theEntity) {
        var result = [];
        if (theEntity.OwnersTags) {
            for (var i = 0; i < theEntity.OwnersTags.length; i++) {
                var current = theEntity.OwnersTags[i];
                result.push(current.id);
            }
        }
        theEntity.OwnersTags = [];
        return result.join(',');
    };

    // service.getUsersFromIds = function(sIDs, usersCatalog) {
    //     if (sIDs != null && sIDs.length > 0) {
    //         var arrIDs = sIDs.split(',');
    //         return arrIDs.map(function(sID) {
    //             return usersCatalog.getById(sID);
    //         });
    //     } else {
    //         return [];
    //     }
    // };

    return service;
})

.service('PackageService', function(crudFactory) {

    var groupPacks = function(theEntity) {

        theEntity.Suma = 0;
        theEntity.visible = true;
        if (theEntity.nodes && theEntity.nodes.length > 0) {


            theEntity.nodes.forEach(function(node) {
                groupPacks(node);
            });

            //Count Singles.
            var singlesCount = theEntity.nodes
                .reduce(function(count, node) {
                    if (node.Type == 'SINGLE') {
                        count++;
                    }
                    return count;
                }, 0);

            //Leave only one single.
            var bSingleAdded = false;
            theEntity.nodes = theEntity.nodes
                .filter(function(node) {
                    if (node.Type != 'SINGLE') return true;

                    if (!bSingleAdded) {
                        bSingleAdded = true;
                        node.Count = singlesCount;
                        return true;
                    }
                    return false;
                });

            //Sum Container-Totals.
            var containersSum = theEntity.nodes
                .reduce(function(sum, node) {
                    if (node.Type == 'SINGLE') {
                        sum += node.Count;
                    } else {
                        sum += node.Suma;
                    }
                    return sum;
                }, 0);

            theEntity.Suma = containersSum;
        }
    };

    var crudInstance = new crudFactory({

        entityName: 'Package',

        catalogs: [],

        adapter: function(theEntity, self) {


            //Group by level:
            groupPacks(theEntity);

            return theEntity;
        },

        adapterIn: function(theEntity) {

        },

        adapterOut: function(theEntity, self) {
            theEntity.nodes = null;
        },

        dependencies: [

        ]
    });

    return crudInstance;

})

.service('IncidentService', function(crudFactory) {

    var crudInstance = new crudFactory({

        entityName: 'Incident',

        catalogs: [],

        adapter: function(theEntity, self) {
            return theEntity;
        },

        adapterIn: function(theEntity) {

        },

        adapterOut: function(theEntity, self) {

        },

        dependencies: [

        ]
    });

    return crudInstance;

})

.service('catAreaService', function(crudFactory) {

    var crudInstance = new crudFactory({

        entityName: 'catArea',

        catalogs: [],

        adapter: function(theEntity, self) {
            return theEntity;
        },

        adapterIn: function(theEntity) {

        },

        adapterOut: function(theEntity, self) {

        },

        dependencies: [

        ]
    });

    return crudInstance;

})

.service('catMaterialService', function(crudFactory) {

    var crudInstance = new crudFactory({

        entityName: 'catMaterial',

        catalogs: [],

        adapter: function(theEntity, self) {
            return theEntity;
        },

        adapterIn: function(theEntity) {

        },

        adapterOut: function(theEntity, self) {

        },

        dependencies: [

        ]
    });

    return crudInstance;

})

.service('catWorkOrderService', function(crudFactory) {

    var crudInstance = new crudFactory({

        entityName: 'catWorkOrder',

        catalogs: [],

        adapter: function(theEntity, self) {
            theEntity.DisplayWorkOrder = theEntity.ImportWONumber || ImportWONumber.substring(1);
            return theEntity;
        },

        adapterIn: function(theEntity) {

        },

        adapterOut: function(theEntity, self) {

        },

        dependencies: [

        ]
    });

    return crudInstance;

})

.service('catWorkstationService', function(crudFactory, localStorageService) {

    var crudInstance = new crudFactory({

        entityName: 'catWorkstation',

        catalogs: [],

        adapter: function(theEntity, self) {
            return theEntity;
        },

        adapterIn: function(theEntity) {

        },

        adapterOut: function(theEntity, self) {

        },

        dependencies: [

        ]
    });

    crudInstance.getLocalWorkstationConfig = function() {
        return localStorageService.get('LocalWorkstationConfig')
    };

    crudInstance.setLocalWorkstationConfig = function(workstation) {
        localStorageService.set('LocalWorkstationConfig', workstation);
    };

    crudInstance.removeLocalWorkstationConfig = function() {
        localStorageService.remove('LocalWorkstationConfig');
    };



    return crudInstance;

})

.service('BarcoderService', function(crudFactory) {

    var toBase64 = function(imageString) {
        var result = '';
        if (imageString.length > 0) {
            result = 'data:image/bmp;base64,' + imageString;
        }
        return result;
    }

    var crudInstance = new crudFactory({

        entityName: 'barcode',

        catalogs: [],

        adapter: function(theEntity, self) {
            theEntity.Barcode64 = toBase64(theEntity.BarcodeImage)
            return theEntity;
        },

        adapterIn: function(theEntity) {

        },

        adapterOut: function(theEntity, self) {

        },

        dependencies: [

        ]
    });

    return crudInstance;

});
