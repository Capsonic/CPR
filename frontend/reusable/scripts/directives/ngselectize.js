'use strict';

/**
 * @ngdoc directive
 * @name appApp.directive:ngSelectize
 * @description
 * # ngSelectize
 */
angular.module('appApp').directive('ngSelectize', function($timeout) {
    return {
        template: '<select></select>',
        restrict: 'E',
        scope: {
            theModel: '=',
            service: '=',
            // options: '=',
            placeholder: '@',
            labelField: '@',
            valueField: '@',
            beforeSave: '&',
            isDisabled: '=',
            maxItems: '@',
            allowCreate: '='
        },
        link: function postLink(scope, element, attrs, ngModel) {

            var loading = true;
            scope.options = [];

            scope.maxItems = scope.maxItems != undefined ? scope.maxItems : 1;

            var selectize = element.selectize({
                create: scope.allowCreate,
                sortField: scope.labelField,
                searchField: scope.labelField,
                placeholder: scope.placeholder,
                valueField: scope.valueField,
                labelField: scope.labelField,
                maxItems: scope.maxItems,
                persist: true,
                //Callbacks
                onChange: function(value) {
                    $timeout(function() {
                        // if (value == '') {
                        //     if (scope.theModel) {
                        //         scope.theModel.Value = '';
                        //     }
                        // } else 
                        if (value != '' && scope.options) {
                            if (scope.maxItems == 1) {
                                var selectedOption = scope.options.find(function(option) {
                                    return option[scope.valueField] == value;
                                });
                                if (selectedOption) {
                                    scope.theModel = selectedOption[scope.valueField];
                                    lastValue = scope.theModel;
                                }
                            } else {
                                scope.theModel = value;
                                lastValue = scope.theModel;
                            }
                        } else {
                            scope.theModel = null;
                            lastValue = scope.theModel;
                        }
                    });
                },
                onOptionAdd: function(value, data) {
                    if (!loading) {
                        var oNew = {
                            id: 0
                        };
                        oNew[scope.labelField] = value;
                        scope.beforeSave({ entity: oNew });
                        scope.service.save(oNew, scope.options).then(function(data) {
                            scope.theModel = data[scope.valueField];
                            lastValue = scope.theModel;
                            refresh();
                        });
                    }
                },
                onItemAdd: function(value, $item) {
                    // if (!loading) {
                    //     scope.service.save({
                    //         id: 0,
                    //         Value: value,
                    //         MetricKey: scope.metric.id
                    //     }, scope.scope.options).then(function(data) {
                    //         console.log(data, scope.metric);
                    //         scope.theModel = data.Result;
                    //     });
                    // }
                }
            })[0].selectize;

            var lastValue = scope.theModel;
            scope.$watch(function() {
                return !loading && lastValue != scope.theModel ? scope.theModel : false;
            }, function(newValue) {
                if (newValue != false) {
                    refresh();
                }
            });

            scope.$watch('isDisabled', function(newValue) {
                if (newValue) {
                    selectize.disable();
                } else {
                    selectize.enable();
                }
            });

            function load() {
                loading = true;
                scope.service.loadEntities(true).then(function(data) {
                    scope.options = data.Result;
                    refresh();
                });
            }

            function refresh() {
                loading = true;
                loadOptions(scope.options ? scope.options : []);
                loadSelected(scope.theModel);
                loading = false;
            }

            function loadSelected(values) {
                selectize.clear();
                if (values != undefined) {
                    var arrValues = values.split(',');
                    arrValues.forEach(function(value) {
                        var item = scope.options.find(function(option) {
                            return option[scope.valueField] == value;
                        });
                        if (item) {
                            selectize.addItem(value, true);
                        }
                    });
                }
                selectize.refreshItems();
                $timeout(function() {
                    selectize.close();
                }, 100);
            }

            function loadOptions(newOptions) {
                scope.options = newOptions;
                selectize.clearOptions();
                if (newOptions) {
                    selectize.addOption(newOptions);
                }
                // selectize.refreshOptions();
            }

            load();

            // scope.$on('DeleteMetricYear', function(s, metricYear) {
            //     if (metricYear && metricYear.id) {
            //         alertify.confirm(
            //             'Are you sure you want to delete this Year: ' + metricYear.Value + '?',
            //             function() {
            //                 scope.$apply(function() {
            //                     scope.service.remove(metricYear, scope.options).then(function(data) {
            //                         load(scope.metric);
            //                     });

            //                 });
            //             });
            //     } else {
            //         alertify.message('Nothing selected');
            //     }
            // });

        }
    };
});
