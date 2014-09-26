var repo = require('./key-value-store.js');

module.exports = {
	handle: function (query, body, callback) {
		if (handlers[query])
			handlers[query](body, callback);
		else
			callback(new Error('Handler for query "' + query + '" was not found.'));
	}
};

var handlers = {
	'GetCustomer': function (q, callback) {
		repo.get(q.customerId, callback);
	},
	
	'GetAllCustomers': function (q, callback) {
		repo.getAll(q.orderBy, q.desc, callback);
	}
};