function TcpPackage(data)
{
	this.magic   = 0x23071981;
	this.version = 0x00000001;
	this.options = 0x00000000;
	this.datalen = 0x00000000;
	this.data    = "";
	
	this.compile = function() {
		var buf = new Buffer();
		buf.writeUint32LE(this.magic, 0x00);
		buf.writeUInt32LE(this.version, 0x04);
		buf.writeUInt32LE(this.options, 0x08);
		buf.writeUInt32LE(this.data.length, 0x0C);
		buf.write(this.data, 0x10, 'utf8');

		return buf;
	}

	this.parse = function(buffer) {
		if (buffer.length < 0x10) {
			return -1; 
		}		
		var _magic = buffer.readUInt32LE(0x00);
		if (this.magic != _magic) { 
			return -2; 
		}		
		this.version = buffer.readUInt32LE(0x04);
		this.options = buffer.readUInt32LE(0x08);
		this.datalen = buffer.readUInt32LE(0x0C);
		this.data    = buffer.toString('utf8',0x10);

		return 0;
	}

};

module.exports.TcpPackage = TcpPackage;   
