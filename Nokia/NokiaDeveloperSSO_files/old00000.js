if(window.navigator.userAgent.match(/Series60|SymbianOS/) != null) {
	window.onload = function() {
		// in symbian no childElementCount/childNodes
		var nav = document.getElementById('fnnav'), i = nav.children.length;
		while(i--) {
			var navi = nav.children[i];
			// in symbian children means grandchildren as well
			if(navi.parentNode != nav) continue;
			navi.onclick = function() {
				var subnav = this.parentNode.getElementsByTagName('ul');
				var j = subnav.length;
				while(j--) subnav[j].style.display = 'none';
				subnav = this.getElementsByTagName('ul');
				subnav[0].style.display = 'block';
			}
		}
	}
}

/*
var Request = {
  parameter: function(name) {
    return this.parameters()[name];
  },

  parameters: function() {
    var result = {};
    var url = window.location.href;
    var parameters = url.slice(url.indexOf('?') + 1).split("&");

    for(var i=0; i < parameters.length; i++) {
      var parameter = parameters[i].split('=');
      result[parameter[0]] = parameter[1];
    }
    return result;
  }
};
*/

/* Cookies/Multivalue */
/*
function createCookie(name,value,days) {
	if (days) {
		var date = new Date();
		date.setTime(date.getTime()+(days*24*60*60*1000));
		var expires = "; expires="+date.toGMTString();
	}
	else var expires = "";
	document.cookie = name+"="+value+expires+"; path=/";
}

function readCookie(name) {
	var nameEQ = name + "=";
	var ca = document.cookie.split(';');
	for(var i=0;i < ca.length;i++) {
		var c = ca[i];
		while (c.charAt(0)==' ') c = c.substring(1,c.length);
		if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length,c.length);
	}
	return null;
}

function MultiValueCookie(name, idSeparator, valueSeparator) {
	var value = readCookie(name);
	this.name = name;
	if (valueSeparator != null) {
		this.valueSeparator=valueSeparator;
	} else {
		this.valueSeparator=" ";
	}
	if (idSeparator != null) {
		this.idSeparator = idSeparator;
	} else {
		this.idSeparator = ",";
	}
	this.expires = 1;
	this.values = new Array();
	if (value != null) {
		var items = value.split(this.idSeparator);
		for (var i=0;i<items.length;i++) {
			var parts = items[i].split("|");
			if (parts.length == 2) {
				this.values[this.values.length] = { key: parts[0], value: parts[1] };	
			}
		}
	}
}

MultiValueCookie.prototype = {
	findValueIndex: function(keyStr) {
		for (i=0; i<this.values.length;i++) {
			if (this.values[i].key == keyStr) {
				return i;
			}
		}
		return -1;
	},
	
	getIds: function() {
		var result = new Array();
		for (i = 0; i < this.values.length; i++) {
		  result.push(this.values[i].key);
		}
		return result;
	},
	
	setValue: function(keyStr, str) {
		var i = this.findValueIndex(keyStr);
		var newStr;
		if (i == -1) {
			i = this.values.length;
			newStr = str;
		} else {
			if (this.values[i].value == '') {
				newStr = str;
			} else {
				var re=new RegExp("("+str+" )|("+str+"$)");
				if (re.exec(this.values[i].value) != null) return;
				newStr = this.values[i].value + this.valueSeparator + str;
			}
		}
	
		this.values[i] = {
			key: keyStr,
			value: newStr
		};
	},
	
	removeValue: function(keyStr, str) {
		var i = this.findValueIndex(keyStr);
		if (i == -1) return;
		var re=new RegExp("("+str+" )|("+str+"$)");
		this.values[i] = {
			key: keyStr,
			value: this.values[i].value.replace(re, "")
		};
	},
	
	getValues: function(keyStr) {
		var i = this.findValueIndex(keyStr);
		if (i == -1) return null;
		return this.values[i].value.split(this.valueSeparator);
	},
	
	setExpirationDays: function(days) {
		this.expires = days;
	},
	
	clear: function() {
		this.values = new Array();	
	},
	
	save: function(days) {
		var result = '';
		for (i=0;i<this.values.length;i++) {
			result = result + (this.values[i].key + "|" + this.values[i].value);
			if (i+1 < this.values.length) {
				result += this.idSeparator;
			}
		}
		createCookie(this.name, result, this.expires);
	}
};
*/
/* End of MultivalueCookie */

/* Calendar */
/*
function resetCalEventBg(el){
    el.find('.fnCalEventBg').animate( { width: "4px" }, 400);
	el.each(function() {
		if($(this).hasClass('fnCalWebinar')){	
			$(this).children('.fnCalendarEvent').children().animate( { color: "#0066cc" }, 300);
		}else{
			$(this).children('.fnCalendarEvent').children().animate( { color: "#ff7e00" }, 300);
		}  
	});
}
function containsChinese(str) {
        var re1 = new RegExp(".*[\u4E00-\uFA29]+.*");
        var re2 = new RegExp("^[\uE7C7-\uE7F3]*$");
        return re1.test(str) && !re2.test(str);
}

function cufonReplaceWrapper(selector, styles) {
        $(selector).each(function() {
                var content=$(this).html();
                if (!containsChinese(content)) {
                        Cufon.replace($(this).get(), styles);
                }
        });
}

function initSpotlightSnippetImageGallery() {
	var thumbs = $("#fancybox-wrap .spotlightThumbs img");
	if (thumbs.size() == 0) return;
	thumbs.click(function() {
		$("#fancybox-wrap .spotlightImages img").removeClass("active");
		$(this).addClass("active");
		var clone = $(this).clone();
		clone.removeAttr('width').removeAttr('height');
		$("#fancybox-wrap .spotlightImages img:first").remove();
		$("#fancybox-wrap .spotlightImages").prepend(clone);
	});
}

function initSpotlightImageGallery() {
	$(".spotlightImages").each(function() {		
		$(this).children().children().first().addClass("active").clone().removeAttr("width").removeAttr("height").prependTo($(this));
		
		if( $(this).children().children().size() > 1) {
		
			// onclick remove previous image, clone new one, add active class
			$(".spotlightThumbs img").click(function() {
				$(this).parent().siblings("img").fadeOut().remove();
				$(this).clone().removeAttr("width").removeAttr("height").fadeIn().prependTo($(this).parent().parent());
					$(".spotlightThumbs img").each(function() {
						if ( $(this).hasClass("active") ) {
							$(this).removeClass("active");
						}
					});
				$(this).addClass("active");
			});		
		} else {
			$(this).children().children().hide();
		}
	});
}

function displayLocalTwitterFeed(ele, filename, cb) {
	displayTwitterFeed(ele, '/gen/twitter/'+filename, cb);
}

function displayTwitterFeed(ele, url, cb) {
	jQuery.ajax({
		dataType : "json",
		url : url,
		success : function(data, statusText) {
			function getTimeHtml(tweet) {
				return '<a class="timestamp" href="http://twitter.com/#!/'+tweet.user.screen_name+'/status/'+
					tweet.id_str+'">'+getAbstractTimeString(tweet.created_at)+'</a>';
			}
			var html = "";
			for(var i = 0; i < Math.min(data.length, 3); ++i) {
				html += '<div class="tweet">'+
					'<div class="text">'+htmlifyTweet(data[i])+
					' [<a href="http://twitter.com/#!/'+data[i].user.screen_name+'">'+
					data[i].user.screen_name+'</a>]<br/>'+
					getTimeHtml(data[i])+
					'</div></div>';
			}
			$(ele).html(html);
			if(typeof cb == "function") cb();
		},
		error: function (xhr, ajaxOptions, thrownError) {
			// if feed failed to load for some reason the callback can schedule a retry
			if(typeof cb == "function") cb(xhr.status||0);
		}
	});
}

function htmlifyTweet(tweet) {
	// when multiple hashes in one word, use text up to the second hash
	var ret = tweet.text.replace(/\B(#(.+?))\b/gi, '<a href="http://twitter.com/#!/search?q=%23$2">$1</a>');
	
	// only consider @-chars with non-alphanumeric preceeding chars to be replies (leave emails be)
	ret = ret.replace(/\B(@(.+?))\b/gi, '<a href="http://twitter.com/#!/$2">$1</a>');
	
	// if json was fetched with &include_entities=true
	if(typeof tweet.entities != 'undefined') {
		$.each(tweet.entities.urls, function(i, u) {
			var link;
			if (u.display_url != null) {
				link = '<a href="'+ u.expanded_url +'">'+ u.display_url +'</a>';
			} else {
				link = '<a href="'+ u.url +'">'+ u.url +'</a>';
			}
			ret = ret.replace(u.url, link);
		});
	} else {
		// if twitter ever stops shortening urls and allows hashes in URLs, this has to change
		ret = ret.replace(/((f|ht)tps?:\/\/.+?)\b/gi, '<a href="$1">$1</a>');
	}
	
	return ret;
}

function getAbstractTimeString(time) {
	if(typeof time == "string") {
		// twitter timestamps are not compatible with IE
		if($.browser.msie) time = time.replace(/(\+\d{4}) (\d{4})$/, "$2 $1");
		time = Date.parse(time);
	}
	
	var now = new Date().getTime();
	var diff = now - time;		
	var mins = Math.round(diff/1000/60);
    // in case user's clock is off
    if(diff < 0) return "0min ago";

	if(mins < 60) {
		return mins + "min ago";
	}
	var hours = Math.round(diff/1000/60/60);
	if(hours < 24) {
		return hours + "h ago";
	}
	var days = Math.round(diff/1000/60/60/24);
	if ( days < 7 ) {
		return days + "d ago";
	}

	var week = Math.round(diff/1000/60/60/24/7);
	if ( week < 4 ) {
		return week + "w ago";
	}
	return  Math.round(diff/1000/60/60/24/30) + " month(s) ago";
}
*/

$(document).ready(function() {

    /*
	var value = $('meta[name=PATH\\.1\\.LABEL]').attr("content");
	if(value){
		if(value.indexOf("Design")!=-1){
		    $("#navFirst_underLine").css("display","block");
		    $("#navFirst_link").css("color","white");		
		}else if(value.indexOf("Develop")!=-1){
			$("#navSecond_underLine").css("display","block");	
			$("#navSecond_link").css("color","white");
		}else if(value.indexOf("Distribute")!=-1){
			$("#navThird_underLine").css("display","block");
			$("#navThird_link").css("color","white");	
		}else if(value.indexOf("Devices")!=-1){
			$("#navFourth_underLine").css("display","block");
			$("#navFourth_link").css("color","white");	
		}else if(value.indexOf("Resources")!=-1){
			$("#navFifth_underLine").css("display","block");
			$("#navFifth_link").css("color","white");	
		}else if(value.indexOf("Community")!=-1){
			$("#navSixth_underLine").css("display","block");
			$("#navSixth_link").css("color","white");	
		}	
	}
	*/

    /*
    $("table.bordered tr:nth-child(even)").addClass('rowEven');
    */

    /*
	// select a search scope based on location
	var allMetas = document.getElementsByTagName("meta");
	LOOP1: for(var n=3; n>0; n-=1) {
		for(var i = 0; i < allMetas.length; i++) {
			if(allMetas[i].getAttribute("name") == 'PATH\\.'+n+'\\.URL') {
				var ss = allMetas[i].getAttribute("content");
				if(!ss.length) continue;
				ss = ss.replace(/^http(s)?:../, ".").replace(/\//g, '.').replace('.qa.', '.').replace(/\.$/, '');
				var ssEl = document.getElementById('fnHeaderSearchMenu');
				var el = document.getElementById('searchscope'+ss);
				if(ssEl != null && el != null) {
					ssEl.selectedIndex = el.index;
					if(ssEl.selectedIndex < 0)
						ssEl.selectedIndex = 0;
					break LOOP1;
				}
			}
		}
	}
	*/

    /*
	// if spotlight has images, use a wider container	
	$(".spotlightContent").each(function() {
        if ( $(this).find(".spotlightImages").size() > 0 ) {
			$(this).width(750);
        }	
	});
	*/

	// encode/decode urls
	$.extend({URLEncode:function(c){var o='';var x=0;c=c.toString();var r=/(^[a-zA-Z0-9_.]*)/;
	  while(x<c.length){var m=r.exec(c.substr(x));
		if(m!=null && m.length>1 && m[1]!=''){o+=m[1];x+=m[1].length;
		}else{if(c[x]==' ')o+='+';else{var d=c.charCodeAt(x);var h=d.toString(16);
		o+='%'+(h.length<2?'0':'')+h.toUpperCase();}x++;}}return o;},
	URLDecode:function(s){var o=s;var binVal,t;var r=/(%[^%]{2})/;
	  while((m=r.exec(o))!=null && m.length>1 && m[1]!=''){b=parseInt(m[1].substr(1),16);
	  t=String.fromCharCode(b);o=o.replace(m[1],t);}return o;}
	});

    /*
	// bookmark social sites
	if(navigator.userAgent.match(/iPhone|iPod|Maemo|Tablet browser|SymbianOS/) != null) {
		$("#fntitleToggle").click(function(){
			$("#fnbookMarks").toggle();
		});
	} else {
		$("#fntitleToggle").hover(
			function(){	
				$("#fnbookMarks").show();
			},
			function(){
				$("#fnbookMarks").hide();
			}
		);
	}

	var ehref = $.URLEncode(location.href);
	var etitle = $.URLEncode(document.title);

	$("#fnbookmarkDelicious").click(function(){ 		
		window.open('http://delicious.com/save?v=5&noui=&jump=close&url='+ehref+'&title='+etitle+'','delicious','toolbar=no,width=550,height=550,resizable=yes'); 
	});

	$("#fnbookmarkDigg").click(function(){ 		
		window.open('http://digg.com/submit?url='+ehref+'&title='+etitle+'&bodytext=&media=news&topic=tech_news','digg','toolbar=no,width=1070,height=750,scrollbars=yes,resizable=yes');
	});	
	
	$("#fnbookmarkGoogle").click(function(){ 		
		window.open('http://www.google.com/bookmarks/mark?op=add&hl=en&bkmk='+ehref+'&title='+etitle+'','google','toolbar=no,width=650,height=600,resizable=yes');
	});
	
	$("#fnbookmarkYahoo").click(function(){ 		
		window.open('http://myweb2.search.yahoo.com/myresults/bookmarklet?u='+ehref+'&t='+etitle+'','yahoo','toolbar=no,width=850,height=650,scrollbars=yes,resizable=yes');
	});	

	$("#fnbookmarkStumble").click(function(){ 		
		window.open('http://www.stumbleupon.com/submit?url='+ehref+'&title='+etitle+'','stumbleupon','toolbar=no,width=1070,height=550,scrollbars=yes,resizable=yes');
	});		

	$("#fnbookmarkReddit").click(function(){ 		
		window.open('http://www.reddit.com/submit?url='+ehref+'&title='+etitle+'','reddit','toolbar=no,width=650,height=650,resizable=yes');
	});			

	$("#fnbookmarkDiigo").click(function(){ 		
		window.open('http://www.diigo.com/import_all/transfer_furl?u='+ehref+'&t='+etitle+'','diigo','toolbar=no,width=1100,height=900,scrollbars=yes,resizable=yes');
	});				
	
	$("#fnbookmarkFurl").click(function(){ 		
		window.open('http://www.furl.net/storeIt.jsp?u='+ehref+'&t='+etitle+'','furl','toolbar=no,width=600,height=750,scrollbars=yes,resizable=yes');
	});				
	    
	$("#fnbookmarkTechnorati").click(function(){ 		
		window.open('http://technorati.com/faves?add='+ehref+'','technorati','toolbar=no,width=1000,height=750,scrollbars=yes,resizable=yes');
	});			
	
	$("#fnbookmarkNewsvine").click(function(){ 		
		window.open('http://www.newsvine.com/_tools/seed&save?u='+ehref+'&h='+etitle+'','newsvine','toolbar=no,width=970,height=750,resizable=yes');
	});	
	
	$("#fnbookmarkMagnolia").click(function(){ 		
		window.open('http://ma.gnolia.com/bookmarklet/add?url='+ehref+'&title='+etitle+'&description=','magnolia','toolbar=no,width=800,height=600,scrollbars=yes,resizable=yes');
	});	
	
	$("#fnbookmarkSquidoo").click(function(){ 		
		window.open('http://www.squidoo.com/lensmaster/bookmark?'+ehref+'','squidoo','toolbar=no,width=750,height=600,scrollbars=yes,resizable=yes');
	});	
	
	$("#fnbookmarkTwitter").click(function(){ 		
		window.open('http://twitter.com/home?status=Add+This:+'+ehref+'','twitter','toolbar=no,width=790,height=550,resizable=yes');
	});	
	
	$("#fnbookmarkFacebook").click(function(){ 		
		window.open('http://www.facebook.com/sharer.php?u='+ehref+'&t='+etitle+'','facebook','toolbar=no,width=550,height=550,resizable=yes');
	});	
	
	$("#fnbookmarkBrowser").click(function(){ 		
		bookmarksite(document.title, location.href)
	});	

	function bookmarksite(title, url){
		if (window.sidebar) // firefox
			window.sidebar.addPanel(title, url, "");
		else if (window.opera && window.print){ // opera
			var elem = document.createElement('a');
			elem.setAttribute('href',url);
			elem.setAttribute('title',title);
			elem.setAttribute('rel','sidebar');
			elem.click();
		}
		else if(document.all)// ie
			window.external.AddFavorite(url, title);
	}
	*/

    /*
	$("a.devCompareLink").click(function(){
		var width  = 900;
		var height = 600;
		var left   = (screen.width  - width)/2;
		var top    = (screen.height - height)/2;
		var params = 'width='+width+', height='+height;
		params += ', top='+top+', left='+left;
		params += ', directories=no';
		params += ', location=no';
		params += ', menubar=no';
		params += ', resizable=yes';
		params += ', scrollbars=yes';
		params += ', status=no';
		params += ', toolbar=no';
		var comparisonWindow =  window.open(this.href, 'showcomparison', params);
		if (window.focus) {
		    comparisonWindow.focus()
		}
		return false;
	});
	*/

	// toggle .subNav on navigation, add selected class if under section
	// older Symbian that doesn't support jquery is supported also separately
	if(navigator.userAgent.match(/iPhone|iPod|Maemo|Tablet browser|SymbianOS/) != null) {
		$("#fnnav > li > a").click(function(){
			
			$("#fnnav > li.selected").removeClass("selected").children('ul').hide();
			$(this).parent().addClass("selected").children('ul').show();
			return false;
		});
	} else {
		$("#fnnav > li").hover(
			function(){	
				$(this).addClass("selected").children('ul').show();
			},
			function(){
				$(this).removeClass("selected").children('ul').hide();
			}
		);
	}

	$("#fnnav > li").each(function(){
		if (this.id == ("fn"+$('meta[name=PATH\\.1\\.LABEL]').attr("content")) ) {
			$(this).addClass("active");	
		} 		
	});

        // url tweaks
        var siteDomain = window.location.host;
        var siteProtocol = window.location.protocol;

    /*
	// url tweaks happening when on stage (change live links into stage links)
        if (siteDomain == 'stage.qa.developer.nokia.com' || siteDomain == 'stage.developer.nokia.com' || siteDomain == 'stage.test.forumnokia.dmz') {

            jQuery("a").each(function(){
                if(jQuery(this).attr('href')){
                    var linkHref = jQuery(this).attr('href');
                    var startPos = linkHref.indexOf("//") + 2;
                    var suffix = siteDomain.substr(siteDomain.length - 4, 4);
                    var endPos = linkHref.indexOf(suffix) + suffix.length - startPos;
                    var replaceString = linkHref.substr(startPos, endPos);
                    if(replaceString == 'www.test.forumnokia.dmz'){
                        var newDomain = "stage.test.forumnokia.dmz";
                        jQuery(this).attr('href', (jQuery(this).attr('href').replace(replaceString, newDomain)));
                        var replaceProtocol = linkHref.substr(0, startPos - 2);
                        jQuery(this).attr('href', (jQuery(this).attr('href').replace(replaceProtocol, siteProtocol)));
                    }
                    else if(replaceString == 'www.qa.developer.nokia.com'){
                        var newDomain = "stage.qa.developer.nokia.com";
                        jQuery(this).attr('href', (jQuery(this).attr('href').replace(replaceString, newDomain)));
                        var replaceProtocol = linkHref.substr(0, startPos - 2);
                        jQuery(this).attr('href', (jQuery(this).attr('href').replace(replaceProtocol, siteProtocol)));
                    }
                    else if(replaceString == 'www.developer.nokia.com'){
                        var newDomain = "stage.developer.nokia.com";
                        jQuery(this).attr('href', (jQuery(this).attr('href').replace(replaceString, newDomain)));
                        var replaceProtocol = linkHref.substr(0, startPos - 2);
                        jQuery(this).attr('href', (jQuery(this).attr('href').replace(replaceProtocol, siteProtocol)));
                    }
                }
            });

        }
        */

    /*
	// url tweaks happening when on QA (change prod links to qa links)
        if(siteDomain == 'stage.qa.developer.nokia.com' || siteDomain == 'www.qa.developer.nokia.com' || siteDomain == 'qa.stage.forum.nokia.com' || siteDomain == 'www.qa.live.forum.nokia.com'){

            jQuery("a").each(function(){
                if(jQuery(this).attr('href')){
                    var linkHref = jQuery(this).attr('href');
                    var startPos = linkHref.search("//") + 2;
                    var endPos = linkHref.search(".com") + 4 - startPos;
                    var replaceString = linkHref.substr(startPos, endPos);
                    if(replaceString == 'forumnokia.secure.force.com'){
                        var newDomain = "forumnokia.post1.cs2.force.com";
                        jQuery(this).attr('href', (jQuery(this).attr('href').replace(replaceString, newDomain)));
                    }
                }
            });

        }
        */

    /*
	// hide all subnavs and " selected" if clicking anywhere in #content
	$('#fncontent, #bugcontent').click(function() {
		$(".subNav").hide();
		$("#fnnav li").removeClass("selected");
		$(".sectionListing").hide();
		$("#fnLanguagesNav").hide();		
	});
    */

    /*
	// select active element for tabbedNavigation and articleNavigation
	 jQuery("#tabbedNavigation, #articleNav").each(function(){
		 if(jQuery(this).find('li.selected').length == 0) {
			var matches = [];
			jQuery(this).find('li a').each(function(){
				var pageUrl = $.URLEncode(location.href);
				var itemUrl = $.URLEncode(jQuery(this).attr('href'));
				if((pageUrl.search(itemUrl)) > -1){
					matches.push({len:itemUrl.length, ele:jQuery(this).parents('li').eq(0)});
				}
			});
			matches = matches.sort(function(a,b) { return b.len - a.len; });
			if(matches.length > 0)
				matches[0].ele.addClass('selected');
		 }
	 });
	 */
	 
	// select active element for stepNavigation
    /*
	if ( $('meta[content^="Step "]').size() == 0 ) {
		$('.stepOrder:first').parents("td").addClass('selected').prev().addClass("beforeSelected");
	} else {
		var counter = 0;
		$(".stepOrder").each(function(){
			counter++;
			if ($('meta[content*="Step '+counter+'"]').attr("content")) {
				$(this).parents("td").addClass("selected").prev().addClass("beforeSelected");
			}
		});
	}
	// add classes to first and last #stepNavigation a elements
	$("#stepNavigation a").first().addClass("first");
	$("#stepNavigation a").last().addClass("last");
	*/

	// change and return search input value
	$('#fnheadersearch input[type=text]').focus(function(){ 
		//if($(this).val() == $(this).attr('defaultValue')) {
        if($(this).val() == 'Search') {
			$(this).val('');
			$(this).addClass('focused');
		}
	});	

	$('#fnheadersearch input[type=text]').blur(function(){
		if($(this).val() == '') {
			$(this).val('Search');
			$(this).removeClass('focused');
		} 
	});

	$("#fnselectLanguage").mouseup(function() {		
		if( $('#fnLanguagesNav').is(':visible') ) {
	    	$('#fnLanguagesNav').css("display","none");	
	    	$('#regional').removeClass("active"); 	    		    		
		}
		else {
			$('#fnLanguagesNav').css("display","block");
			$('#regional').addClass("active"); 
		}
	});
	
	$("#fnheadersubmitSearch").click(function() {
		if ( $("#fnheadersearchInput").val() == "Search" ) {
			$("#fnheadersearchInput").val("");
		}
	});
	
	$("#regional").mouseover(function() {
		$('#regional').addClass("active"); 
	});	
	$("#regional").mouseout(function() {	
		$('#regional').removeClass("active"); 
	});		
	
	$('#fnregionalSites > li').hover(function() {		        
		$(this).toggleClass("active"); 	
		$('#fnregionalSitesNav').toggle();
	});			

    /*
	// add classes to first and last .solution elements
	$(".solution").first().addClass("firstSolution");
	$(".solution").last().addClass("lastSolution");

	// remove margins from .signpost elements, due to IE7 bug :first-child bug 
        $(".signpostsHalfWidth").each(function(){      
          jQuery(this).find('.signpost').eq(0).css({ marginLeft: '0px'});
        });
        $(".signpostsThirdWidth").each(function(){      
          jQuery(this).find('.signpost').eq(0).css({ marginLeft: '0px'});
        });
	
	// add id's to #article h2 elements for "section navigation"		
	if ($('meta[name=articleInnerNavigation]').attr("content") != "false") {
		if ($('.article h2').size() > 0) {
			$('#articleColumn').prepend('<div class="resource-box"><h2 class="toc-title">Contents</h2><ul id="nDevInpageToc"></ul></div>');
			$(".article h2").each(function(i) {
				$(this).attr('id', 'article'+i);
                        	var tocLink =  $('<a href="#article'+i+'"></a>').html($(this).text());
				$('#nDevInpageToc').append('<li><a href="#article'+i+'">' + $(this).text() + '</a></li>');
			});
			
			$("#nDevInpageToc a").click(function() { 
				var anchorRef = $(this).attr('href');
				if (anchorRef.search('#') > -1) {
					$('html, body').animate({scrollTop:$(anchorRef.substr(anchorRef.search("#"),anchorRef.length)).offset().top}, 500, function(){});
				}
			});
		}
	}
	*/

	// content owner information box
	$("#fncontentOwner").click(function() {
		$(this).fadeOut("slow");
	});
	$("#fncontentOwner").hover(
		function() { $(this).css("background-color","#ffffbb") },
		function() { $(this).css("background-color","#f2f2b1") }
	);	

    /*
	// create rounded corners on images	
        $(".roundImage").load(function() { $(this).each(function() {
                $(this).wrap('<span class="roundCorner"></span>');
                $(this).parent().css("height", $(this).height());
                $(this).parent().css("width", $(this).width());
          })
        });

	$(".roundImageLarge").each(function() {
		$(this).wrap('<span class="roundCornerLarge"></span>');	
		$(this).parent().css("height", $(this).height());
		$(this).parent().css("width", $(this).width());		
	});

	$('.tl').before("<div class='roundCornerTl'></div>");
	$('.tr').before("<div class='roundCornerTr'></div>");
	$('.bl').before("<div class='roundCornerBl'></div>");
	$('.br').before("<div class='roundCornerBr'></div>");	
*/

    /*
	//Accordions 
    $(".accordeonsCollapsable .accordeonTitle").click(function(){
        //var thisAccordeon = $(this).parents('.accordeonContainer').first();
            if(!(jQuery(this).parent().hasClass('noToggling'))){
		$(this).siblings(".accordeonContent").toggle();
		$(this).parent().toggleClass("accordeonExpanded");
            }
    });
		
	$(".accordeonExpandAll").click(function() {
		$(".accordeonContent").show();
		$(".accordeonContainer").addClass("accordeonExpanded");
	});
	
	$(".accordeonCollapseAll").click(function() {
		$(".accordeonContent").hide();
		$(".accordeonContainer").removeClass("accordeonExpanded");
		$("#Technical_Specs.accordeonContent").show();
	});
	*/

    /*
	// generate equal heighted elements
	$.fn.equalHeight = function () {
		var height        = 0;
		var maxHeight    = 0;

		// Store the tallest element's height
		this.each(function () {
			height        = $(this).outerHeight();
			maxHeight    = (height > maxHeight) ? height : maxHeight;
		});

		// Set element's min-height to tallest element's height
		return this.each(function () {
			var t            = $(this);
			var minHeight    = maxHeight - (t.outerHeight() - t.height());
			var property    = 'height';

			t.css(property, minHeight + 'px');
		});
	};	
	// generate equal height for all .signpostContent elements
	$('.signpostsHalfWidth .signpostContent').equalHeight();
	$('.signpostsThirdWidth .signpostContent').equalHeight();
	$('#fnspotlightList .spotlight').equalHeight();
    */
	
	// open rel=external links to new window
	$("a[rel=external]").attr('target', '_blank');	
	
	// function for adding z-indexes for IE7 bug
	$(function() {
		var zIndexNumber = 100;
		$('.sectionNavigation').each(function() {
			$(this).css('zIndex', zIndexNumber);
			zIndexNumber -= 1;
		});
	});
	
    /*
	// spotlight image gallery
	// show the first thumbnail
	initSpotlightImageGallery();
	*/
	
	// change the width of the caption div to match width of img
	$(".caption").each(function () {		
		imgWidth = $(this).children("img").attr("width");		
		$(this).width(imgWidth);
	});

    /*
	// initialize fancybox for spotlight elements
	$(".spotlight-pane a, .spotlight a, .spotLink a").click( fbshowVideo);
	*/

    /*
	$(".spotlight-pane a, .spotlight a, .spotLink a").fancybox({
			'autoDimensions': false,
			'height'		: 'auto',
			'titleShow'		: false,
			'width'			: 'auto',
			'overlayShow'	: true,
			'overlayColor'	: '#000000',
			'overlayOpacity': 0.3,
			'transitionIn'	: 'elastic',
			'transitionOut'	: 'elastic',
			'scrolling'		: 'no'			
	});
		*/

    /*
	$(".selectMenu").selectmenu({
            transferClasses: true,
            style:'dropdown',
            width: 200
        });
        */

    /*
	$(".selectMenuOneThirdWidth").selectmenu({
            transferClasses: true,
            style:'dropdown',
            width: 255
        });
        */

    /*
        // initialize flash videos
        $(".swfobj").each(function(intIndex) {
            var file = $(this).html();
            if (file && file.length > 0) {
                var flashvars = {};
                var params = {wmode : "transparent", allowscriptaccess:'always'};
                var attributes = {};
 
                var dims = $(this).attr("title");
                var w = 500;
                var h = 400;
                if (dims && dims.length > 0) {
                    w = dims.replace(/[xX].*$/, "");
                    h = dims.replace(/^.*[xX]/, "");
                } else if($(this).hasClass('infostream')) {
                	// for streaming_uri resources..
					if(/fnWidth/.test(file) && /fnHeight/.test(file)) {
						w = parseInt(file.match(/fnWidth=([0-9]*)/)[1]);
						h = parseInt(file.match(/fnHeight=([0-9]*)/)[1]);
					} else {
						w = 840;
						h = 685;
					}
				}
                //swfobject.embedSWF(file, this.id, w, h, "9.0.0", "", flashvars, params, attributes);
                // current swfobject doesn't seem to work with some video/browser combinations
                var str = '<object classid="clsid:D27CDB6E-AE6D-11cf-96B8-444553540000" width="' + w + '" height="' + h + '"><param name="movie" value="' + file + '"></param>';
				var emb = '';
				$.each(params, function(name, val) {
					str += '<param name="' + name + '" value="' + val + '"></param>';
					emb += ' ' + name + '="' + val + '"';
				});
				str += '<embed src="' + file + '" type="application/x-shockwave-flash" width="' + w + '" height="' + h + '"' + emb + '></embed></object>';
				$(this).replaceWith(str);
            }
        });
        */

        // initialize fancybox for flash video link
    /*
        $("a.fnflash").each(function(index, element) {
                var w=parseInt($(this).attr('width'));
                var h=parseInt($(this).attr('height'));
                if($(this).hasClass('infostream')) {
					var file = $(this).attr('href');
					if(/fnWidth/.test(file) && /fnHeight/.test(file)) {
						w = parseInt(file.match(/fnWidth=([0-9]*)/)[1]);
						h = parseInt(file.match(/fnHeight=([0-9]*)/)[1]);
					} else {
						w = 840;
						h = 685;
					}
				}
                if (isNaN(w)) w = 250;
                if (isNaN(h)) h = 150;
                $(element).fancybox({
                        'height' : h,
                        'width' : w,
                        'padding' : 10,
                        'margin' : 20,
                        'autoScale' : false,
                        'autoDimensions' : false,
                        'transitionIn'  : 'none',
                        'transitionOut' : 'none',
                        'type' : 'swf',
                        'overlayShow' : true,
                        'overlayColor' : '#000000',
                        'overlayOpacity' : 0.3,
                        'wmode' : 'opaque',
                        'hideOnContentClick' : false,
                        'titlePosition' : 'inside'
                        });
        });
        */

        
    /* Search 
    jQuery(".fnSearchCriteria a").click(function(){
        var criteriaArray = jQuery(this).attr('id').split("_");
        changeSearchCriteria(jQuery(this), criteriaArray[0], criteriaArray[1]);
        return false;
    });
    */

    /*
	jQuery(".fnCalendar ul li a").mouseover(function(){
        var el = jQuery(this).parents('li').eq(0);
        var t = setTimeout(function() {
            el.find('.fnCalEventBg').animate( { width: "100%" }, 500);        
            el.find('.fnCalendarEvent a').animate( { color: "#fff" }, 300);  
        }, 400);
        $(this).data('timeout', t);
      
    }).mouseout(function(){
        clearTimeout($(this).data('timeout'));
        if($(this).parents('li').hasClass('opened')){
        
        }else{
            resetCalEventBg($(this).parents('li').eq(0));
        }
    });

    var calNr = 21;

	var t = setTimeout(function(){
		$('.fnCalPanel').fadeOut();	                                           
	}, 3000); 	
	
    jQuery(".fnCalendarEvent").click(function(){
		clearTimeout(t); 	
        jQuery(this).parents('li').eq(0).addClass('opened');
        var elLeft = jQuery(this).parents('li').eq(0).position().left;
        var elTop = jQuery(this).parents('li').eq(0).position().top - 1;
        var panelLeft = "";

        if(elLeft > 550){
            panelLeft = -332;
            jQuery(this).parents('li').eq(0).find('.fnCalPanel').eq(0).addClass('fnCalPanelRight');
        }else{
            panelLeft = 125;
        }

        jQuery(this).parents('li').eq(0).find(".fnCalPanel").eq(0).css({left: panelLeft + 'px', top: '-22px'}).fadeIn();
        jQuery(this).parents('li').eq(0).css('z-index', calNr)
        calNr++;

        return false;
    });
    jQuery(".closeFnCalPanel").click(function(){
        resetCalEventBg(jQuery(this).parents('li').eq(0));
        jQuery(this).parents('li').eq(0).removeClass('opened');
        jQuery(this).parents('.fnCalPanel').eq(0).fadeOut(function(){            
            jQuery(this).parents('li').eq(0).find('.fnCalPanel').eq(0).removeClass('fnCalPanelRight');   
        });
    });

	$('.fnCalPanel').hover (		
	  function () {			
		clearTimeout(t); 
	  },
	  function () {
		var calParent = $(".fnCalPanel").parents('li');

		t = setTimeout(function(){			
			resetCalEventBg(calParent);
			calParent.removeClass('opened');						
			$('.fnCalPanel').fadeOut();	                                           
		}, 3000); 
	  }
	);
	*/

    /*
	if(location.href.indexOf('/gen/videos_all.xhtml') != -1) {
	var v_id = FNVideoUtil.getParameterByName("id");
	 if( v_id === undefined || v_id==null || v_id == "") {
		 	return ;
	 } else{
	 		FNVideoUtil.showVideo( v_id );
	 }
	 }
	 */

    /* jQuery UI radiobutton, checkbox *//*
    $("#fncontainer input").filter(":checkbox,:radio").checkbox();
*/

});

/*
// request serial number (abstract tool page)
function requestSerial() {
    var fileId = "";
    if (document.getElementById("version").fileIdSerial) {
        var parameter = document.getElementById("version").fileIdSerial.options[document.getElementById("version").fileIdSerial.selectedIndex].value;
        var split = parameter.indexOf('-');
        var productName = parameter.substring(0,split);
        var productVersion = parameter.substring(split+1,parameter.length);
    } else {
        alert("Failed");
    }
    var url = "/product_request_serial?productName=" + productName + "&productVersion=" + productVersion;
    if (url != "") {
        document.location = url;
    }
}

//abstract tool page
function getGenericURI() {
    var id = document.getElementById('linkURL');
    var path = document.getElementById("version").fileId.options[document.getElementById("version").fileId.selectedIndex].value;
    id.href = path;
}

// download of multiple file (abstract tool page)
function getDLMethod() {
    var path = document.getElementById("version").fileId.options[document.getElementById("version").fileId.selectedIndex].value;
    document.location = path;
}
*/

// show china ICP text for chinese users (called by Akamai)
function show_icp() {
    document.getElementById("china_icp").style.visibility = "visible";
}

/*
var FNVideoUtil = {};

FNVideoUtil.video_base_page = '/gen/videos_all.xhtml';

FNVideoUtil.showVideo = function(v_id) {
   
   var shared_URL = v_id,
   bread = '';
   
   if (v_id == undefined || v_id == null || v_id=="") {
   	return;
   }
   
   if(v_id.substr(0,7) === 'http://') {
   		shared_URL =  	v_id.substr(23, v_id.length);
    	shared_URL = 'http://' + window.location.host + this.video_base_page +'?id=' + shared_URL;
		v_id = location.protocol +"//" +window.location.host + "/info/" + v_id.substr(7,v_id.length ) +  ".snippet.html";   
   }else{
   	//from Social websites ...
   	   shared_URL = v_id;
	   v_id =  location.protocol + "//" +window.location.host + "/info/sw.nokia.com/id/" +v_id + ".snippet.html";  
   }
 	
   //cleanup of previous ones..
   if( $("#fnbreadCrumbsVdo")) {
		$("#fnbreadCrumbsVdo").remove();
   }
			
   if ($("#fnshowURL")) {
   		$("#fnshowURL").remove();
   }
  									
   bread = FNVideoUtil.bread + '<div id="sharedURL" data='+ shared_URL +'> </div>' ;
 
	$.ajax({
		type	: "GET",
		cache	: false,
		url		: v_id,
 		success: function(data) {
 			$.fancybox( bread + data + FNVideoUtil.script_part,
			{
        	'autoDimensions'	: false,
			'width'         		: 'auto',
			'height'        		: 'auto',
			'transitionIn'		: 'none',
			'transitionOut'		: 'none',
			'scrolling'			:	'no'
		});
		},
		error:function() { alert("Requested video not available."); }
	}) ;
}

FNVideoUtil.getParameterByName = function( name )
{
  name = name.replace(/[\[]/,"\\\[").replace(/[\]]/,"\\\]");
  var regexS = "[\\?&]"+name+"=([^&#]*)";
  var regex = new RegExp( regexS );
  var results = regex.exec( window.location.href );
  if( results == null )
    return "";
  else
    return decodeURIComponent(results[1].replace(/\+/g, " "));
}
*/

/*
FNVideoUtil.showSocial = function(socialId){
	
	var ehref = '',
		etitle = 'Developer Nokia Videos' ,
		shared_URL,
		v_id ='';
		
	if(document.getElementById("sharedURL")) {
		v_id = document.getElementById("sharedURL").getAttribute("data");
		
		//coming from Learning pages 
		if( v_id.indexOf(window.location.host + this.video_base_page ) != -1 ){
			shared_URL = v_id;
		}
		//coming from /gen/videos_all.xhtml
		else if( v_id.substr(0,7) === 'http://sw.nokia.com' ) {
	   		shared_URL =  	v_id.substr(23, v_id.length);
			shared_URL = "http://" + window.location.host + this.video_base_page + '?id=' + shared_URL;
			v_id = "http://" +window.location.host + "/info/" + v_id.substr(7,v_id.length ) +  ".snippet.html";   
	   	}else{
   			//from Social websites ...
   	   		 shared_URL = v_id;
	  		 v_id = "http://" +window.location.host + this.video_base_page + '?id=' +v_id ;  
  		 }
		 ehref =v_id;
	}else{
		//coming from /gen/videos_all.html
		var cnodes ;
		if (document.getElementById("fancybox-inner")) {
			cnodes = document.getElementById("fancybox-inner").childNodes;
			
			for (var i = 0; i < cnodes.length; ++i) {
				if (cnodes[i].id && cnodes[i].id.indexOf("spotlightContent") != -1) {
					var spid = cnodes[i].id;
					var surlid = spid.substr(spid.indexOf('-'), spid.length);
					if(document.getElementById("spotlightsharedURL" + surlid)) 
						v_id = document.getElementById("spotlightsharedURL" + surlid).getAttribute("data-sharedURL");
				}
			}
			if ( v_id != '' && v_id.substr(0, 19) === 'http://sw.nokia.com') {
				shared_URL = v_id.substr(23, v_id.length);
				shared_URL = 'http://' + window.location.host + this.video_base_page + '?id=' + shared_URL;
				ehref = shared_URL;
			}
		}
	}
	
	if (ehref == '') {
		alert("No Video available for sharing.");
		return;
	}
	
	etitle = $("#fancybox-inner").find("h2").html();
	
	if(etitle == null || etitle == '') {
		etitle = 'Developer Nokia Videos' ;
	}else {
		etitle = jQuery.trim( etitle );
	}
	
	switch(socialId) {
	
	case "fnbookmarkClipBoardVdo" :
	    this.showShareURL(ehref);		
	    break;  
			
	case "fnbookmarkDeliciousVdo" :		
		window.open('http://delicious.com/save?v=5&noui=&jump=close&url='+ehref+'&title='+etitle+'','delicious','toolbar=no,width=550,height=550,resizable=yes'); 
	    break;  	

	case "fnbookmarkDiggVdo" : 		
		window.open('http://digg.com/submit?url='+ehref+'&title='+etitle+'&bodytext=&media=news&topic=tech_news','digg','toolbar=no,width=1070,height=750,scrollbars=yes,resizable=yes');
 	    break;
		
	case "fnbookmarkGoogleVdo" : 		
		window.open('http://www.google.com/bookmarks/mark?op=add&hl=en&bkmk='+ehref+'&title='+etitle+'','google','toolbar=no,width=650,height=600,resizable=yes');
		break;
	
	case "fnbookmarkYahooVdo" : 		
		window.open('http://myweb2.search.yahoo.com/myresults/bookmarklet?u='+ehref+'&t='+etitle+'','yahoo','toolbar=no,width=850,height=650,scrollbars=yes,resizable=yes');
		break;
       		
	case "fnbookmarkStumbleVdo" : 		
		window.open('http://www.stumbleupon.com/submit?url='+ehref+'&title='+etitle+'','stumbleupon','toolbar=no,width=1070,height=550,scrollbars=yes,resizable=yes');
	    break;

	case "fnbookmarkRedditVdo" : 		
		window.open('http://www.reddit.com/submit?url='+ehref+'&title='+etitle+'','reddit','toolbar=no,width=650,height=650,resizable=yes');
	    break;

	case "fnbookmarkDiigoVdo" : 		
		window.open('http://www.diigo.com/import_all/transfer_furl?u='+ehref+'&t='+etitle+'','diigo','toolbar=no,width=1100,height=900,scrollbars=yes,resizable=yes');
	    break;
	
	case "fnbookmarkFurlVdo" : 		
		window.open('http://www.furl.net/storeIt.jsp?u='+ehref+'&t='+etitle+'','furl','toolbar=no,width=600,height=750,scrollbars=yes,resizable=yes');
	    break;
	    
	case "fnbookmarkTechnoratiVdo" : 		
		window.open('http://technorati.com/faves?add='+ehref+'','technorati','toolbar=no,width=1000,height=750,scrollbars=yes,resizable=yes');
	    break;
	
	case "fnbookmarkNewsvineVdo": 		
		window.open('http://www.newsvine.com/_tools/seed&save?u='+ehref+'&h='+etitle+'','newsvine','toolbar=no,width=970,height=750,resizable=yes');
	 	break;
	
	case "fnbookmarkMagnoliaVdo":		
		window.open('http://ma.gnolia.com/bookmarklet/add?url='+ehref+'&title='+etitle+'&description=','magnolia','toolbar=no,width=800,height=600,scrollbars=yes,resizable=yes');
	 	break;
	
	case "fnbookmarkSquidooVdo": 		
		window.open('http://www.squidoo.com/lensmaster/bookmark?'+ehref+'','squidoo','toolbar=no,width=750,height=600,scrollbars=yes,resizable=yes');
	 	break;
	
	case "fnbookmarkTwitterVdo": 		
		window.open('http://twitter.com/home?status='+ehref+'','twitter','toolbar=no,width=790,height=550,resizable=yes');
	 	break;
	
	case "fnbookmarkFacebookVdo":
	
	var data_stuff = $("#fancybox-inner").find("div[id^=spotlightsharedURL]"),icon_src,desc,t_title ;
	icon_src = data_stuff.attr('data-icon');
	if( icon_src!=null && icon_src.match("^/images") ) {
	    icon_src = '//'+window.location.host +icon_src	
	}	
	desc = data_stuff.html();
	t_title = data_stuff.attr('data-title');
	if(icon_src !=null && desc!=null)
		window.open('http://www.facebook.com/sharer.php?s=100&p[title]='+t_title+'&p[summary]='+ desc +'&p[url]='+ehref+'&p[images][0]=http:'+icon_src,'sharer','toolbar=0,status=0,width=550,height=550,resizable=yes')
	else {
         //current implementation.		
	  	window.open('http://www.facebook.com/sharer.php?u='+ehref+'&t='+etitle+'','facebook','toolbar=no,width=550,height=550,resizable=yes');
	}
	break;
	
	case "fnbookmarkBrowserVdo":	
		this.bookmarksite(etitle, ehref);
	 	break;
	
	}	
 }
*/

/*
 FNVideoUtil.showShareURL = function(ehref){
 	document.getElementById("fnshowURL").innerHTML = '<p class="highlight">' + ehref + '</p>';
 }

 FNVideoUtil.bookmarksite = function(title, url){
		if (window.sidebar) // firefox
			window.sidebar.addPanel(title, url, "");
		else if (window.opera && window.print){ // opera
			var elem = document.createElement('a');
			elem.setAttribute('href',url);
			elem.setAttribute('title',title);
			elem.setAttribute('rel','sidebar');
			elem.click();
		}
		else if(document.all)// ie
			window.external.AddFavorite(url, title);
	}
	*/

/*
FNVideoUtil.script_part ="<script type='text/javascript' >" + 
   				'$(document).ready(function() {	'+
 	'if(navigator.userAgent.match(/iPhone|iPod|Maemo|Tablet browser|SymbianOS/) != null) {'+
 	'	$("#fntitleToggleVdo").click(function(){'+
			'$("#fnbookMarksVdo").toggle();'+
			
		'});'+
	'} else {'+
 		'$("#fntitleToggleVdo").hover('+
			'function(){	'+
				'$("#fnbookMarksVdo").show();'+
			'},'+
			'function(){'+
				'$("#fnbookMarksVdo").hide();'+
			'}'+
		');'+
  	'}'+
	'$("#fntitleToggleVdo").hover(function(){$("#fnbookMarksVdo").show();},function(){$("#fnbookMarksVdo").hide();});' +
'});'+
"<\/script>";

FNVideoUtil.bread = '<div><div id="fnshowURL"></div>'+
   '<div id="fnbreadCrumbsVdo">' +
   '<div id="fntitleToggleVdo"><a href="#">Share</a>' +
   '<div id="fnbookMarksVdo">' +
    '<img onClick="FNVideoUtil.showSocial(this.id)" id="fnbookmarkClipBoardVdo" src="/images/footericon_clipboard.png" title="Show sharable link" alt="Share link" width="16" height="16" />' +
	'<img onClick="FNVideoUtil.showSocial(this.id)" id="fnbookmarkBrowserVdo" src="/images/footericon_bookmark.gif" title="Bookmark This Page" alt="Bookmark" width="16" height="16" />' +
	'<img onClick="FNVideoUtil.showSocial(this.id)" id="fnbookmarkDeliciousVdo" src="/images/footericon_delicious.gif" title="Delicious" alt="Delicious" width="16" height="16" />' +
	'<img onClick="FNVideoUtil.showSocial(this.id)" id="fnbookmarkDiggVdo" src="/images/footericon_digg.gif" title="Digg" alt="Digg" width="16" height="16" />' +
    '<img onClick="FNVideoUtil.showSocial(this.id)" id="fnbookmarkFacebookVdo" src="/images/footericon_facebook.gif" title="Facebook" alt="Facebook" width="16" height="16" />' +
	'<img onClick="FNVideoUtil.showSocial(this.id)" id="fnbookmarkGoogleVdo" src="/images/footericon_google.gif" title="Google" alt="Google" width="16" height="16" />' +
	'<img onClick="FNVideoUtil.showSocial(this.id)" id="fnbookmarkYahooVdo" src="/images/footericon_yahoo.gif" title="Yahoo" alt="Yahoo" width="16" height="16" />' +
	'<img onClick="FNVideoUtil.showSocial(this.id)" id="fnbookmarkStumbleVdo" src="/images/footericon_temp1.gif" title="StumbleUpon" alt="StumbleUpon" width="16" height="16" />' +
	'<img onClick="FNVideoUtil.showSocial(this.id)" id="fnbookmarkRedditVdo" src="/images/footericon_reddit.gif" title="Reddit" alt="Reddit" width="16" height="16" />' +
	'<img onClick="FNVideoUtil.showSocial(this.id)" id="fnbookmarkDiigoVdo" src="/images/footericon_diigo.GIF" title="Diigo" alt="Diigo" width="16" height="16" />' +
	'<img onClick="FNVideoUtil.showSocial(this.id)" id="fnbookmarkTechnoratiVdo" src="/images/footericon_technocrati.gif" title="Technorati" alt="Technorati" width="16" height="16" />' +
	'<img onClick="FNVideoUtil.showSocial(this.id)" id="fnbookmarkTwitterVdo" src="/images/footericon_twitter.gif" title="Twitter" alt="Twitter" width="16" height="16" />' +
  '</div>' +
 '</div>' +
 '</div></div>';
 */

/*
function fbshowVideo(){
	var contentid = this.href.substr( this.href.indexOf('#') , this.href.length),
	spcount = contentid.substr( contentid.indexOf('-') , contentid.length),
	landingPageFlash = 'landingPageAppSpotlightFlash', landingPageYoutube = 'landingPageAppSpotlightYoutube',
	content;

	if(  $(contentid).find('#spotlightsharedURL' +spcount).length >0  )  {
		if( $("#fnbreadCrumbsVdo")) {
				$("#fnbreadCrumbsVdo").remove();
		}
		if ( $("#fnshowURL") ){
				$("#fnshowURL").remove();
		}
		if ( contentid.indexOf(landingPageFlash) != -1 ) {
			content = FNVideoUtil.bread + FNVideoUtil.script_part + '<div id='+contentid +' class="spotlightContentFPFlash">'  +  $(contentid).html() + '</div>';
		}
		else if ( contentid.indexOf(landingPageYoutube) != -1 ) {
			content = FNVideoUtil.bread + FNVideoUtil.script_part + '<div id='+contentid +' class="spotlightContentFPYoutube">'  +  $(contentid).html() + '</div>';
		}
		else {
			content = FNVideoUtil.bread + FNVideoUtil.script_part + '<div id='+contentid +' class="spotlightContent">'  +  $(contentid).html() + '</div>';
		}
	}else{
		if ( contentid.indexOf(landingPageFlash) != -1 ) {
			content = '<div id='+contentid +' class="spotlightContentFPFlash">' +  $(contentid).html() + '</div>';
		}
		else if ( contentid.indexOf(landingPageYoutube) != -1 ) {
			content = '<div id='+contentid +' class="spotlightContentFPYoutube">' +  $(contentid).html() + '</div>';
		}
		else {
			content = '<div id='+contentid +' class="spotlightContent">' +  $(contentid).html() + '</div>';
		}
		content = content + '<script type="text/javascript">FNVideoUtil.initSpotlightImageGallery("'+contentid+'");</script>'

	}

	$.fancybox(content,
			{
			'autoDimensions': false,
			'autoScale'     : false,
			'height'		: 'auto',
			'titleShow'		: false,
			'width'			: 'auto',
			'overlayShow'	: true,
			'overlayColor'	: '#000000',
			'overlayOpacity': 0.3,
			'transitionIn'	: 'elastic',
			'transitionOut'	: 'elastic',
			'scrolling'		: 'no'			
	});
}
*/

/*
FNVideoUtil.initSpotlightImageGallery  = function(contentid) {

	$(contentid).find(".spotlightImages").each(function() {	
		if( $(this).children().children().size() > 1) {
			// onclick remove previous image, clone new one, add active class
			$(".spotlightThumbs img").click(function() {
				$(this).parent().siblings("img").fadeOut().remove();
				$(this).clone().removeAttr("width").removeAttr("height").fadeIn().prependTo($(this).parent().parent());
					$(contentid).find(".spotlightThumbs img").each(function() {
						if ( $(this).hasClass("active") ) {
						   $(this).removeClass("active");
						}
						
					});
				$(this).addClass("active");
			});		
		} else {
			$(this).children().children().hide();
		}
	});
}
*/
