angular.module('WebUI')
.controller('CustomerListController', ['$scope', '$http', 'apiRoot', 'lastCorrelationId', 
	function ($scope, $http, apiRoot, lastCorrelationId) {
	var url = apiRoot + '/customers';
		if (lastCorrelationId.value)
			url += '?correlationId=' + encodeURIComponent(lastCorrelationId.value);
	$http.get(url).then(function(response){
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