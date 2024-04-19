/* --------------------------------------------------------------------
 * Drolex FileStyle - A jQuery plug-in
 * Author: Drolex LLC, web@drolex.net
 * Copyright (c) 2013 Drolex LLC
 * Licensed under The MIT License (MIT) [http://opensource.org/licenses/MIT]
 * --------------------------------------------------------------------
 */
 var file = {
	maxlength:20, // maximum length of filename before it's trimmed
		
	convert:function(){
		// Convert all file type inputs.
		$('input[type=file]').each(function(){
			$(this).wrap('<div class="file" />');
			$(this).parent().prepend('<div>Browse&#8230;</div>');
			$(this).parent().prepend('<span id="fileplaceholder">No file chosen</span>');
			$(this).fadeTo(0,0);
			$(this).attr('size', '50'); // Use this to adjust width for FireFox.
			$(this).width($(this).parent().width());
			$(this).height($(this).parent().height());
		});
	},
	
	update:function(x){
		// Update the filename display.
		var filename = x.val().replace(/^.*\\/g,'');
		if(filename.length > this.maxlength){
			trim_start = this.maxlength/2-1;
			trim_end = trim_start+filename.length-this.maxlength+1;
			filename = filename.substr(0,trim_start)+'&#8230;'+filename.substr(trim_end);
		}
		if(filename == '')
			filename = 'No file chosen';
		x.siblings('span').html(filename);
	}
}

$(document).ready(function(){
	file.convert();
	$('input[type=file]').change(function(){
		file.update($(this));
	});
});