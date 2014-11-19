
window.UCSF 	= window.UCSF || {};
UCSF.UI 		= UCSF.UI || {};

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
						//gadgets.window.adjustHeight();
						});
		});
		
		$('.moreless').bind('hide', function(){
			$(this).text('( more... )')
				.parents('.section')
				.children('.roundbox')
				.slideUp('fast', function(){
					//gadgets.window.adjustHeight();
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
				
			//gadgets.window.adjustHeight();
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
				//gadgets.window.adjustHeight();
			}
			
		}
		renderParams();	
		
	});});//jquery.ready os.ready
};