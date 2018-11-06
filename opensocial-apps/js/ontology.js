// figure out a way to generate this!
var RDF = function(property) {
	return "http://www.w3.org/1999/02/22-rdf-syntax-ns#" + property;
};

var RDFS = function(property) {
	return "http://www.w3.org/2000/01/rdf-schema#" + property;	
};

var BIBO = function(property) {
	return "http://purl.org/ontology/bibo/" + property;	
};

var FOAF = function(property) {
	return "http://xmlns.com/foaf/0.1/" + property;	
};

var VIVO = function(property) {
	return "http://vivoweb.org/ontology/core#" + property;
};

var PRNS = function(property) {
	return "http://profiles.catalyst.harvard.edu/ontology/prns#" + property;
};

var R2R = function(property) {
	return "http://ucsf.edu/ontology/r2r#" + property;
};

var ORNG = function(property) {
	return "http://orng.info/ontology/orng#" + property;
};

var GEO = function(property) {
	return 'http://www.w3.org/2003/01/geo/wgs84_pos#' + property;
};
// These are jsonld convenience functions

// returns one person into callback
var framePerson = function(data, callback) {
	osapi.jsonld.frame(data,  FOAF('Person'), callback)
};

// returns an array of people into callback
var framePeople = function(data, callback) {
	osapi.jsonld.frameArray(data,  FOAF('Person'), callback)
};

var getPropertyAsArray = function(obj, prop) {
	if (obj[prop] instanceof Array) {
		return obj[prop];
	}
	else if (obj[prop]) {
		return [obj[prop]];
	}
	else {
		return [];
	}
};

var getJSONItemId = function(obj) {
	// sometimes these are deeper
	return obj instanceof Array ? obj["@id"] : obj;
}
