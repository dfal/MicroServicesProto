angular.module('WebUI')
.controller('CustomerListController', ['$scope', '$http', 'apiRoot', function($scope, $http, apiRoot){
	$http.get(apiRoot + '/customers').then(function(response){
		$scope.customers = response.data;
		console.log(response);
	});
}])
.directive('backButton', function(){
	return {
		restrict: 'A',

		link: function(scope, element, attrs){
			element.bind('click', goBack);

			function goBack() {
				history.back();
				scope.$apply();
			}
		}
	};
});