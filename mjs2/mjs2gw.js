//! LFD NodeJs/GTM Gateway 

var tcp = require('./tcpsrv.js');
var srv = new tcp.GtmTcpServer();

srv.on('package', OnPackage);

srv.run();

function OnPackage(client, pkg)
{
	console.log("OnPackage from " + client);
	if (pkg.datalen > 0) {
		console.log(pkg.data);
	}
}
