/**
 * a small framework to simplify opensocial
 * author: justin kruger
 */
if( typeof(console) == "undefined" ){ var console = { "log" :function(){} }; }

var os = os || {}; 		//opensocial
os.isReady = false;		//need to wait for the container to load **slow
os.readyList = [];

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

os.ready( function(){
	gadgets.window.adjustHeight();
	
	
});