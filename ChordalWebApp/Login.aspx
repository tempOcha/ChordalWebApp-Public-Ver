<%@ Page Title="Login" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="ChordalWebApp.Login" %>

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
                        <h2 class="auth-visual-title">Welcome Back</h2>
                        <p class="auth-visual-subtitle">
                            Continue your musical journey. Learn chord progressions, 
                            master harmony, and create beautiful music.
                        </p>
                    </div>
                </div>
            </div>

            <!-- Right Panel - Login Form -->
            <div class="auth-form-panel">
                <div class="auth-form-header">
                    <h2 class="auth-form-title">Sign In</h2>
                    <p class="auth-form-subtitle">Enter your credentials to access your account</p>
                </div>

                <!-- Status Message -->
                <asp:Panel ID="pnlStatus" runat="server" Visible="false" CssClass="auth-status-message">
                    <asp:Label ID="lblLoginStatus" runat="server" />
                </asp:Panel>

                <!-- Login Form -->
                <div class="auth-form-group">
                    <label class="auth-form-label" for="txtUsernameLogin">Username or Email</label>
                    <div class="auth-input-wrapper">
                        <asp:TextBox 
                            ID="txtUsernameLogin" 
                            runat="server" 
                            CssClass="auth-form-input" 
                            placeholder="Enter your username or email"
                            autocomplete="username" />
                    </div>
                    <asp:RequiredFieldValidator 
                        ID="rfvUsernameLogin" 
                        runat="server" 
                        ControlToValidate="txtUsernameLogin"
                        ErrorMessage="Username or email is required" 
                        CssClass="auth-validation" 
                        Display="Dynamic" />
                </div>

                <div class="auth-form-group">
                    <label class="auth-form-label" for="txtPasswordLogin">Password</label>
                    <div class="auth-input-wrapper">
                        <asp:TextBox 
                            ID="txtPasswordLogin" 
                            runat="server" 
                            CssClass="auth-form-input" 
                            TextMode="Password" 
                            placeholder="Enter your password"
                            autocomplete="current-password" />
                        <span class="password-toggle">👁️</span>
                    </div>
                    <asp:RequiredFieldValidator 
                        ID="rfvPasswordLogin" 
                        runat="server" 
                        ControlToValidate="txtPasswordLogin"
                        ErrorMessage="Password is required" 
                        CssClass="auth-validation" 
                        Display="Dynamic" />
                </div>

                <!-- Remember Me & Forgot Password -->
                <div class="auth-form-helper">
                    <div class="auth-remember">
                        <asp:CheckBox 
                            ID="chkRememberMe" 
                            runat="server" />
                        <label for="<%= chkRememberMe.ClientID %>">Remember me</label>
                    </div>
                    <asp:HyperLink 
                        ID="hlForgotPassword" 
                        runat="server" 
                        NavigateUrl="~/ForgotPassword.aspx" 
                        CssClass="auth-forgot-link">
                        Forgot password?
                    </asp:HyperLink>
                </div>

                <!-- Submit Button -->
                <asp:Button 
                    ID="btnLogin" 
                    runat="server" 
                    Text="Sign In" 
                    OnClick="btnLogin_Click" 
                    CssClass="auth-submit-btn" />

                <!-- Footer Links -->
                <div class="auth-footer">
                    <p class="auth-footer-text">
                        Don't have an account? 
                        <asp:HyperLink 
                            ID="hlSignUp" 
                            runat="server" 
                            NavigateUrl="~/Register.aspx" 
                            CssClass="auth-footer-link">
                            Sign up
                        </asp:HyperLink>
                    </p>
                    <p class="auth-footer-text" style="margin-top: 0.5rem;">
                        <asp:HyperLink 
                            ID="hlAdminLogin" 
                            runat="server" 
                            NavigateUrl="~/AdminLogin.aspx" 
                            CssClass="auth-footer-link"
                            style="color: #dc3545;">
                            Admin Login
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
