var express = require('express');

var server = express();
server.set('port', 3000);
server.use(express.static(__dirname + '/public'));
server.listen(server.get('port'), function() {
  console.log('Express server listening on port ' + server.get('port'));
});

