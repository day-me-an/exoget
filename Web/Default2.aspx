<%@ Page Language="C#" MasterPageFile="~/HeaderAndFooter.master" AutoEventWireup="true" CodeFile="Default2.aspx.cs" Inherits="Default2" Title="Home 2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="content" Runat="Server">

<style type="text/css">

/* begin css tabs */
.tabs
{
	padding:0;
	margin:0;
}

.tabs li
{
    display:block;
    float:left;
}

.tabs li a
{
    display:block;
    padding:5px;

    padding-left:1em;
    padding-right:1em;
    margin-right:8px;

    border:2px solid #9bbef4;
    border-top-left-radius:15px 15px;
    border-top-right-radius:15px 15px;
    -moz-border-radius-topleft:15px;
    -moz-border-radius-topright:15px;
    -webkit-border-top-left-radius:15px 15px;
    -webkit-border-top-right-radius:15px 15px;
    
    text-align:center;
}

.tabs li a:hover
{
    border-color:orange;
    text-decoration:none;
}
/* end css tabs */

.userProfile .menu
{
float:none;
margin-right:4px;
margin-left:4px;
width:auto;
}

.menuHolder
{
float:left;
border:solid 1px #33cc00;
border-top:none;
padding:0;
width:20%;
}

.menuHolder h3
{
margin:0;
margin-bottom:8px;
background:#f4fff4;
padding:3px;
text-align:center;
font-size:115%;
}

.mediaRow p
{
font-size:smaller;
}

.mediaRow .props li
{
display:none;
}

.mediaRow .props li.published
{
display:block;
border-right:none;
}

.mediaRowList
{
	padding-top:6px;
}

</style>

<script type="text/javascript">
var FIRST_INCOMING_DELAY = 1000;
var INCOMING_DELAY = 5000;
var INCOMING_COUNT = 10;

var firstIncomingCallDone = false;
var nextIncomingCount = 0;
var incomingPaused = false;
var incomingStart = -10;

var incomingArray = new Array();

/*
window.onblur = function()
{
  incomingPaused = true;
}

window.onfocus = function()
{
  incomingPaused = false;
}
*/

function getNextIncoming()
{
  if(incomingArray.length == 0)
  {
    // ajax to get another batch
    $.ajax
    ({
      async: false,
      type: "GET",
      url: "/IncomingFeeder.aspx",
      data:
      {
        start: (incomingStart + INCOMING_COUNT),
        count: INCOMING_COUNT
      },
      success:
      function(batch)
      {
        incomingStart += 10;
        
       $(".mediaRow", batch).each(function()
       {
        incomingArray.push(this);
       }); 
      }
    });
  }
  
  nextIncomingCount++;
  
  if(nextIncomingCount % 10 == 0)
    return $(incomingArray.pop()).css("background", "yellow");
    
  else
    return incomingArray.pop();
}

function mediaRowClickHandler()
{
  $(".props li", this).toggle();
}

function incomingHandler()
{
  if(incomingPaused)
    return;

  var nextIncoming = getNextIncoming();

  $(nextIncoming).click(mediaRowClickHandler);

  $(nextIncoming).hover
  (
    function()
    {
      incomingPaused = true;
    },
    
    function()
    {
      incomingPaused = false;
    }
  );

  if($(".mediaRowList .mediaRow").size() == INCOMING_COUNT)
    $(".mediaRowList .mediaRow:last").remove();
  
  $(nextIncoming).hide().prependTo(".mediaRowList").fadeIn(1500);

  if(!firstIncomingCallDone)
  {
    firstIncomingCallDone = true;
    setInterval(incomingHandler, INCOMING_DELAY);
  }
}

$(document).ready(function()
{
  setTimeout(incomingHandler, FIRST_INCOMING_DELAY);
  
  $(".mediaRow").hover
  (
    function()
    {
      incomingPaused = true;
    },
    
    function()
    {
      incomingPaused = false;
    }
  );
  
  $(".pause").click(function()
  {
    incomingPaused = ($(this).text() == "Pause");
    $(this).text(incomingPaused ? "Resume" : "Pause");
    
    return false;
  });
});
</script>

<div class="box">
    <div class="top">
        <h2>Arrivals</h2>
    </div>

    <div>
        <ul class="tabs">
            <li><a href="#">All</a></li>
            <li><a href="#">Business</a></li>
            <li><a href="#">Comedy</a></li>
            <li><a href="#">Technology</a></li>
        </ul>
        
        <br class="cb" />
        
        <div class="mediaRowList">
            
        </div>
    </div>

</div>

</asp:Content>

