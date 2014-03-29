// figure out a way to generate this!

var RDFS = function(property) {
	return "http://www.w3.org/2000/01/rdf-schema#" + property;	
}

var BIBO = function(property) {
	return "http://purl.org/ontology/bibo/" + property;	
}

var FOAF = function(property) {
	return "http://xmlns.com/foaf/0.1/" + property;	
}

var VIVO = function(property) {
	return "http://vivoweb.org/ontology/core#" + property;
};

var PRNS = function(property) {
	return "http://profiles.catalyst.harvard.edu/ontology/prns#" + property;
};

