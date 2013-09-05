window.UCSF 	= window.UCSF || {}; 
UCSF.UI = UCSF.UI || {};


/*
//DEPRICATED

UCSF.UI.applyMoreLess = function(){
	$(document).ready(function(){
		$('.moreless').toggle(
			function(){
				$(this).text('( less... )')
					.parents('.section')
					.children('.roundbox')
					.slideDown('fast', function(){ 
							gadgets.window.adjustHeight();
							});
			},
			function(){
				$(this).text('( more... )')
					.parents('.section')
					.children('.roundbox')
					.slideUp('fast', function(){
						gadgets.window.adjustHeight();
						});
			});
	});
}; //applyMoreLess
UCSF.UI.applyMoreLess();

UCSF.UI.renderNav = function(){
	$(document).ready(function(){
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
	});
};
UCSF.UI.renderNav();

	
UCSF.UI.renderParams = function(){
	$(document).ready(function(){
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
		
	});
};
UCSF.UI.renderParams();


UCSF.UI.applyQuestions = function(){
	$(document).ready(function(){
		
		$('.question fieldset').each(function(){ 
			
			var question = $(this);
			
			//apply event handler for radio buttons
			$(this).find('input[type=radio]').change(function(){
				var str = [];
				$(question).find('input:checked').each(function(){
					str.push($(this).val());
				}); 
				//console.log( this.name, this.value, str );
				var map = gadgets.json.parse( "{ \"" + this.name + "\" : \"" + str + "\" }" );
				//console.log(map);
				updateAppData( map );				
			});
			
			//apply event handler for checkboxes buttons
			$(this).find('input[type=checkbox]').change(function(){
				var str = [];
				$(question).find('input:checked').each(function(){
					str.push($(this).val());
				}); 
				str = str.join(", ");
				//console.log( this.name, this.value, str );
				var map = gadgets.json.parse( "{ \"" + this.name + "\" : \"" + str + "\" }" );
				//console.log(map);
				updateAppData( map );
			});
			
			//apply event handler for input and textareas
			$(this).find('input[name], textarea[name]')
				.not('[type=checkbox]').not('[type=radio]')
				.change(function(){
					
					
					var str = $(this).val() || "";
					console.log( $(this), str );
					
					
					var map = gadgets.json.parse( "{ \"" + $(this).attr('name') + "\" : \"" + str + "\" }" );
					console.log(map);
					updateAppData( map );
				
				});
			 
		});//.question .fieldset
	});//ready
};//applyQuestions
UCSF.UI.applyQuestions();


*/