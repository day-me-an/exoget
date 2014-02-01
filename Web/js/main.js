﻿/*
    $Id: main.js,v 1.3 2007/08/18 02:59:49 damian Exp $
*/

var mediaRowPropsLastChildBorderRightColor = "transparent";

var isMediaDetails = false;
var hasMediaCommentFormVisible = false;
var currentMediaId;
var currentMediaSkey;

function documentReady_global()
{
    var qElem = document.getElementById("s_query");

    /*
        Focus main query input field
    */
    qElem.focus();

    /*
        Prevent empty query submit and submit the URL rewritten search
    */
    function submitSearch(query)
    {
        var searchUrl = "/";
        
        var isScopeVideo = $("#searchFormScopes input[@checked]").attr("id") == "ss_video";
        
        if(isScopeVideo)
            searchUrl += "video/";
            
        else
            searchUrl += "audio/";
        
        var singlePropSearchMatch = null;
        var encodedQuery;
        
	    if (query.match(/[+:"]|((^\-)|(\s\-[^\s]))/) == null)
	    {
		    encodedQuery = Tokenize(query);
		    encodedQuery = encodedQuery.replace(/\s/g, "-")
	    }	
	    else
	    {    
            // trim
            query = $.trim(query);
            
	        // replace ' and ` and ”
	        query = query.replace(/'/g, "");
	        query = query.replace(/`/g, "");
	        query = query.replace(/”/g, '"');
        	
	        // remove fake -s
	        query = query.replace(/- /g, " ");
        	
	        // if "all uppercase", make lowercase
	        if(query.search(/^([a-z\s\-]+)$/i) != -1
		        && query.search(/([^a-z]+)/) != -1)
	        {
		        query = query.toLowerCase();
	        }
        	
            encodedQuery = encodeURIComponent(query);
            encodedQuery = encodedQuery.replace(/%3A/g, ":");
            encodedQuery = encodedQuery.replace(/\-/g, "--");
            encodedQuery = encodedQuery.replace(/%20/g, "-");
            
            var regexSinglePropSearch = /^([a-z]+)\:([\-]+)?"?([^\:]+)"?$/i;
            singlePropSearchMatch = regexSinglePropSearch.exec(encodedQuery);
        }
        
        if(singlePropSearchMatch != null)
        {
            searchUrl += "field/";
            searchUrl += singlePropSearchMatch[1].toLowerCase();
            searchUrl += "/";
    		
		    var value = singlePropSearchMatch[3];
		    value = $.trim(value);
    		
		    // if the user used quotes for no reason, remove
		    if(value.indexOf("%22") == 0
			    && value.indexOf("%22", 3) == (value.length - 3)
			    && value.search(/([a-z]+)/i) != -1)
		    {
			    value = value.replace(/%22/g, "");
		    }
    		
            searchUrl += value;
        }
        else
        {
            searchUrl += "search/";
            searchUrl += encodedQuery;
        }
        
        searchUrl += "/";
        
        var searchOptions = getCurrentSearchOptions();
        
        if(!isScopeVideo && searchOptions != 0)
        {
            searchUrl += "?options=";
            searchUrl += searchOptions;
        }
        
        window.location.href = searchUrl;
    }

    $("#searchForm").submit(function()
    {
        if(qElem.value.length == 0)
            return false;
            
        submitSearch(qElem.value);
        
        return false;
    });

    /*
        Search scope click event, current scope styling, auto submission
    */
    $("#searchFormScopes label").click(function()
    {
        var forAttrValue = this.htmlFor;

        $(this).removeClass("hover");
        $("#searchFormScopes label").removeClass("current");
        $(this).addClass("current");

        if(forAttrValue == "ss_audio")
            $("#searchForm .searchFormOptionsOpener").show();
            
        else
            $("#searchForm .searchFormOptionsOpener").hide();

        if(qElem.value != "" && (qElem.value != qElem.defaultValue || forAttrValue != $("#searchFormScopes input[@checked]").attr("id")))
        {
           document.getElementById(forAttrValue).checked = true;
           submitSearch(qElem.value);
        }
        else
        {
            document.getElementById(forAttrValue).checked = true;
            qElem.focus();
        }

        return false;
    });

    /*
        Set visibility:hidden on object and embed elements
    */
    function toggleObjectVisibility()
    {
        $("object,embed").toggleClass("hidden");
    }

    /*
        Show search options dialog
    */
    $(".searchFormOptionsOpener").click(function()
    {        
        var dialog = document.getElementById("searchFormOptionsContainer");
            
        if(dialog == null)
        {
            var dialogUrl = "/SearchOptionsDialog.aspx";
            var currentSearchOptions = getCurrentSearchOptions();
            
            if(currentSearchOptions != 0)
                dialogUrl += "?options=" + currentSearchOptions;    
            
            $.get
            (
                dialogUrl,
                function(containerNode)
                {				    
				    $(containerNode).hide().appendTo("#searchForm > fieldset").slideToggle
				    (
				        120,
				        function()
				        {
				            qElem.focus();
				        }
				    );
				    
				    $("#searchFormOptionsContainer .close").click(searchOptionsDialogCloseHandler);
				    $("#searchFormOptionsContainer .saveOptions").click(searchFormOptionsSaveHandler);				    
                }
            );
        }
        else
        {
		    $(dialog).slideToggle
		    (
		        120,
                function()
                {
                    qElem.focus();
                }
		    );
		}
        
        return false;
    });

    /*
        Close advanced search panel
    */
    function searchOptionsDialogCloseHandler()
    {
	    var dialog = document.getElementById("searchFormOptionsContainer");

        $(dialog).hide();
        toggleObjectVisibility();
        qElem.focus();
        
        return false;
    }
    //$("#searchFormOptions .close").click(searchOptionsDialogCloseHandler);

    /*
        Save default search options to cookie
    */
    function searchFormOptionsSaveHandler()
    {
        this.disabled = true;

        $.cookie
        (
            "searchOptions",
            getCurrentSearchOptions(),
            {
                path: "/",
                expires: 1825
            }        
        );

        $("#searchFormOptionsContainer").hide();
        toggleObjectVisibility();
        this.disabled = false;    
        qElem.focus();

        return false;
    }
    //$("#searchFormOptions .saveOptions").click(searchFormOptionsSaveHandler);

    /*
        Get the current search options, if non, returns null
    */
    function getCurrentSearchOptions()
    {
        var currentSearchOptions = 0;
        
        if(document.getElementById("searchFormOptions") != null)
        {
            $("#searchFormOptions input[@type='checkbox']").each(function()
            {
                if(this.checked)
                    currentSearchOptions = currentSearchOptions | parseInt(this.name.substring(3)); // all have 3 char prefix of "so_"
            });
            
            var searchOptionDuration = document.getElementById("so_duration");
            var searchOptionDurationValue = parseInt(searchOptionDuration.options[searchOptionDuration.selectedIndex].value);
            currentSearchOptions = currentSearchOptions | searchOptionDurationValue;    
        }
        else if($.query.get("options") != null)
            currentSearchOptions = parseInt($.query.get("options"));
                
        else if($.cookie("searchOptions") != null)
            currentSearchOptions = parseInt($.cookie("searchOptions"));
        
        if(currentSearchOptions == 1535)
            currentSearchOptions = 0;
        
        return currentSearchOptions;
    }

    /*
        Remove separator from last property in MediaRow, mozilla & safari support CSS's last-child
    */
    if($.browser != $.browser.mozilla && $.browser != $.browser.safari)
    {
        $(".mediaRow .props li:last-child").css("border-right-color", mediaRowPropsLastChildBorderRightColor);
    }
}

function documentReady_mediaDetails()
{
    /*
        Media Comment Form
            Show For New Comment
    */
    $("#toggleMediaCommentForm").click(function()
    {
        if(hasMediaCommentFormVisible)
            return false;
    
        $(this.parentNode).hide();
        $("#mediaCommentForm").show().find(":input[@name=title]").each(function(){this.focus();});
        hasMediaCommentFormVisible = true;

        return false;
    });

    /*
        Media Comment Form
            Prevent Normal Submit
    */
    $(".mediaCommentForm").submit(function()
    {
        return false;
    });

    /*
        Media Comment Form
            Submit Comment
    */
    function mediaCommentFormHandler()
    {
        var button = this;
        button.disabled = true;

        var form = button.parentNode.parentNode.parentNode;
        
        var title = $(":input[@name=title]", form).val();
        var body = $(":input[@name=body]", form).val();
        var parentId = $(":input[@name=parentId]", form).val() || 0;
        
        $.post
        (
            "/User.asmx/AddMediaComment",
            {
                mediaId: currentMediaId,
                mediaSkey: currentMediaSkey,
                title: title,
                body: body,
                parentId: parentId
            },
            function(r)
            {
                switch($("Status", r).text())
                {
                    case "Success":
                        $(form).hide();
                        form.reset();
                        hasMediaCommentFormVisible = false;
                    break;
                
                    default:
                        alert($("Message", r).text());
                        button.disabled = false;
                    break;
                }
            }
        );

        return false;
    }
    $(".mediaCommentForm button").click(mediaCommentFormHandler);

    /*
        Media Comment Form
            Show new form for reply
    */
    $(".comments .comment .actions a").click(function()
    {
        if(hasMediaCommentFormVisible)
            return false;
    
        var replyMediaCommentForm = document.getElementById("mediaCommentForm").cloneNode(true);
        replyMediaCommentForm.id = null;
        $("button", replyMediaCommentForm).click(mediaCommentFormHandler);    
        
        var parentIdElem = document.createElement("input");
        parentIdElem.name = "parentId";
        parentIdElem.type = "hidden";
        parentIdElem.value = this.parentNode.parentNode.id.split("_")[1];
        
        // add parent comment id field
        replyMediaCommentForm.appendChild(parentIdElem);
        
        $(replyMediaCommentForm).insertAfter(this.parentNode).show().find(":input[@name=title]").each(function(){this.focus();});
        hasMediaCommentFormVisible = true;
        
        return false;
    });

    /*
        Media Comment
            Rating
    */
    $(".comments .comment .actions .rating li strong").click(function()
    {
        var rateGroupNode = this.parentNode.parentNode;

        $(rateGroupNode).css("background-color", "#F4FFF4");

        var commentId = this.parentNode.parentNode.parentNode.parentNode.id.split("_")[1];
        var rating = $(this).attr("class") == "approve" ? "Approve" : "Disapprove";
        var _this = this;
        
        $.post
        (
            "/User.asmx/AddMediaCommentRating",
            {
                mediaId: currentMediaId,
                commentId: commentId,
                rating: rating
            },
            function(r)
            {
                switch($("Status", r).text())
                {
                    case "Success":
                        $(_this).unbind();
                        elem.innerText = (parseInt($(elem).text()) + 1);
                    break;
                
                    default:
                        alert($("Message", r).text());
                    break;
                }
                
                $(rateGroupNode).css("background-color", "#FFF");
            }
        );
        
        return false;
    });

    /*
        Star Rating
            Submit Rating
    */
    $(".star-rating li a").click(function()
    {
        var rating = parseInt($(this).text());
        var _this = this;
        
        $.post
        (
            "/User.asmx/AddMediaRating",
            {
                mediaId: currentMediaId,
                mediaSkey: currentMediaSkey,
                rating: rating
            },
            function(r)
            {
                switch($("Status", r).text())
                {
                    case "Success":
                        $(".star-rating .current-rating").css("width", (rating * 20) + "%");
                        $(_this).unbind();
                    break;
                
                    default:
                        alert($("Message", r).text());
                    break;
                }
            }
        );        
        
        return false;
    });

    /*
        Add Media Favorite
            Button
    */
    $(".mediaDetails .addFavorite").click(function()
    {
        this.disabled = true;
        var _this = this;
        
        $.post
        (
            "/User.asmx/AddMediaFavorite",
            {
                mediaId: currentMediaId,
                mediaSkey: currentMediaSkey
            },
            function(r)
            {
                var status 
            
                switch($("Status", r).text())
                {
                    case "Success":
                        $(_this).text("Success");
                    break;
                    
                    case "Duplicate":
                        $(_this).text("Already Added");
                    break;
                
                    default:
                        alert($("Message", r).text());
                        _this.disabled = false;
                    return;
                }
                
                $(_this).unbind();
            }
        );        
    });

    /*
        Fix for "click to activate and use this control"
    */
    $("object").each(function()
    {
        var newObject;

        if(!$.browser.msie)
        {
            newObject = document.createElement("object");
            newObject.style.display = "block";

            for(var i = 0; i < this.attributes.length; i++)
            {
                if(this.attributes[i].nodeValue != "")
                    newObject.setAttribute(this.attributes[i].nodeName, this.attributes[i].nodeValue);
            }
            
            $("param", this).appendTo(newObject);
        }    
        else
        {
            newObject = document.createElement("embed");

            for(var i = 0; i < this.attributes.length; i++)
            {
                if(this.attributes[i].nodeValue != "")
                    newObject.setAttribute(this.attributes[i].nodeName != "data" ? this.attributes[i].nodeName : "src", this.attributes[i].nodeValue);
            }

            $("param", this).each(function()
            {
                newObject.setAttribute(this.getAttribute("name"), this.getAttribute("value"));
            });
        }
        
        newObject.style.display = "block";
        this.parentNode.replaceChild(newObject, this);   
    });
}

/*
    document.ready event entry handler
*/
$(document).ready(function()
{
    documentReady_global();

    if(isMediaDetails)
        documentReady_mediaDetails();
});

/*
    Hacks for IE6, use word "fix" so no one ever thinks its trying to maliciously hack there IE6
*/
function fixIE6()
{
    /*
        IE6 doesnt't support color "transparent"
    */
    mediaRowPropsLastChildBorderRightColor = "white";

    $(document).ready(function()
    {
        /*
            MediaRow hover effect hack for IE6
        */
        $(".mediaRow").hover
        (
            function()
            {
                $(this).addClass("resultHover");
                $(".title", this).addClass("aHover");
            },
            
            function()
            {
                $(this).removeClass("resultHover");
                $(".title", this).removeClass("aHover");
            }
        );

        /*
            MediaRow main title link hack for IE6
        */
        $(".mediaRow .title").hover
        (
            function()
            {
                $(this).addClass("aHover");
            },
            
            function()
            {
                $(this).removeClass("aHover");
            }
        );
        
        /*
            Search form scope hover effect hack for IE6
        */
        $("#searchFormScopes label").hover
        (
            function()
            {   
                if(!$(this).is(".current"))
                    $(this).addClass("hover");
            },
            
            function()
            {
                $(this).removeClass("hover");
            }
        );
    });
}

function IsUpper(c)
{
	return c.match(/[A-Z]/) != null;
}

function IsLower(c)
{
	return c.match(/[a-z]/) != null;
}

var letterRegex = null;
function IsLetter(c)
{
	if(letterRegex == null)
		letterRegex = new RegExp("[\u0041-\u005a\u0061-\u007a\u00aa\u00b5\u00ba\u00c0-\u00d6\u00d8-\u00f6\u00f8-\u02c1\u02c6-\u02d1\u02e0-\u02e4\u02ee\u037a-\u037d\u0386\u0388-\u038a\u038c\u038e-\u03a1\u03a3-\u03ce\u03d0-\u03f5\u03f7-\u0481\u048a-\u0513\u0531-\u0556\u0559\u0561-\u0587\u05d0-\u05ea\u05f0-\u05f2\u0621-\u063a\u0640-\u064a\u066e\u066f\u0671-\u06d3\u06d5\u06e5\u06e6\u06ee\u06ef\u06fa-\u06fc\u06ff\u0710\u0712-\u072f\u074d-\u076d\u0780-\u07a5\u07b1\u07ca-\u07ea\u07f4\u07f5\u07fa\u0904-\u0939\u093d\u0950\u0958-\u0961\u097b-\u097f\u0985-\u098c\u098f\u0990\u0993-\u09a8\u09aa-\u09b0\u09b2\u09b6-\u09b9\u09bd\u09ce\u09dc\u09dd\u09df-\u09e1\u09f0\u09f1\u0a05-\u0a0a\u0a0f\u0a10\u0a13-\u0a28\u0a2a-\u0a30\u0a32\u0a33\u0a35\u0a36\u0a38\u0a39\u0a59-\u0a5c\u0a5e\u0a72-\u0a74\u0a85-\u0a8d\u0a8f-\u0a91\u0a93-\u0aa8\u0aaa-\u0ab0\u0ab2\u0ab3\u0ab5-\u0ab9\u0abd\u0ad0\u0ae0\u0ae1\u0b05-\u0b0c\u0b0f\u0b10\u0b13-\u0b28\u0b2a-\u0b30\u0b32\u0b33\u0b35-\u0b39\u0b3d\u0b5c\u0b5d\u0b5f-\u0b61\u0b71\u0b83\u0b85-\u0b8a\u0b8e-\u0b90\u0b92-\u0b95\u0b99\u0b9a\u0b9c\u0b9e\u0b9f\u0ba3\u0ba4\u0ba8-\u0baa\u0bae-\u0bb9\u0c05-\u0c0c\u0c0e-\u0c10\u0c12-\u0c28\u0c2a-\u0c33\u0c35-\u0c39\u0c60\u0c61\u0c85-\u0c8c\u0c8e-\u0c90\u0c92-\u0ca8\u0caa-\u0cb3\u0cb5-\u0cb9\u0cbd\u0cde\u0ce0\u0ce1\u0d05-\u0d0c\u0d0e-\u0d10\u0d12-\u0d28\u0d2a-\u0d39\u0d60\u0d61\u0d85-\u0d96\u0d9a-\u0db1\u0db3-\u0dbb\u0dbd\u0dc0-\u0dc6\u0e01-\u0e30\u0e32\u0e33\u0e40-\u0e46\u0e81\u0e82\u0e84\u0e87\u0e88\u0e8a\u0e8d\u0e94-\u0e97\u0e99-\u0e9f\u0ea1-\u0ea3\u0ea5\u0ea7\u0eaa\u0eab\u0ead-\u0eb0\u0eb2\u0eb3\u0ebd\u0ec0-\u0ec4\u0ec6\u0edc\u0edd\u0f00\u0f40-\u0f47\u0f49-\u0f6a\u0f88-\u0f8b\u1000-\u1021\u1023-\u1027\u1029\u102a\u1050-\u1055\u10a0-\u10c5\u10d0-\u10fa\u10fc\u1100-\u1159\u115f-\u11a2\u11a8-\u11f9\u1200-\u1248\u124a-\u124d\u1250-\u1256\u1258\u125a-\u125d\u1260-\u1288\u128a-\u128d\u1290-\u12b0\u12b2-\u12b5\u12b8-\u12be\u12c0\u12c2-\u12c5\u12c8-\u12d6\u12d8-\u1310\u1312-\u1315\u1318-\u135a\u1380-\u138f\u13a0-\u13f4\u1401-\u166c\u166f-\u1676\u1681-\u169a\u16a0-\u16ea\u1700-\u170c\u170e-\u1711\u1720-\u1731\u1740-\u1751\u1760-\u176c\u176e-\u1770\u1780-\u17b3\u17d7\u17dc\u1820-\u1877\u1880-\u18a8\u1900-\u191c\u1950-\u196d\u1970-\u1974\u1980-\u19a9\u19c1-\u19c7\u1a00-\u1a16\u1b05-\u1b33\u1b45-\u1b4b\u1d00-\u1dbf\u1e00-\u1e9b\u1ea0-\u1ef9\u1f00-\u1f15\u1f18-\u1f1d\u1f20-\u1f45\u1f48-\u1f4d\u1f50-\u1f57\u1f59\u1f5b\u1f5d\u1f5f-\u1f7d\u1f80-\u1fb4\u1fb6-\u1fbc\u1fbe\u1fc2-\u1fc4\u1fc6-\u1fcc\u1fd0-\u1fd3\u1fd6-\u1fdb\u1fe0-\u1fec\u1ff2-\u1ff4\u1ff6-\u1ffc\u2071\u207f\u2090-\u2094\u2102\u2107\u210a-\u2113\u2115\u2119-\u211d\u2124\u2126\u2128\u212a-\u212d\u212f-\u2139\u213c-\u213f\u2145-\u2149\u214e\u2183\u2184\u2c00-\u2c2e\u2c30-\u2c5e\u2c60-\u2c6c\u2c74-\u2c77\u2c80-\u2ce4\u2d00-\u2d25\u2d30-\u2d65\u2d6f\u2d80-\u2d96\u2da0-\u2da6\u2da8-\u2dae\u2db0-\u2db6\u2db8-\u2dbe\u2dc0-\u2dc6\u2dc8-\u2dce\u2dd0-\u2dd6\u2dd8-\u2dde\u3005\u3006\u3031-\u3035\u303b\u303c\u3041-\u3096\u309d-\u309f\u30a1-\u30fa\u30fc-\u30ff\u3105-\u312c\u3131-\u318e\u31a0-\u31b7\u31f0-\u31ff\u3400-\u4db5\u4e00-\u9fbb\ua000-\ua48c\ua717-\ua71a\ua800\ua801\ua803-\ua805\ua807-\ua80a\ua80c-\ua822\ua840-\ua873\uac00-\ud7a3\uf900-\ufa2d\ufa30-\ufa6a\ufa70-\ufad9\ufb00-\ufb06\ufb13-\ufb17\ufb1d\ufb1f-\ufb28\ufb2a-\ufb36\ufb38-\ufb3c\ufb3e\ufb40\ufb41\ufb43\ufb44\ufb46-\ufbb1\ufbd3-\ufd3d\ufd50-\ufd8f\ufd92-\ufdc7\ufdf0-\ufdfb\ufe70-\ufe74\ufe76-\ufefc\uff21-\uff3a\uff41-\uff5a\uff66-\uffbe\uffc2-\uffc7\uffca-\uffcf\uffd2-\uffd7\uffda-\uffdc]");

	return letterRegex.exec(c) != null;
}

function IsDigit(c)
{
	return c.match(/[0-9]/) != null;
}

function IsLetterOrDigit(c)
{
	return IsLetter(c) || IsDigit(c);
}

function Tokenize(str)
{
	// options
	var wavy = false;
	var makeLowercase = true;
	var letterCasing = true;

	var sb = "";
	var prevCharAppendedAsSpace = false;
	
	for(var i = 0; i < str.length; i++)
	{
		var c = str.charAt(i);

		if (letterCasing
			&& IsUpper(c)
			&& (i - 2) > 0
			&& IsLower(str.charAt(i - 1))
			&& (wavy ? IsLower(str.charAt(i - 2)) : true)
			&& (i + 2) < str.length && IsLower(str.charAt(i + 1))
			&& (wavy ? IsLower(str.charAt(i + 2)) : true))
		{
			sb += ' ';
			sb += (makeLowercase ? c.toLowerCase() : c);
			prevCharAppendedAsSpace = false;
		}
		else if (IsDigit(c)
			&& (i - 3) > 0
			&& (i + 1) < str.length
			&& IsLetter(str.charAt(i - 1)) && ((IsLetter(str.charAt(i - 2)) && IsLetter(str.charAt(i - 3))) || IsDigit(str.charAt(i + 1)))
			&& (sb.length > 0 && sb.charAt(sb.length - 1) != ' '))
		{
			sb += ' ';
			sb += c;
			prevCharAppendedAsSpace = false;
		}
		else if (IsLetter(c)
			&& (i + 3) < str.length
			&& IsLetter(str.charAt(i + 1)) && IsLetter(str.charAt(i + 2)) && IsLetter(str.charAt(i + 3))
			&& (i - 1) > 0
			&& IsDigit(str.charAt(i - 1))
			&& (sb.length > 0 && sb.charAt(sb.length - 1) != ' '))
		{
			sb += ' ';
			sb += (makeLowercase ? c.toLowerCase() : c);
			prevCharAppendedAsSpace = false;
		}
		else if (c == '\''
			|| c == '`'
			|| (c == '.' && (((i - 1) > 0
			&& (IsUpper(str.charAt(i - 1)) || IsDigit(str.charAt(i - 1))))
			|| ((i + 1) < str.length && (IsUpper(str.charAt(i + 1)) || IsDigit(str.charAt(i + 1)))))))
		{
			prevCharAppendedAsSpace = false;
			continue;
		}
		else if (IsLetterOrDigit(c))
		{
			sb += (makeLowercase ? c.toLowerCase() : c);
			prevCharAppendedAsSpace = false;
		}
		else if (i != (str.length - 1)
			&& i != 0
			&& !prevCharAppendedAsSpace)
		{
			sb += ' ';
			prevCharAppendedAsSpace = true;
		}
	}
	
	return sb;
}

/*
var MAX_DUMP_DEPTH = 10;
function dumpObj(obj, name, indent, depth)
{
    if (depth > MAX_DUMP_DEPTH)
        return indent + name + ": <Maximum Depth Reached>\n";

    if (typeof obj == "object")
    {
        var child = null;
        var output = indent + name + "\n";
        indent += "\t";

        for (var item in obj)
        {
            try
            {
                child = obj[item];
            }
            catch (e)
            {
                child = "<Unable to Evaluate>";
            }

            if (typeof child == "object")
                output += dumpObj(child, item, indent, depth + 1);

            else
                output += indent + item + ": " + child + "\n";
        }

        return output;
    }
    else
        return obj;
}
*/