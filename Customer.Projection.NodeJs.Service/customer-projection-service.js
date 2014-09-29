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

var processedEvents = [];
var redeliveredQueries = [];

var conn = amqp.createConnection({ host: qeueHost, heartbeat: 60 }, {
	reconnect: false,
	//reconnectBackoffStrategy: 'liner',
	//reconnectBackoffTime: 10000
});

conn.on('ready', function () {
	
	logger.trace('Connection with ' + qeueHost + ' is established');
	
	startEventHandling(conn, eventHandler);
	
	startQueryHandling(conn, queryHandler);
});

conn.on('error', function (err) {
	logger.error(err);
});

var startQueryHandling = function (conn, queryHandler) {
	
	conn.exchange(queryExchangeName, { type: 'fanout', durable: false, autoDelete: false }, function (exchange) {
		logger.trace('Exchange ' + queryExchangeName + ' is ready.');
		
		conn.queue(queryQueueName, { durable: false, exclusive: false, autoDelete: false }, function (queue) {
			queue.bind(exchange, '');
			
			logger.trace(queryQueueName + ' is bound to ' + queryExchangeName);
			
			queue.subscribe({ack:true}, function (payload, headers, deliveryInfo, msg) {
				logger.info(" [x] %s: %s", deliveryInfo.type, payload.data.toString('utf8'));
				
				var body = JSON.parse(payload.data);
				if (body.correlationId && processedEvents.filter(function(e) {
					return e.correlationId == body.correlationId;
				}).length == 0) {
					logger.warn('Requeuing query');
					redeliveredQueries[deliveryInfo.messageId] = 1 + (redeliveredQueries[deliveryInfo.messageId] ? redeliveredQueries[deliveryInfo.messageId] : 0);
					
					if (redeliveredQueries[deliveryInfo.messageId] > 5)
						msg.reject(false);
					else
						setTimeout(function() { msg.reject(true); }, 500); //requeue
					
					return;
				}

				delete redeliveredQueries[deliveryInfo.messageId];

				queryHandler.handle(deliveryInfo.type, body, function (err, response) {
					var replyOptions = { correlationId: deliveryInfo.correlationId };
					if (err) {
						logger.error(err);
						replyOptions.type = 'ERROR';
						replyOptions.headers = { errorCode: err.message };
						response = err.message;
					} else if (response === null)
						replyOptions.type = 'NULL';
					
					conn.publish(deliveryInfo.replyTo, response, replyOptions, function (e) {
						if (e) logger.error(e);
					});
					msg.acknowledge();
				});
			});
		});
	});
};

var startEventHandling = function (conn, eventHandler) {
	conn.exchange(eventExchangeName, { type: 'fanout', durable: true, autoDelete: false }, function (exchange) {
		logger.trace('Exchange ' + eventExchangeName + ' is ready.');
		
		conn.queue(eventQueueName, { durable: true, exclusive: false, autoDelete: false }, function (queue) {
			queue.bind(exchange, '');
			logger.trace(eventQueueName + ' is bound to ' + eventExchangeName);
			
			queue.subscribe(function (payload, headers, deliveryInfo) {
				logger.info(' [x] %s: %s', deliveryInfo.type, payload.data.toString('utf8'));
				
				var body = JSON.parse(payload.data);
				eventHandler.handle(deliveryInfo.type, body, function (err) {
					if (err) {
						body.err = err;
						logger.error(err);
					}
					
					processedEvents.push(body);
				});
			});
		});
	});
};
