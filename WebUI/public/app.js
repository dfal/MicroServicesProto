angular.module('WebUI', [
	'ngRoute'
])
.config(['$routeProvider', function($routeProvider){
	$routeProvider
		.when('/customers',{
			templateUrl: 'customer-list.html',
			controller: 'CustomerListController'
		})
		.when('/customers/new',{
			templateUrl: 'customer-detail.html',
			controller: 'NewCustomerController'
		})
		.when('/customers/:id',{
			templateUrl: 'customer-detail.html',
			controller: 'CustomerDetailController'
		}).
		otherwise({
			redirectTo: '/customers'
		});
}])
.constant('apiRoot', 'http://localhost:33651/api')
.value('lastCorrelationId', { value: null });