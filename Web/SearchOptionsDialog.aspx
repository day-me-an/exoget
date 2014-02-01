<%@ Import Namespace="Exo.Exoget.Model.Search" %>
<%@ Import Namespace="Exo.Exoget.Web" %>
<%@ Page Language="C#" AutoEventWireup="false" CodeFile="SearchOptionsDialog.aspx.cs" Inherits="SearchOptionsDialog" %>

<%
SearchOptions currentSearchOptions = Helper.CurrentSearchOptions;
%>

<div id="searchFormOptionsContainer">
    <fieldset id="searchFormOptions">
        <legend class="title">Search Options</legend>

        <div class="close"></div>

        <label for="so_duration">Duration</label>
        <select id="so_duration" name="so_duration">
            <option value="<%=(uint)SearchOptions.DurationOne%>"<%if((currentSearchOptions & SearchOptions.DurationOne) == SearchOptions.DurationOne){%> selected="selected"<%}%>>&lt; 1 min</option>
            <option value="<%=(uint)SearchOptions.DurationTwo%>"<%if((currentSearchOptions & SearchOptions.DurationTwo) == SearchOptions.DurationTwo){%> selected="selected"<%}%>>≥ 1 min</option>
            <option value="<%=(uint)SearchOptions.DurationThree%>"<%if((currentSearchOptions & SearchOptions.DurationThree) == SearchOptions.DurationThree){%> selected="selected"<%}%>>≥ 10 min</option>
            <option value="<%=(uint)SearchOptions.DurationFour%>"<%if((currentSearchOptions & SearchOptions.DurationFour) == SearchOptions.DurationFour){%> selected="selected"<%}%>>≥ 30 min</option>
        </select>

        <fieldset>
            <legend>Quality</legend>
            
            <input id="so_qualityPoor" type="checkbox" name="so_<%=(uint)SearchOptions.QualityPoor%>" value="1"<%if((currentSearchOptions & SearchOptions.QualityPoor) == SearchOptions.QualityPoor){%> checked="checked"<%}%> />
            <label for="so_qualityPoor">Poor</label>

            <input id="so_qualityOk" type="checkbox" name="so_<%=(uint)SearchOptions.QualityOk%>" value="1"<%if((currentSearchOptions & SearchOptions.QualityOk) == SearchOptions.QualityOk){%> checked="checked"<%}%> />
            <label for="so_qualityOk">OK</label>

            <input id="so_qualityGood" type="checkbox" name="so_<%=(uint)SearchOptions.QualityGood%>" value="1"<%if((currentSearchOptions & SearchOptions.QualityGood) == SearchOptions.QualityGood){%> checked="checked"<%}%> />
            <label for="so_qualityGood">Good</label>
                            
            <input id="so_qualityExcellent" type="checkbox" name="so_<%=(uint)SearchOptions.QualityExcellent%>" value="1"<%if((currentSearchOptions & SearchOptions.QualityExcellent) == SearchOptions.QualityExcellent){%> checked="checked"<%}%> />
            <label for="so_qualityExcellent">Excellent</label>                    
        </fieldset>
        
        <fieldset>
            <legend>Format</legend>

            <input id="so_formatMp3" type="checkbox" name="so_<%=(uint)SearchOptions.FormatMp3%>" value="1"<%if((currentSearchOptions & SearchOptions.FormatMp3) == SearchOptions.FormatMp3){%> checked="checked"<%}%> />
            <label for="so_formatMp3">MP3</label>

            <input id="so_formatMsmedia" type="checkbox" name="so_<%=(uint)SearchOptions.FormatMsMedia%>" value="1"<%if((currentSearchOptions & SearchOptions.FormatMsMedia) == SearchOptions.FormatMsMedia){%> checked="checked"<%}%> />
            <label for="so_formatMsmedia">Windows</label>

            <input id="so_formatRealmedia" type="checkbox" name="so_<%=(uint)SearchOptions.FormatRealmedia%>" value="1"<%if((currentSearchOptions & SearchOptions.FormatRealmedia) == SearchOptions.FormatRealmedia){%> checked="checked"<%}%> />
            <label for="so_formatRealmedia">RealMedia</label>

            <input id="so_formatQuicktime" type="checkbox" name="so_<%=(uint)SearchOptions.FormatQuicktime%>" value="1"<%if((currentSearchOptions & SearchOptions.FormatQuicktime) == SearchOptions.FormatQuicktime){%> checked="checked"<%}%> />
            <label for="so_formatQuicktime">Quicktime</label>

            <input id="so_formatMpeg4" type="checkbox" name="so_<%=(uint)SearchOptions.FormatMp4%>" value="1"<%if((currentSearchOptions & SearchOptions.FormatMp4) == SearchOptions.FormatMp4){%> checked="checked"<%}%> />
            <label for="so_formatMpeg4">MP4</label>

            <%--
            <input id="so_formatOther" type="checkbox" name="so_<%=(uint)SearchOptions.FormatMp3%>"<%if((currentSearchOptions & SearchOptions.QualityPoor) != 0){%> checked="checked"<%}%> />
            <label for="so_formatOther">Other</label>
            --%>
        </fieldset>
        
        <button class="saveOptions">Save As Defaults</button>                
    </fieldset>
</div>
