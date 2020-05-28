window.UCSF 	= window.UCSF || {};

var protocol = location.protocol;
var slashes = protocol.concat("//");
var host = slashes.concat(window.location.hostname)

var DEV = 'DEV';
var STAGE = 'STAGE';
var PROD = 'PROD';

if (host.includes("dev.") || host.includes("dev-")) {
	UCSF.Environment = DEV;
}
else if (host.includes("stage-")) {
	UCSF.Environment = STAGE;
}
else {
	UCSF.Environment = PROD;
}

UCSF.ENV_GADGET_URL = host + '/apps_godzilla';

// To build this run use a spreadsheet and the following query:
// SELECT *  FROM [ProfilesRNS].[UCSF.].[Brand] order by Theme
UCSF.ConvertToProductionURL = function(profileURL) {
	var productionURL = profileURL;

	if (UCSF.Environment == DEV) {
		// DEV
		productionURL = productionURL.replace('https://dev.researcherprofiles.org', 'https://www.researcherprofiles.org');
		//productionURL = productionURL.replace('http://stage-profiles.ucsf.edu/lbl', 'https://profiles.lbl.gov');
		productionURL = productionURL.replace('https://dev-uc.researcherprofiles.org', 'https://profiles.ucbraid.org');
		productionURL = productionURL.replace('https://dev-ucdavis.researcherprofiles.org', 'https://profiles.ucdavis.edu');
		productionURL = productionURL.replace('https://dev-uci.researcherprofiles.org', 'https://profiles.icts.uci.edu');
		productionURL = productionURL.replace('https://dev-ucla.researcherprofiles.org', 'https://profiles.ucla.edu');
		productionURL = productionURL.replace('https://dev-ucsd.researcherprofiles.org', 'https://profiles.ucsd.edu');
		productionURL = productionURL.replace('https://dev-ucsf.researcherprofiles.org', 'https://profiles.ucsf.edu');
		productionURL = productionURL.replace('https://dev-usc.researcherprofiles.org', 'https://profiles.sc-ctsi.org');
	}
	else if (UCSF.Environment == STAGE) {
		// STAGE
		productionURL = productionURL.replace('https://stage.researcherprofiles.org', 'https://www.researcherprofiles.org');
		//productionURL = productionURL.replace('https://stage-lbl.researcherprofiles.org', 'https://profiles.lbl.gov');
		productionURL = productionURL.replace('https://stage-profiles.ucbraid.org', 'https://profiles.ucbraid.org');
		productionURL = productionURL.replace('https://stage-profiles.ucdavis.edu', 'https://profiles.ucdavis.edu');
		productionURL = productionURL.replace('https://stage-profiles.icts.uci.edu', 'https://profiles.icts.uci.edu');
		productionURL = productionURL.replace('https://stage-ucla.researcherprofiles.org', 'https://profiles.ucla.edu');
		productionURL = productionURL.replace('https://stage-ucsd.researcherprofiles.org', 'https://profiles.ucsd.edu');
		productionURL = productionURL.replace('https://stage-ucsf.researcherprofiles.org', 'https://profiles.ucsf.edu');
		productionURL = productionURL.replace('https://stage-usc.researcherprofiles.org', 'https://profiles.sc-ctsi.org');		
	}

	return productionURL;
};

//Clinical Trials
UCSF.ClinicalTrials = UCSF.ClinicalTrials || {};

if (UCSF.Environment == PROD) {
	UCSF.ClinicalTrials.seviceURL = 'https://api.researcherprofiles.org/ClinicalTrialsApi/api/clinicaltrial';
}
else {
	UCSF.ClinicalTrials.seviceURL = 'https://stage-api.researcherprofiles.org/ClinicalTrialsApi/api/clinicaltrial';	
}

UCSF.GetGoogleAnaltyicsViewId = function() {
	if (host.includes(".ucsf.") || host.includes("-ucsf.")) {
		return "23439892"
	}
	else if (host.includes(".ucsd.") || host.includes("-ucsd.")) {
		return "89442630"
	}
	return "";
}



