<%@ Register TagPrefix="cc1" Namespace="WebControlCaptcha" Assembly="WebControlCaptcha" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeFile="UserForm.ascx.cs" Inherits="UserForm" %>

<form runat="server" id="userForm" method="post" action='/<%#Page.Request.Path%>' class="filloutForm">
    
    <%if (Mode == UserFormMode.Create)
      { %>
    <div style="margin-bottom:3px; clear:both">
        <p style="float:left">All fields marked with an asterisk (<span class="required">*</span>) are required.</p>
        <p style="float:right">Do you already have an exoGet Account? Please <a href="signin">Sign In</a>.</p>
    </div>
    <%} %>
    
        <fieldset class="cb">
            <legend>Account Details</legend>
            <ul class="inputContainer">
                <li<%if(!EmailPropertyProxyValidator.IsValid || !EmailCustomValidator.IsValid){ %> class="highlight"<%} %>>
                    <asp:label ID="Label1" runat="server" associatedcontrolid="email">Email Address<%if(Mode == UserFormMode.Create){%>#s#<span class="required">*</span><%}%></asp:label>
                    <input runat="server" class="large" id="email" name="email" type="text" />
                    
                    <el:PropertyProxyValidator
                        runat="server"
                        ID="EmailPropertyProxyValidator"
                        ControlToValidate="email"
                        PropertyName="Email"
                        SourceTypeName="Exo.Exoget.Model.User.UserInfo"
                        Display="Dynamic"
                        EnableClientScript="false"
                        CssClass="error"
                    />
                    
                    <asp:customvalidator runat="server" ID="EmailCustomValidator" ControlToValidate="email" OnServerValidate="ValidateEmail" Display="Dynamic" EnableClientScript="false" CssClass="error">
                    Sorry, that email address is being used by an existing account.
                    </asp:customvalidator> 
                </li>
                
                <%if (Mode == UserFormMode.Create)
                  { %>
                <li<%if(!UserNamePropertyProxyValidator.IsValid || !UserNameCustomValidator.IsValid){ %> class="highlight"<%} %>>
                    <asp:label ID="Label2" runat="server" associatedcontrolid="userName">User Name<%if(Mode == UserFormMode.Create){%>#s#<span class="required">*</span><%}%></asp:label>
                    
                    <input runat="server" id="userName" type="text" class="medium" />
                    
                    <el:PropertyProxyValidator
                        runat="server"
                        ID="UserNamePropertyProxyValidator"
                        ControlToValidate="userName"
                        PropertyName="Username"
                        SourceTypeName="Exo.Exoget.Model.User.UserInfo"
                        Display="Dynamic"
                        EnableClientScript="false"
                        CssClass="error"
                    />
                    
                    <asp:customvalidator runat="server" ID="UserNameCustomValidator" ControlToValidate="userName" OnServerValidate="ValidateUserName" Display="Dynamic" EnableClientScript="false" CssClass="error">
                    Sorry, that user name is already being used with an existing account.
                    </asp:customvalidator> 
                </li>
                <%} %>
                        
                <li<%if(!PasswordPropertyProxyValidator.IsValid){ %> class="highlight"<%} %>>
                    <asp:label ID="Label3" runat="server" associatedcontrolid="password"><%if(Mode == UserFormMode.Create){%>Choose<%}else{%>Change<%}%> Password<%if(Mode == UserFormMode.Create){%>#s#<span class="required">*</span><%}%></asp:label>
                    <input runat="server" id="password" type="password" class="medium" />
                    
                    <%if (Mode == UserFormMode.Create)
                      {%>
                    <el:PropertyProxyValidator
                        runat="server"
                        ID="PasswordPropertyProxyValidator"
                        ControlToValidate="password"
                        PropertyName="Password"
                        SourceTypeName="Exo.Exoget.Model.User.UserInfo"
                        Display="Dynamic"
                        EnableClientScript="false"
                        CssClass="error"
                    />
                    <%} %>
                </li>
                        
                <li<%if((Mode == UserFormMode.Create && !RetypePasswordRequiredFieldValidator.IsValid) || !RetypePasswordCompareValidator.IsValid){ %> class="highlight"<%} %>>
                    <asp:label ID="Label4" runat="server" associatedcontrolid="retypePassword">Retype Password<%if(Mode == UserFormMode.Create){%>#s#<span class="required">*</span><%}%></asp:label>
                    <input runat="server" id="retypePassword" type="password" class="medium" />
                    
                    <asp:requiredfieldvalidator runat="server" ID="RetypePasswordRequiredFieldValidator" ControlToValidate="retypePassword" Display="Dynamic" EnableClientScript="false" CssClass="error">
                    Please choose a password and retype it.
                    </asp:requiredfieldvalidator>
                    
                    <asp:comparevalidator runat="server" ID="RetypePasswordCompareValidator" ControlToValidate="retypePassword" ControlToCompare="password" display="Dynamic" enableclientscript="false" CssClass="error">
                    The &quot;Choose Password&quot; and &quot;Retype Password&quot; fields must be the same, please check each field.
                    </asp:comparevalidator>
                </li>
            </ul>
        </fieldset>
                
        <fieldset>
            <legend>About Yourself</legend>
            <ul class="inputContainer">
                      
                <li<%if(!FirstNamePropertyProxyValidator.IsValid){ %> class="highlight"<%} %>>
                <asp:label ID="Label5" AssociatedControlID="firstName" runat="server">First Name<%if(Mode == UserFormMode.Create){%>#s#<span class="required">*</span><%}%></asp:label>
                    <input runat="server" id="firstName" type="text" class="medium" />
                    
                    <el:PropertyProxyValidator
                        runat="server"
                        ID="FirstNamePropertyProxyValidator"
                        ControlToValidate="firstName"
                        PropertyName="FirstName"
                        SourceTypeName="Exo.Exoget.Model.User.UserInfo"
                        Display="Dynamic"
                        EnableClientScript="false"
                        CssClass="error"
                    />                        
                </li>
                      
                <li<%if(!LastNamePropertyProxyValidator.IsValid){ %> class="highlight"<%} %>>
                    <asp:label ID="Label6" AssociatedControlID="lastName" runat="server">Last Name<%if(Mode == UserFormMode.Create){%>#s#<span class="required">*</span><%}%></asp:label>
                    <input runat="server" id="lastName" type="text" class="medium"/>
                    
                    <el:PropertyProxyValidator
                        runat="server"
                        ID="LastNamePropertyProxyValidator"
                        ControlToValidate="lastName"
                        PropertyName="LastName"
                        SourceTypeName="Exo.Exoget.Model.User.UserInfo"
                        Display="Dynamic"
                        EnableClientScript="false"
                        CssClass="error"
                    />
                </li>
                      
                <li<%if(!genderRequiredFieldValidator.IsValid){ %> class="highlight"<%} %>>
                    <span class="label">Gender<%if(Mode == UserFormMode.Create){%>#s#<span class="required">*</span><%}%></span>
                    <div class="horzRadios">
                         <asp:RadioButtonList runat="server" ID="gender" RepeatLayout="Flow">
                             <asp:ListItem Value="1">Male</asp:ListItem>
                             <asp:ListItem Value="2">Female</asp:ListItem>
                         </asp:RadioButtonList>
                    </div>
                    <asp:RequiredFieldValidator runat="server" ID="genderRequiredFieldValidator" ControlToValidate="gender" Display="Dynamic" EnableClientScript="false" CssClass="error">
                    Please select your gender.
                    </asp:RequiredFieldValidator>
                </li>
                        
                <li<%if(!BirthDateCustomValidator.IsValid){ %> class="highlight"<%} %>>
                    <asp:label ID="Label7" AssociatedControlID="birthMonth" runat="server">Birth Date<%if(Mode == UserFormMode.Create){%>#s#<span class="required">*</span><%}%></asp:label>
                    <div class="horzSelects">
                        <asp:dropdownlist runat="server" id="birthMonth" EnableViewState="true" DataTextField="Value" DataValueField="Key" Style="width:90px" />
                        <asp:dropdownlist runat="server" id="birthDay" EnableViewState="true" Style="width:55px" />
                        <asp:dropdownlist runat="server" id="birthYear" EnableViewState="true" Style="width:60px" />
                    </div>
                    
                    <asp:customvalidator runat="server" ID="BirthDateCustomValidator" OnServerValidate="ValidateBirthDate" Display="Dynamic" EnableClientScript="false" CssClass="error">
                    Please select a value for all  three fields of your birth date.
                    </asp:customvalidator>                      
                </li>
                                             
                <li<%if(!LocationCustomValidator.IsValid){ %> class="highlight"<%} %>>
                    <asp:label ID="Label8" AssociatedControlID="location" runat="server">Location<%if(Mode == UserFormMode.Create){%>#s#<span class="required">*</span><%}%></asp:label>
                    <asp:dropdownlist runat="server" id="location" DataTextField="Value" DataValueField="Key" />
                    
                    <asp:customvalidator runat="server" ID="LocationCustomValidator" ControlToValidate="location" OnServerValidate="ValidateLocation" Display="Dynamic" EnableClientScript="false" CssClass="error">
                        Please select your location.
                    </asp:customvalidator>
                </li>
            </ul>
        </fieldset>
        
        <%if (Mode == UserFormMode.Create)
          { %>
          <%-- 
        <fieldset>
            <legend>Verification</legend>
            <ul class="inputContainer">
                <li class="verification<%if(!CaptchaResponseCustomValidator.IsValid){ %> highlight<%} %>">
                    <div style="display:block; clear:both; margin-bottom:1em">
                    <cc1:captchacontrol runat="server" ID="captcha" CacheStrategy="HttpRuntime" ToolTip="CAPTCHA"></cc1:captchacontrol>
                    </div>
                    <br class="cb" />
                    <label for="<%=captcha.UniqueID%>">Characters In Picture <span class="required">*</span></label>
                    <input id="<%=captcha.UniqueID%>" name="<%=captcha.UniqueID%>" type="text" />
                    <asp:customvalidator runat="server" ID="CaptchaResponseCustomValidator" OnServerValidate="ValidateCaptcha" Display="Dynamic" EnableClientScript="false" CssClass="error">Please enter the text contained in the image above.</asp:customvalidator>                                
                </li>
            </ul>              
        </fieldset>
        --%>
        <%}
          else
          { %>
        <fieldset>
            <legend>Personal Settings</legend>
            <ul class="inputContainer">
                <li class="checkbox">
                    <asp:CheckBox runat="server" ID="favoritesPublic" Text="Allow my favorites to be viewed by other users" />
                </li>
                
                <li class="checkbox">
                    <asp:CheckBox  runat="server" ID="historyPublic" Text="Allow my viewing history to be viewed by other users" />
                </li>
                
                <li class="checkbox">
                    <asp:CheckBox  runat="server" ID="ratedPublic" Text="Allow my rating and comment history to be viewed by other users" />
                </li>                
            </ul>              
        </fieldset>          
        <%} %>
        
        
        <div>
            <asp:HiddenField runat="server" ID="returnUrl" />
            <input type="submit" value="<%=(Mode == UserFormMode.Create ? "Register" : "Save Changes")%>" />
        </div>
</form>