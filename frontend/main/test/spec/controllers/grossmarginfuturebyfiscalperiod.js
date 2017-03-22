'use strict';

describe('Controller: GrossmarginfuturebyfiscalperiodCtrl', function () {

  // load the controller's module
  beforeEach(module('appApp'));

  var GrossmarginfuturebyfiscalperiodCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    GrossmarginfuturebyfiscalperiodCtrl = $controller('GrossmarginfuturebyfiscalperiodCtrl', {
      $scope: scope
      // place here mocked dependencies
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(GrossmarginfuturebyfiscalperiodCtrl.awesomeThings.length).toBe(3);
  });
});
