/**
 * Project Name:	jActivating
 * Project URI:	http://jactivating.sourceforge.net
 * Description:	Allows website users to interact with embedded content without 'activating' it first
 * Author:		David Muñoz <donraspu AT gmail DOT com>
 * Version:		1.1.2 - May 16, 2007
 * License:		This software is Public Domain (no rights reserved)
 *
 * Installation: 
 * 1) Upload embeddedcontent.js into some directory on your web server.
 * 2) Include the JavaScript file in the <head> of your document like this:
 * 		<script type="text/javascript" src="embeddedcontent.js" defer="defer"></script>
 */
var jActivating = 
{

	// Checks for Internet Explorer
	isMSIE : (document.all && !window.opera) ? true : false,
	
	
	/**
	 * Activates embedded content reinserting it
	 * @return void
	 */
	reinsertContent : function()
	{	
	var totalNodes = new Array(3);
		totalNodes['object'] = document.getElementsByTagName('object').length;
		totalNodes['embed'] = document.getElementsByTagName('embed').length;
		totalNodes['applet'] = document.getElementsByTagName('applet').length;
		for(var tagName in totalNodes)
		{
			var counter = totalNodes[tagName] - 1;
			for(var node; node = document.getElementsByTagName(tagName)[counter]; counter--)
			{
				sourceCode = jActivating.getSourceCode(node);
				if(sourceCode)
				{
					node.outerHTML = sourceCode;
				}
			}
		}
		jActivating.isMSIE = null;
	},
	
	
	/**
	 * Retrieves the object and its content in HTML.
	 * @param object - The analyzed node
	 * @return string - The HTML received, NULL for malformed syntax (end tag)
	 */
	getSourceCode : function(node)
	{
		var sourceCode = node.outerHTML;
		switch(node.nodeName.toLowerCase())
		{
			case 'embed':
				return sourceCode;
			break;
			case 'object':
			case 'applet':
				var openTag = sourceCode.substr(0, sourceCode.indexOf('>') + 1);
				var closeTag = sourceCode.substr(sourceCode.length - 9).toLowerCase();
				if(closeTag != '</object>' && closeTag != '</applet>')
				{
					// Filters malformed syntax for avoid unexpected results in Internet Explorer
					return null;
				}
				if(jActivating.isMSIE)
				{
					var innerCode = jActivating.getInnerCode(node);
					sourceCode = openTag + innerCode + closeTag;
				}
				return sourceCode;
			break;
		}
	},
	
	
	/**
	 * Retrieves the HTML between start and end tags of the object.
	 * @param object - The analyzed node
	 * @return string - The HTML received
	 *
	 * NOTE: the innerHTML property isn't used because Internet Explorer
	 * sometimes returns a empty result.
	 */
	getInnerCode : function(node)
	{
		var innerCode = '';
		var totalChilds = node.childNodes.length - 1;
		for(var counter = totalChilds, child; child = node.childNodes[counter]; counter--)
		{
			innerCode += child.outerHTML;
		}
		return innerCode;
	}
	
}


// Execute script only for Internet Explorer and Opera (9+)
if(document.attachEvent)
{
	if(window.opera)
	{
		document.attachEvent("DOMContentLoaded", jActivating.reinsertContent);
	}
	else
	{
		jActivating.reinsertContent();
	}
}