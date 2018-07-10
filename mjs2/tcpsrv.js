//! Gtm TCP Gateway Klasse
var GtmTcpServer = function() {
	var m_Server = 0;
	var m_Client = [];
	
	this.Config = {
		Port: 4200
	};

	this.run = function() {
		var net = require('net');		
		// server already running?
		if (0 !== m_Server) { return -1; }
		// initialise server instance
		m_Server = net.createServer(OnConnection);
		m_Server.GTS = this;
		// hook event handlers
		m_Server.on('error', OnError);
		m_Server.on('close', OnClose);
		m_Server.on('listening', OnListening);
		// start listening for clients
		try{
			m_Server.listen(this.Config.Port);
		}catch(ex){
			console.log(ex);
			return -2;
		}

		return 0;
	};
	
	function OnError(e)
	{
		console.log("server error: " + e);
	}
	
	function OnClientError(e)
	{
		console.log("client error: " + e);		
	}
	
	function OnListening()
	{
		console.log("server now listening for connections...");
	}
	
	function OnConnection(cs)
	{
		console.log("new connection from " + cs.remoteAddress);
		
		cs.on('end',OnEnd);
		cs.on('data',OnData);
		cs.on('error',OnClientError);
		cs.on('close',OnClientClose);

		m_Client[cs] = { 
			address: cs.remoteAddress 
		};
	}
	
	function OnClose()
	{
		console.log("server closed!");
	}
	
	function OnClientClose(had_error)
	{
		console.log("client closed! " + m_Client[this].address);
		delete m_Client[this];
	}
	
	function OnData(data)
	{
		console.log("OnData " + this.remoteAddress + " length " + data.length);

		var tcp = require('./tcppkg.js');
		var pkg = new tcp.TcpPackage();
		var sc  = pkg.parse(data);

		if(sc == 0){
			m_Server.GTS.emit('package', m_Client[this], pkg);
		}else{
			console.log("\tfailed to parse package!");
			console.log(data.toString());
		}
	}
	
	function OnEnd()
	{
		console.log("client ended! " + m_Client[this].address);
	}	
};

var events = require('events');
GtmTcpServer.prototype = new events.EventEmitter;
exports.GtmTcpServer = GtmTcpServer;
