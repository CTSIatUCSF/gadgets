
window.UCSF 	= window.UCSF || {};
UCSF.UI 		= UCSF.UI || {};

/**
 * HideShow a UCSF Visibility Control 
 * 
 * Has two toggle buttons ( hide ) and ( show ) 
 * which manage a value in appData "VISIBLE".
 * "VISIBLE" determines who can see the app via
 * a container hack.
 * 
 * THIS CONTROL REQUIRES CUSTOM MODS TO THE SHINDIG SERVER
 * 
 * */
UCSF.UI.Hideshow = function(){
//sets up the Hideshow object
	$(document).ready(function(){ os.ready(function(){
		
		$('#hideshow').html(
				'<div class="hideshow_message"></div>'+
				'<div class="hideshow_control"><span class="hide">Hide</span> | <span class="show">Show</span></div>'
				);
		
	    function getHideShow(){
			osapi.appdata.get(
				{ 	'userId': '@viewer',
					/*'groupId': '@self',*/
					'appId':'@app',
					'fields': ['VISIBLE']
				}).execute(function(response){
					console.log( '#hideshow .get', os.osapi.getViewerFromResult(response) , response );
					
					var visible = ( (os.osapi.getViewerFromResult(response).VISIBLE || false ) === 'Y');
					
	    			// nj added 2-8-11
	    			if(visible === true) {
	    				gadgets.pubsub.publish("VISIBLE", "Y");
	    			}
	    			if(visible === false) {
	    				gadgets.pubsub.publish("VISIBLE", "N");
	    			}
					
					//the default should be to hide
					$('#hideshow').attr('value', visible ? 'hidden' : 'visible' );
					
					//$('#hideshow .hideshow_message').text(
						//visible ?
							//'This section is VISIBLE to UCSF' :
							//'This section is HIDDEN from UCSF' 
							//);
							
					$('#hideshow .hideshow_message').text(
						visible ?
							'This section is VISIBLE to the public' :
							'This section is HIDDEN from the public' 
							);							
					
					if(visible){
						$('#hideshow .hideshow_message').addClass('visible_message').removeClass('hidden_message');
						$('#hideshow .show').addClass('selected').unbind('click', showHideShow );
						$('#hideshow .hide').removeClass('selected').unbind('click', hideHideShow ).click( hideHideShow );
						
					}else{
						$('#hideshow .hideshow_message').addClass('hidden_message').removeClass('visible_message');
						$('#hideshow .hide').addClass('selected').unbind('click', hideHideShow);
						$('#hideshow .show').removeClass('selected').unbind('click', showHideShow ).click( showHideShow );
					}
					
					gadgets.window.adjustHeight();
					
				});
	    }
	    
	    function hideHideShow(){
	    	updateHideShow('N');
	    }
	    
	    function showHideShow(){
	    
	    	//if( confirm("Are you sure you would like to make this section visible to UCSF?")){
	    	if( confirm("Are you sure you would like to make this section visible to the public?")){
	    		updateHideShow('Y');
	    	}else{
	    		console.log( 'user canceled: showHideShow');
	    	}
	    }
	    
	    getHideShow();
	    
	    function updateHideShow(show){
	    	var value = (show === 'Y') ? 'Y' : 'N';
    	
	    	osapi.appdata.update(
	    		{ 	'userId': '@viewer',
	    			/*'groupId': '@self',*/
					'appId':'@app',
					'data': { 'VISIBLE': value.toString() }
				}).execute(function(response){
					console.log( '#hideshow .update', os.osapi.getViewerFromResult(response), response );
					getHideShow();
				});
	    	
	    	// nj added 2-8-11
	    	gadgets.pubsub.publish("VISIBLE", value.toString());
	    };
	    
	    
	    
	});});//jquery.ready os.ready
};
UCSF.UI.Hideshow();

/**
 * MoreLess - a control to toggle the display of a DIV
 * 
 * HTML SPECIFIC CODE REQUIRED
 * 
 */
UCSF.UI.MoreLess = function(){
	//sets up the Moreless object
	$(document).ready(function(){ 
			
		$('.moreless').toggle(
				function(){
					$(this).trigger('show');
				},
				function(){
					$(this).trigger('hide');
				});
		
		$('.moreless').bind('show', function(){
			$(this).text('( less... )')
				.parents('.section')
				.children('.roundbox')
				.slideDown('fast', function(){ 
						gadgets.window.adjustHeight();
						});
		});
		
		$('.moreless').bind('hide', function(){
			$(this).text('( more... )')
				.parents('.section')
				.children('.roundbox')
				.slideUp('fast', function(){
					gadgets.window.adjustHeight();
					});
		});
		
	});/*document.ready*/
};

/**
 * DebugNav - provides a quick navigation menu for the control to navigate different views.
 * 
 * TODO: slim down the menu, and have it render based on url param.
 * 
 */
UCSF.UI.DebugNav = function(){
	//sets up the DebugNav object
	$(document).ready(function(){ os.ready(function(){
		
		function renderNav(){
			var supported_views = gadgets.views.getSupportedViews();
			var views = {};
			//console.log('gadgets.views.getSupportedViews()', gadgets.views.getSupportedViews(), JSON.stringify( gadgets.views.getSupportedViews() ) );
			var nav = $('<ul/>');
			
			//add this so that when i am on a container that does not naitively support canvas.owener, it's still accessible.
			views["canvas.owner"] = { "name_":"canvas.owner"  };
			views["update"] = { "name_":"update"  };
			
			//getting rid of the dups
			$.each(supported_views, function(){ views[this.name_] = this; });
			
			$.each(views, function(){
				$(nav).append( '<li>' + this.name_ + '</li>' );	
			});
			
			$('.viewnav').append("<div style='font-weight:bold;font-size:1.1em;'>Navigation</div>");
			$('.viewnav').append(nav);
			$('.viewnav').wrap("<div class='roundbox'/>");
			
			$('.viewnav li')
				.click(function(){
						gadgets.views.requestNavigateTo( $(this).text() );
				});
				
			gadgets.window.adjustHeight();
		}
		renderNav();

	});});//jquery.ready os.ready
};

UCSF.UI.DebugParams = function(){
	//sets up the DebugParams object
	$(document).ready(function(){ os.ready(function(){

		function renderParams(){
			var params = gadgets.views.getParams();
			var ul = $('<ul/>');
			var c = 0;
			console.log( params );
			
			
			for(p in params){
				$(ul).append( '<li><label>' + p + ' =</label><span class="pvalue">'+params[p]+'</span></li>' );
				c++;	
			}
			if(c>0){
				$('.params').append("<div style='font-weight:bold;font-size:1.1em;'>Params</div>");
				$('.params').append(ul);
				$('.params').wrap("<div class='roundbox'/>");
				gadgets.window.adjustHeight();
			}
			
		}
		renderParams();	
		
	});});//jquery.ready os.ready
};