angular.module('WebUI')
.controller('CustomerDetailController', ['$scope', '$routeParams', '$http', '$location', 'apiRoot', function($scope, $routeParams, $http, $location, apiRoot){
	$scope.customer = {};
	$http.get(apiRoot + '/customers/' + $routeParams.id).then(function(response){
		$scope.customer = response.data;
		console.log(response);
	});

	$scope.delete = function(){
		$http['delete'](apiRoot + '/customers/' + $routeParams.id).then(function(){
			$location.path('/customers');
		});
	};

	$scope.save = function(){
		$http.put(apiRoot + '/customers/' + $routeParams.id, $scope.customer).then(function(){
			$location.path('/customers');
		});
	};

}]);

angular.module('WebUI')
.controller('NewCustomerController', ['$scope', '$http', '$location', 'apiRoot', function($scope, $http, $location, apiRoot){
	$scope.customer = {};

	$scope.save = function(){
		console.log($scope.customer);
		$http.post(apiRoot + '/customers', $scope.customer).then(function(){
			$location.path('/customers');
		});
	};
}]);