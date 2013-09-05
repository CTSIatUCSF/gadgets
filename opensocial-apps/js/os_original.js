/**
 * a small framework to simplify opensocial
 * author: justin kruger
 */
if( typeof(console) == "undefined" ){ window.console = { "log" :function(){}, "warning" :function(){}, "error" :function(){} }; }

window.os = window.os || {}; 	//opensocial helpers
os.isReady = false;		//need to wait for the container to load **slow
os.readyList = [];
os.Viewer = os.Viewer || {};
os.Owner = os.Owner || {};

os.osapi = os.osapi || {};

os.makeReady = function(){
	os.isReady = true;
	os.execReady();
}

os.execReady = function(){
	//execute all functions stored on stack
	while( os.readyList.length ){
		//like pop but pulls from the front of the stack fifo
		var fn = os.readyList.shift();
		if( typeof(fn) == 'function'){
			fn.call();
		}
	}    
}

os.init = function(){
	try{
		var batch = osapi.newBatch().
		    add("viewer", osapi.people.getViewer() ).
		    add("owner", osapi.people.getOwner() );
		    
		    batch.execute(function(result){
		    	//TODO: check result for error
		    	
				  console.log('@viewer', result.viewer);
				  os.Viewer = result.viewer;
				  
				  console.log('@owner', result.owner);
				  os.Owner = result.owner;
				  
				  os.initComplete();
				});
	
	}catch(e){
		console.log('os.init failed', e);
	}
	
};

os.initComplete = function(){
	os.makeReady();
};

/**
wrapper function that only executes when opensocial is ready.
*/
os.ready = function(fn){
	if(os.isReady){
		fn.call();
		return undefined;
	}else{
		os.readyList.push(fn);    	
	}
}

gadgets.util.registerOnLoadHandler(os.makeReady);
// gadgets.util.registerOnLoadHandler(os.init);  //use this to force the @owner and @viewer to be set., makes 2 web requests though ;-(

os.osapi._getFirstPerson = function(result){
	var person = {};
	for( p in result){
		person = result[p];
		break;
	}
	return person;
};

os.osapi.getViewerFromResult = function(result){
	return os.osapi._getFirstPerson(result);
};

os.osapi.getOwnerFromResult = function(result){
	return os.osapi._getFirstPerson(result);
};



os.ready( function(){
	gadgets.window.adjustHeight();
	
	
});