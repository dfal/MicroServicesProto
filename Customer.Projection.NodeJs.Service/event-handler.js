var repo = require('./key-value-store.js');

module.exports = {
	handle: function (event, body, callback) {
		body.eventType = event;
		handlers[event] 
			? handlers[event](body, callback)
			: callback(new Error('Handler for event "' + event + '" is not found.'));
	}
};

var handlers = {
	'CustomerCreated': function onCustomerCreated(e, callback){
		repo.save(e.sourceId, {
			id: e.sourceId, 
			name: e.name, 
			vatNumber: e.vatNumber, 
			email: e.email, 
			version: e.sourceVersion, 
			events: [e]
		}, callback);
	},

	'CustomerDeleted': function onCustomerDeleted(e, callback){
		repo.remove(e.sourceId, callback);
	},

	'CustomerRenamed': function onCustomerRenamed(e, callback){
		repo.get(e.sourceId, function(err, customer){
			if (err) {
				callback(err);
				return;
			}
		
			customer.name = e.newName;
			customer.events.push(e);

			repo.save(customer.id, customer, callback);
		});
	},

	'CustomerEmailChanged': function onCustomerEmailChanged(e, callback){
		repo.get(e.sourceId, function(err, customer){
			if (err) {
				callback(err);
				return;
			}
		
			customer.email = e.newEmail;
			customer.events.push(e);

			repo.save(customer.id, customer, callback);
		});
	},

	'CustomerVatNumberChanged': function onCustomerVatNumberChanged(e, callback){
		repo.get(e.sourceId, function(err, customer){
			if (err) {
				callback(err);
				return;
			}
		
			customer.vatNumber = e.newVatNumber;
			customer.events.push(e);

			repo.save(customer.id, customer, callback);
		});
	}
};