var amqp = require('amqp');
var logger = require('./logger.js');

var queryHandler = require('./query-handler.js');
var eventHandler = require('./event-handler.js');

var qeueHost = "localhost";
var queryQueueName = "customer.query.queue";
var queryExchangeName = "customer.query.exchange";
var eventQueueName = "customer.event.queue";//TODO: this one should be unique per instance
var eventErrorQueueName = "customer.event.error.queue";//TODO: this one should be unique per instance
var eventExchangeName = "customer.event.exchange";

var conn = amqp.createConnection({ host: qeueHost, heartbeat: 60 },{
	reconnect: false,
	//reconnectBackoffStrategy: 'liner',
	//reconnectBackoffTime: 10000
});

conn.on('ready', function(){
	
	logger.trace('Connection with ' + qeueHost + ' is established');
	
	startEventHandling(conn, eventHandler);

	startQueryHandling(conn, queryHandler);
});

conn.on('error', function(err){
	logger.error(err);
});

var startQueryHandling = function(conn, queryHandler){

	conn.exchange(queryExchangeName, {type:'fanout', durable: false, autoDelete: false}, function(exchange){
		logger.trace('Exchange ' + queryExchangeName + ' is ready.')
		
		conn.queue(queryQueueName, {durable:false, exclusive:false, autoDelete: false}, function(queue){
			queue.bind(exchange, '');

			logger.trace(queryQueueName + ' is bound to ' + queryExchangeName);

			queue.subscribe(function(msg, headers, deliveryInfo){
				logger.info(" [x] %s: %s", deliveryInfo.type, msg.data.toString('utf8'));

				var body = JSON.parse(msg.data);
				queryHandler.handle(deliveryInfo.type, body, function(err, response){
					if (err){ logger.error(err); return; }

					conn.publish(deliveryInfo.replyTo, response, {correlationId: deliveryInfo.correlationId}, function(){
						logger.info('Response sent to ' + deliveryInfo.replyTo + ' with correlationId ' + deliveryInfo.correlationId);
					});
				});
			});
		})
	});
};

var startEventHandling = function(conn, eventHandler){
	conn.exchange(eventExchangeName, {type: 'fanout', durable: true, autoDelete: false}, function(exchange){
		logger.trace('Exchange ' + eventExchangeName + ' is ready.');

		conn.queue(eventQueueName, {durable: true, exclusive: false, autoDelete: false}, function(queue){
			queue.bind(exchange, '');
			logger.trace(eventQueueName + ' is bound to ' + eventExchangeName);

			queue.subscribe(function(msg, headers, deliveryInfo){
				logger.info(' [x] %s: %s', deliveryInfo.type, msg.data.toString('utf8'));

				var body = JSON.parse(msg.data);
				eventHandler.handle(deliveryInfo.type, body, function(err){
					if (err) logger.error(err);
				});
			});
		});
	});
};
