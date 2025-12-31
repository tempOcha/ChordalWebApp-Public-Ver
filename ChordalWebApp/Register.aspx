<%@ Page Title="Register" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="ChordalWebApp.Register" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("Styles/auth-styles.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="auth-page">
        <div class="auth-container">
            <!-- Left Panel - Visual with Wave Animation -->
            <div class="auth-visual-panel">
                <div id="auth-wave-canvas"></div>
                <div class="auth-visual-content">
                    <!-- Logo and Tagline -->
                    <div class="auth-logo">
                        <div class="auth-logo-icon">C</div>
                        <div class="auth-logo-text">
                            <h1 class="auth-brand">Chordal</h1>
                            <p class="auth-tagline">Harmony rethought.</p>
                        </div>
                    </div>

                    <!-- Visual Message -->
                    <div class="auth-visual-text">
                        <h2 class="auth-visual-title">Start Your Journey</h2>
                        <p class="auth-visual-subtitle">
                            Join thousands of musicians learning chord progressions 
                            and mastering harmony through interactive lessons.
                        </p>
                    </div>
                </div>
            </div>

            <!-- Right Panel - Registration Form -->
            <div class="auth-form-panel">
                <div class="auth-form-header">
                    <h2 class="auth-form-title">Create Account</h2>
                    <p class="auth-form-subtitle">Get started with your free account</p>
                </div>

                <!-- Status Message -->
                <asp:Panel ID="pnlStatus" runat="server" Visible="false" CssClass="auth-status-message">
                    <asp:Label ID="lblStatus" runat="server" />
                </asp:Panel>

                <!-- Registration Form -->
                <div class="auth-form-group">
                    <label class="auth-form-label" for="txtUsername">Username</label>
                    <div class="auth-input-wrapper">
                        <asp:TextBox 
                            ID="txtUsername" 
                            runat="server" 
                            CssClass="auth-form-input" 
                            placeholder="Choose a username"
                            autocomplete="username" />
                    </div>
                    <asp:RequiredFieldValidator 
                        ID="rfvUsername" 
                        runat="server" 
                        ControlToValidate="txtUsername"
                        ErrorMessage="Username is required" 
                        CssClass="auth-validation" 
                        Display="Dynamic" />
                </div>

                <div class="auth-form-group">
                    <label class="auth-form-label" for="txtEmail">Email Address</label>
                    <div class="auth-input-wrapper">
                        <asp:TextBox 
                            ID="txtEmail" 
                            runat="server" 
                            CssClass="auth-form-input" 
                            TextMode="Email" 
                            placeholder="Enter your email"
                            autocomplete="email" />
                    </div>
                    <asp:RequiredFieldValidator 
                        ID="rfvEmail" 
                        runat="server" 
                        ControlToValidate="txtEmail"
                        ErrorMessage="Email is required" 
                        CssClass="auth-validation" 
                        Display="Dynamic" />
                    <asp:RegularExpressionValidator 
                        ID="revEmail" 
                        runat="server" 
                        ControlToValidate="txtEmail"
                        ErrorMessage="Please enter a valid email address" 
                        ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                        CssClass="auth-validation" 
                        Display="Dynamic" />
                </div>

                <div class="auth-form-group">
                    <label class="auth-form-label" for="txtPassword">Password</label>
                    <div class="auth-input-wrapper">
                        <asp:TextBox 
                            ID="txtPassword" 
                            runat="server" 
                            CssClass="auth-form-input" 
                            TextMode="Password" 
                            placeholder="Create a strong password"
                            autocomplete="new-password" />
                        <span class="password-toggle">👁️</span>
                    </div>
                    <asp:RequiredFieldValidator 
                        ID="rfvPassword" 
                        runat="server" 
                        ControlToValidate="txtPassword"
                        ErrorMessage="Password is required" 
                        CssClass="auth-validation" 
                        Display="Dynamic" />
                    <asp:RegularExpressionValidator 
                        ID="revPassword" 
                        runat="server" 
                        ControlToValidate="txtPassword"
                        ErrorMessage="Password must be at least 8 characters" 
                        ValidationExpression=".{8,}"
                        CssClass="auth-validation" 
                        Display="Dynamic" />
                </div>

                <div class="auth-form-group">
                    <label class="auth-form-label" for="txtConfirmPassword">Confirm Password</label>
                    <div class="auth-input-wrapper">
                        <asp:TextBox 
                            ID="txtConfirmPassword" 
                            runat="server" 
                            CssClass="auth-form-input" 
                            TextMode="Password" 
                            placeholder="Re-enter your password"
                            autocomplete="new-password" />
                        <span class="password-toggle">👁️</span>
                    </div>
                    <asp:RequiredFieldValidator 
                        ID="rfvConfirmPassword" 
                        runat="server" 
                        ControlToValidate="txtConfirmPassword"
                        ErrorMessage="Please confirm your password" 
                        CssClass="auth-validation" 
                        Display="Dynamic" />
                    <asp:CompareValidator 
                        ID="cvPassword" 
                        runat="server" 
                        ControlToValidate="txtConfirmPassword" 
                        ControlToCompare="txtPassword"
                        ErrorMessage="Passwords do not match" 
                        CssClass="auth-validation" 
                        Display="Dynamic" />
                </div>

                <!-- Password Requirements -->
                <div class="auth-requirements">
                    <div class="auth-requirements-title">Password Requirements:</div>
                    <ul class="auth-requirements-list">
                        <li>Minimum 8 characters</li>
                        <li>Mix of letters, numbers recommended</li>
                        <li>Avoid common words or patterns</li>
                    </ul>
                </div>

                <!-- Submit Button -->
                <asp:Button 
                    ID="btnRegister" 
                    runat="server" 
                    Text="Create Account" 
                    OnClick="btnRegister_Click" 
                    CssClass="auth-submit-btn" />

                <!-- Footer Links -->
                <div class="auth-footer">
                    <p class="auth-footer-text">
                        Already have an account? 
                        <asp:HyperLink 
                            ID="hlSignIn" 
                            runat="server" 
                            NavigateUrl="~/Login.aspx" 
                            CssClass="auth-footer-link">
                            Sign in
                        </asp:HyperLink>
                    </p>
                </div>
            </div>
        </div>
    </div>

    <!-- Success Wave Overlay -->
    <div id="auth-success-wave"></div>

    <script src="<%= ResolveUrl("Scripts/auth-animations.js") %>"></script>
</asp:Content>
