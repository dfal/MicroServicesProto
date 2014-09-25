var singleton = function singleton(){
	var store = {};

	this.save = function(key, data, callback){
		this.remove(key);
		store[key] = data;
		this.success(callback);
	};

	this.remove = function(key, callback){
		delete store[key];
		this.success(callback);
	};

	this.get = function(key, callback){
		if (!store[key]){
			callback('Entity with key "' + key + '" is not found.');
		}
		this.success(callback, store[key]);
	};

	this.getAll = function(orderBy, desc, callback){
		var values = [];
		for(var key in store) {
			values[values.length] = store[key];
		}

		values.sort(function(a, b){
			if (a[orderBy] == b[orderBy]) return 0;
			if (a[orderBy] > b[orderBy]) return 1;
			return -1;
		});

		if (desc) values.reverse();

		this.success(callback, values);
	};

	this.success = function(callback, result){
		if (callback) callback(null, result);
	};
};

singleton.instance = null;
singleton.getInstance = function(){
	if(this.instance === null){
		this.instance = new singleton();
	}
	return this.instance;
}

module.exports = singleton.getInstance();