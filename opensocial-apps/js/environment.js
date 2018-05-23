window.UCSF 	= window.UCSF || {};

// To build this run use a spreadsheet and the following query:
// SELECT *  FROM [ProfilesRNS].[UCSF.].[Brand] order by Theme
UCSF.ConvertToProductionURL = function(profileURL) {
	var productionURL = profileURL;
	// DEV
	productionURL = profileURL.replace('http://stage-profiles.ucsf.edu/godzilla', 'https://www.researcherprofiles.org');
	productionURL = profileURL.replace('http://stage-profiles.ucsf.edu/lbl', 'https://profiles.lbl.gov');
	productionURL = profileURL.replace('http://stage-profiles.ucsf.edu/profiles_uc', 'https://profiles.ucbraid.org');
	productionURL = profileURL.replace('http://stage-profiles.ucsf.edu/ucdavis', 'https://profiles.ucdavis.edu');
	productionURL = profileURL.replace('http://stage-profiles.ucsf.edu/uci', 'https://profiles.icts.uci.edu');
	productionURL = profileURL.replace('http://stage-profiles.ucsf.edu/ucla', 'https://ucla.researcherprofiles.org');
	productionURL = profileURL.replace('http://stage-profiles.ucsf.edu/ucsd', 'https://profiles.ucsd.edu');
	productionURL = profileURL.replace('http://stage-profiles.ucsf.edu/ucsf', 'https://profiles.ucsf.edu');
	productionURL = profileURL.replace('http://stage-profiles.ucsf.edu/usc', 'https://profiles.sc-ctsi.org');
	
	// STAGE
	productionURL = profileURL.replace('https://stage.researcherprofiles.org', 'https://www.researcherprofiles.org');
	productionURL = profileURL.replace('https://stage-lbl.researcherprofiles.org', 'https://profiles.lbl.gov');
	productionURL = profileURL.replace('https://stage-profiles.ucbraid.org', 'https://profiles.ucbraid.org');
	productionURL = profileURL.replace('https://stage-profiles.ucdavis.edu', 'https://profiles.ucdavis.edu');
	productionURL = profileURL.replace('https://stage-profiles.icts.uci.edu', 'https://profiles.icts.uci.edu');
	productionURL = profileURL.replace('https://stage-ucla.researcherprofiles.org', 'https://ucla.researcherprofiles.org');
	productionURL = profileURL.replace('https://stage-ucsd.researcherprofiles.org', 'https://profiles.ucsd.edu');
	productionURL = profileURL.replace('https://stage-ucsf.researcherprofiles.org', 'https://profiles.ucsf.edu');
	productionURL = profileURL.replace('https://stage-usc.researcherprofiles.org', 'https://profiles.sc-ctsi.org');

	return productionURL;
};

//Clinical Trials
UCSF.ClinicalTrials = UCSF.ClinicalTrials || {};
// DEV
//UCSF.ClinicalTrials.seviceURL = 'https://clinicaltrialsapi.researcherprofiles.org/ClinicalTrialsApi/api/clinicaltrial';
// STAGE
UCSF.ClinicalTrials.seviceURL = 'http://profiles-qweb02.ist.berkeley.edu/ClinicalTrialsApi/api/clinicaltrial';
// PROD
//UCSF.ClinicalTrials.seviceURL = 'https://clinicaltrialsapi.researcherprofiles.org/ClinicalTrialsApi/api/clinicaltrial';



// BELOW IS ALL DEPRECATED!
// UNCOMMENT OUT THE BLOCK THAT IS APPROPRIATE FOR YOUR ENVIRONMENT!!!!

//For production
/************
var ENV_PROFILES_URL = 'http://profiles.ucsf.edu';
var ENV_LOCAL_URL = 'http://profiles.ucsf.edu';
**************/

//For staging
// DEPRECATED? Used by crosslinks var ENV_PROFILES_URL = 'http://stage-profiles.ucsf.edu/profiles_ucsf_29';
// DEPRECATED? Used by ProfileListTool to create a Chatter group var ENV_LOCAL_URL = 'http://stage-profiles.ucsf.edu';
//var ENV_API_URL = 'http://base.ctsi.ucsf.edu/experiments/clinical_trials_gadget_api/';

//Institution variables
//var ENV_INST_ABRV = 'UCSF';
//var ENV_INST_EMAIL = "<a href='mailto:profiles@ucsf.edu'>profiles@ucsf.edu</a>";
//var ENV_INST_PUBSCONTACT = "<a href='mailto:profiles@ucsf.edu?subject=Awarded Grant Information in UCSF Profiles'>contact us</a>";


