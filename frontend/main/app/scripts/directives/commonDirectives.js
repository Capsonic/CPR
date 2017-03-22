'use strict';

/**
 * @ngdoc directive
 * @name iqsApp.directive:hideWhenLoading
 * @description
 * # hideWhenLoading
 */
angular.module('CommonDirectives', []);
angular.module('CommonDirectives').directive('hideWhenLoading', function() {
    return {
        restrict: 'A',
        scope: {
            hideWhenLoading: '='
        },
        link: function postLink(scope, element, attrs) {
            element.css('visibility', 'hidden');
            scope.$watch("hideWhenLoading", function() {
                if (scope.hideWhenLoading) {
                    element.css('visibility', 'hidden');
                } else {
                    element.css('visibility', 'visible');
                }
            });
        }
    };
}).directive('attachmentsBox', function(FileUploader, appConfig, $http, $timeout, $activityIndicator) {
    return {
        restrict: 'E',
        scope: {
            ownerEntity: '=',
            printmode: '=',
            kind: '@',
            afterUpload: '&',
            readOnly: '=',
            whenChange: '&',
            afterDelete: '&',
            customListBind: '@',
            customFolderBind: '@'
        },
        template: '<div nv-file-drop nv-file-over uploader="uploader" class="AttachmentsBox" over-class="UploaderOverClass" style="background-color: rgb(221, 221, 221);padding: 4px;border: solid 2px gray;border-style: dotted;" ng-click="$event.stopPropagation();" ng-dblclick="addAttachment();$event.stopPropagation();"><input type="file" nv-file-select uploader="uploader" multiple style="display:none;" /><div class="noselect" ng-show="ownerEntity[attachmentsList].length==0 || ownerEntity[attachmentsList] == null">No files.</div><p class="input-group noselect" style="border-bottom: solid 1px #E5E5E5;width: 100%;" ng-repeat="attachment in ownerEntity[attachmentsList]"><a ng-hide="attachment.isForUpload" ng-href="{{baseURL}}attachment_download?directory={{attachment.Directory}}&fileName={{attachment.FileName}}&attachmentKind={{kind}}" href="" class="pull-left" style="display: block; top: 4px; position: relative;">{{attachment.FileName}}</a><span ng-show="attachment.isForUpload" class="glyphicon glyphicon-upload pull-left" style="padding: 3px 0;font-size: 14px;"></span><button ng-show="attachment.isForUpload" class="btn-link pull-left" style="display: block; top: 4px; position: relative;padding:0;">{{attachment.FileName}}</button><span class="btn glyphicon glyphicon-remove pull-right btn-sm" style="padding:0;"  ng-hide="printmode || readOnly" ng-click="removeAttachment(attachment, $index);$event.stopPropagation();"></span></p><input type="button" ng-hide="readOnly" class="btn btn-success btn-xs hidden-print" value="Add File" ng-click="addAttachment();$event.stopPropagation();" style="margin-top: 4px;margin-bottom: 0;" /></div>',
        compile: function compile(tElement, tAttrs, transclude) {
            return {
                pre: function preLink(scope, iElement, iAttrs, controller) {
                    scope.uploader = new FileUploader();
                    scope.attachmentsList = scope.customListBind || 'Attachments';
                    scope.attachmentsFolder = scope.customFolderBind || 'AttachmentsFolder';
                    scope.uploader.onAfterAddingFile = function(fileItem) {
                        if (!scope.ownerEntity[scope.attachmentsList]) {
                            scope.ownerEntity[scope.attachmentsList] = [];
                        }
                        scope.ownerEntity[scope.attachmentsList].push({
                            FileName: fileItem.file.name,
                            Directory: (scope.ownerEntity[scope.attachmentsFolder] || ''),
                            isForUpload: true
                        });
                        scope.whenChange({
                            oEntity: scope.ownerEntity
                        });
                    };
                    scope.uploader.onWhenAddingFileFailed = function(item, filter, options) {
                        // console.debug(item);
                        // console.debug(filter);
                        // console.debug(options);
                    };
                    scope.uploader.onSuccessItem = function(item, response, status, headers) {
                        var backendResponse = response;
                        if (!backendResponse.ErrorThrown) {
                            scope.ownerEntity[scope.attachmentsFolder] = backendResponse.ResponseDescription;
                            var theAttachment = scope.getAttachment(item.file.name);
                            delete theAttachment.isForUpload;
                        } else {
                            scope.ErrorThrown = true;
                            alertify.alert(backendResponse.ResponseDescription).set('modal', true);
                            console.debug(response);
                        }
                    };
                    scope.uploader.onErrorItem = function(item, response, status, headers) {
                        console.debug(item);
                        console.debug(response);
                        console.debug(status);
                    };
                    scope.uploader.onCompleteAll = function() {
                        if (!scope.ErrorThrown) {
                            scope.afterUpload({
                                oEntity: scope.ownerEntity
                            });
                        }
                    };
                    scope.uploader.onBeforeUploadItem = function(item) {
                        item.url = appConfig.API_URL + 'attachment?attachmentKind=' + (scope.kind || '') + '&targetFolder=' + (scope.ownerEntity[scope.attachmentsFolder] || '')
                    };
                },
                post: function(scope, iElement, iAttrs) {
                    scope.baseURL = appConfig.API_URL;
                    scope.$watch(function() {
                        return scope.ownerEntity;
                    }, function() {
                        if (scope.ownerEntity) {
                            var apiName = 'api_attachments';
                            if (scope.customListBind) {
                                apiName = 'api_' + scope.customListBind;
                            }
                            scope.ownerEntity[apiName] = {};
                            scope.ownerEntity[apiName].uploadFiles = function() {
                                scope.ErrorThrown = false;
                                if (scope.uploader.getNotUploadedItems().length > 0) {
                                    scope.uploader.uploadAll();
                                } else {
                                    scope.afterUpload({
                                        oEntity: scope.ownerEntity
                                    });
                                }
                            };
                            scope.ownerEntity[apiName].clearQueue = function() {
                                scope.uploader.clearQueue();
                            };
                        }
                    }, false);
                    scope.theElement = iElement;
                    scope.removeAttachment = function(attachment, index) {

                        if (attachment.isForUpload) {
                            scope.uploader.removeFromQueue(scope.getItem(attachment.FileName));
                            scope.ownerEntity[scope.attachmentsList].splice(index, 1);
                        } else {
                            alertify.confirm(
                                'This action cannot be undo, do you want to continue?',
                                function() {
                                    scope.$apply(function() {
                                        $activityIndicator.startAnimating();
                                        $http.get(appConfig.API_URL + 'attachment_delete?directory=' + attachment.Directory + '&fileName=' + attachment.FileName + '&attachmentKind=' + scope.kind).then(function(data) {
                                            var backendResponse = data;
                                            if (!backendResponse.ErrorThrown) {
                                                scope.ownerEntity[scope.attachmentsList].splice(index, 1);
                                                $timeout(function() {
                                                    alertify.success('File deleted successfully.');
                                                }, 100);
                                            } else {
                                                alertify.alert('An error has occurried, see console for more details.').set('modal', true);
                                                console.debug(response);
                                            }
                                            scope.afterDelete({
                                                oEntity: scope.ownerEntity
                                            });
                                            $activityIndicator.stopAnimating();
                                        }, function(data) {
                                            alertify.alert('An error has occurried, see console for more details.').set('modal', true);
                                            console.debug(data);
                                            $activityIndicator.stopAnimating();
                                        });
                                    });
                                });
                        }
                    };
                    scope.getItem = function(sName) {
                        for (var i = 0; i < scope.uploader.queue.length; i++) {
                            if (scope.uploader.queue[i].file.name == sName) {
                                return scope.uploader.queue[i];
                            }
                        }
                        return null;
                    };
                    scope.addAttachment = function() {
                        if (!scope.readOnly) {
                            $timeout(function() {
                                scope.theElement.find('input[type="file"]').click();
                            });
                        }
                    };
                    scope.getAttachment = function(sName) {
                        for (var i = 0; i < scope.ownerEntity[scope.attachmentsList].length; i++) {
                            if (scope.ownerEntity[scope.attachmentsList][i].FileName == sName) {
                                return scope.ownerEntity[scope.attachmentsList][i];
                            }
                        }
                        return null;
                    };
                }
            };
        }
    };
}).directive('modal', function() {
    return {
         template: '<div class="modal fade" tabindex="-1" id="{{modalId}}">'
                         + '<div class="modal-dialog">'
                             + '<div class="modal-content">'
                                 + '<div class="modal-header">'
                                     + '<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>'
                                     + '<h4 class="modal-title">{{title}}</h4>'
                                 + '</div>'
                                 + '<div class="modal-body">'
                                     + '<div ng-transclude></div>'
                                 + '</div>'
                                 + '<div class="modal-footer" ng-if="okLabel">'
                                     + '<button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>'
                                     + '<button type="button" class="btn btn-primary" ng-click="ok_click()">{{okLabel}}</button>'
                                 + '</div>'
                             + '</div>'
                         + '</div>'
                     + '</div>',
        restrict: 'E',
        transclude: true,
        replace: false,
        scope: {
            title: '@',
            modalId: '@',
            okLabel: '@'
        },
        controller: function($scope) {
            $scope.ok_click = function() {
                $scope.$broadcast('modal_ok');
            };
        }
    };
});
