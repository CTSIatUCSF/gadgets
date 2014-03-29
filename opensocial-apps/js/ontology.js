// figure out a way to generate this!
var RDFS = {};
RDFS.ns = "http://www.w3.org/2000/01/rdf-schema#";
RDFS.label = RDFS.ns + "label;"

var FOAF = {}
FOAF.ns = "http://xmlns.com/foaf/0.1/";
FOAF.firstName = FOAF.ns + "firstName";
FOAF.lastName = FOAF.ns + "lastName";

var VIVO = {};
VIVO.ns = "http://vivoweb.org/ontology/core#";
VIVO.email = VIVO.ns + "email";
VIVO.preferredTitle = VIVO.ns + "preferredTitle";
VIVO.authorInAuthorship = VIVO.ns + "authorInAuthorship";
VIVO.linkedInformationResource = VIVO.ns + "linkedInformationResource";

var PRNS = {};
PRNS.ns = "http://profiles.catalyst.harvard.edu/ontology/prns#";
PRNS.fullName = PRNS.ns + "fullName";
PRNS.coAuthorOf = PRNS.ns + "coAuthorOf";
PRNS.similarTo = PRNS.ns + "similarTo";