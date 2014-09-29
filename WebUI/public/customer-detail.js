angular.module('WebUI')
.controller('CustomerDetailController', ['$scope', '$routeParams', '$http', '$location', 'apiRoot', 'lastCorrelationId', 
	function ($scope, $routeParams, $http, $location, apiRoot, lastCorrelationId) {
	$scope.customer = {};
	$http.get(apiRoot + '/customers/' + $routeParams.id).then(function (response) {
		$scope.customer = response.data;
		console.log(response);
	});
	
	$scope.delete = function () {
		$http['delete'](apiRoot + '/customers/' + $routeParams.id).then(function (response) {
			lastCorrelationId.value = response.data.correlationId;
			$location.path('/customers');
		});
	};
	
	$scope.save = function () {
		$http.put(apiRoot + '/customers/' + $routeParams.id, $scope.customer).then(function (response) {
			lastCorrelationId.value = response.data.correlationId;
			$location.path('/customers');
		});
	};
}]);

angular.module('WebUI')
.controller('NewCustomerController', ['$scope', '$http', '$location', 'apiRoot', 'lastCorrelationId',
	function ($scope, $http, $location, apiRoot, lastCorrelationId) {
	$scope.customer = {};
	
	$scope.save = function () {
		console.log($scope.customer);
		$http.post(apiRoot + '/customers', $scope.customer).then(function (response) {
			lastCorrelationId.value = response.data.correlationId;
			$location.path('/customers');
		});
	};
}]);