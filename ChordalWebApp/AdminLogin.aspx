<%@ Page Title="Admin Login" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="AdminLogin.aspx.cs" Inherits="ChordalWebApp.AdminLogin" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("Styles/auth-styles.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="auth-page">
        <div class="auth-container">
            <!-- Left Panel - Admin Visual with Wave Animation -->
            <div class="auth-visual-panel admin">
                <div id="auth-wave-canvas"></div>
                <div class="auth-visual-content">
                    <!-- Logo and Tagline -->
                    <div class="auth-logo">
                        <div class="auth-logo-icon admin">C</div>
                        <div class="auth-logo-text">
                            <h1 class="auth-brand">Chordal Admin</h1>
                            <p class="auth-tagline">Administrative Access</p>
                        </div>
                    </div>

                    <!-- Visual Message -->
                    <div class="auth-visual-text">
                        <h2 class="auth-visual-title">Admin Portal</h2>
                        <p class="auth-visual-subtitle">
                            Secure administrative access with two-factor authentication.
                            Manage users, content moderation, and system configuration.
                        </p>
                    </div>
                </div>
            </div>

            <!-- Right Panel - Admin Login Form -->
            <div class="auth-form-panel">
                <!-- Step 1: Credentials Panel -->
                <asp:Panel ID="pnlCredentials" runat="server">
                    <div class="auth-form-header">
                        <span class="admin-badge-header">ADMIN ACCESS</span>
                        <h2 class="auth-form-title">Administrator Login</h2>
                        <p class="auth-form-subtitle">Enter your admin credentials</p>
                    </div>

                    <!-- Status Message -->
                    <asp:Label ID="lblAdminLoginStatus" runat="server" EnableViewState="false" 
                        CssClass="auth-status-message" style="display:block; margin-bottom: 1.5rem;"></asp:Label>

                    <!-- Admin Login Form -->
                    <div class="auth-form-group">
                        <label class="auth-form-label" for="txtAdminUsername">Admin Username</label>
                        <div class="auth-input-wrapper">
                            <asp:TextBox 
                                ID="txtAdminUsername" 
                                runat="server" 
                                CssClass="auth-form-input" 
                                placeholder="Enter admin username"
                                autocomplete="username" />
                        </div>
                        <asp:RequiredFieldValidator 
                            ID="rfvAdminUsername" 
                            runat="server" 
                            ControlToValidate="txtAdminUsername"
                            ErrorMessage="Admin username is required" 
                            CssClass="auth-validation" 
                            Display="Dynamic"
                            ValidationGroup="Login" />
                    </div>

                    <div class="auth-form-group">
                        <label class="auth-form-label" for="txtAdminPassword">Admin Password</label>
                        <div class="auth-input-wrapper">
                            <asp:TextBox 
                                ID="txtAdminPassword" 
                                runat="server" 
                                CssClass="auth-form-input" 
                                TextMode="Password" 
                                placeholder="Enter admin password"
                                autocomplete="current-password" />
                            <span class="password-toggle">👁️</span>
                        </div>
                        <asp:RequiredFieldValidator 
                            ID="rfvAdminPassword" 
                            runat="server" 
                            ControlToValidate="txtAdminPassword"
                            ErrorMessage="Admin password is required" 
                            CssClass="auth-validation" 
                            Display="Dynamic"
                            ValidationGroup="Login" />
                    </div>

                    <!-- Security Notice -->
                    <div class="auth-requirements" style="border-left-color: #dc3545;">
                        <div class="auth-requirements-title" style="color: #dc3545;">Security Notice</div>
                        <ul class="auth-requirements-list">
                            <li>Two-factor authentication required</li>
                            <li>All login attempts are logged and monitored</li>
                            <li>Account lockout after 5 failed attempts</li>
                        </ul>
                    </div>

                    <!-- Submit Button -->
                    <asp:Button 
                        ID="btnAdminLogin" 
                        runat="server" 
                        Text="Continue to 2FA" 
                        OnClick="btnAdminLogin_Click" 
                        CssClass="auth-submit-btn"
                        style="background: #dc3545;" 
                        ValidationGroup="Login" />

                    <!-- Footer Links -->
                    <div class="auth-footer">
                        <p class="auth-footer-text">
                            Not an admin? 
                            <asp:HyperLink 
                                ID="hlBackToUserLogin" 
                                runat="server" 
                                NavigateUrl="~/Login.aspx" 
                                CssClass="auth-footer-link">
                                User login
                            </asp:HyperLink>
                        </p>
                    </div>
                </asp:Panel>

                <!-- Step 2: Two-Factor Authentication Panel -->
                <asp:Panel ID="pnl2FA" runat="server" Visible="false">
                    <div class="auth-form-header">
                        <span class="admin-badge-header">🔐 TWO-FACTOR AUTH</span>
                        <h2 class="auth-form-title">Verify Your Identity</h2>
                        <p class="auth-form-subtitle">Enter the 6-digit code sent to your email</p>
                    </div>

                    <!-- Email Notice -->
                    <div class="auth-status-message auth-status-success" style="margin-bottom: 1.5rem;">
                        <strong>Check Your Email</strong><br />
                        A verification code has been sent to your registered email address.
                    </div>

                    <!-- 2FA Code Input -->
                    <div class="auth-form-group">
                        <label class="auth-form-label" for="txt2FACode">Verification Code</label>
                        <div class="auth-input-wrapper">
                            <asp:TextBox 
                                ID="txt2FACode" 
                                runat="server" 
                                CssClass="auth-form-input" 
                                MaxLength="6"
                                placeholder="000000"
                                style="text-align: center; font-size: 1.5rem; letter-spacing: 0.5rem; font-weight: 600;"
                                autocomplete="off" />
                        </div>
                        <asp:RequiredFieldValidator 
                            ID="rfv2FACode" 
                            runat="server" 
                            ControlToValidate="txt2FACode"
                            ErrorMessage="Verification code is required" 
                            CssClass="auth-validation" 
                            Display="Dynamic"
                            ValidationGroup="TwoFactor" />
                        <asp:RegularExpressionValidator 
                            ID="rev2FACode" 
                            runat="server" 
                            ControlToValidate="txt2FACode"
                            ValidationExpression="^\d{6}$" 
                            ErrorMessage="Code must be 6 digits" 
                            CssClass="auth-validation" 
                            Display="Dynamic"
                            ValidationGroup="TwoFactor" />
                    </div>

                    <!-- Expiration Timer -->
                    <div style="text-align: center; padding: 0.75rem; background: #fff3cd; border: 1px solid #ffc107; border-radius: 0.375rem; margin-bottom: 1.5rem;">
                        <span style="font-size: 0.875rem; color: #856404;">
                            Code expires in: <asp:Label ID="lblExpiration" runat="server" style="font-weight: 700; color: #dc3545;"></asp:Label>
                        </span>
                    </div>

                    <!-- Verify Button -->
                    <asp:Button 
                        ID="btnVerify2FA" 
                        runat="server" 
                        Text="Verify & Login" 
                        OnClick="btnVerify2FA_Click" 
                        CssClass="auth-submit-btn"
                        style="background: #28a745; margin-bottom: 0.5rem;" 
                        ValidationGroup="TwoFactor" />

                    <!-- Resend Code Button -->
                    <asp:Button 
                        ID="btnResendCode" 
                        runat="server" 
                        Text="Resend Code" 
                        OnClick="btnResendCode_Click" 
                        CssClass="auth-submit-btn"
                        style="background: #6c757d;" 
                        CausesValidation="false" />

                    <!-- Back Link -->
                    <div class="auth-footer">
                        <p class="auth-footer-text">
                            <asp:LinkButton 
                                ID="lnkBackToLogin" 
                                runat="server" 
                                OnClick="lnkBackToLogin_Click" 
                                CssClass="auth-footer-link"
                                CausesValidation="false">
                                ← Back to login
                            </asp:LinkButton>
                        </p>
                    </div>
                </asp:Panel>
            </div>
        </div>
    </div>

    <!-- Success Wave Overlay -->
    <div id="auth-success-wave"></div>

    <script src="<%= ResolveUrl("Scripts/auth-animations.js") %>"></script>
    
    <!-- Auto-focus 2FA code input when visible -->
    <script type="text/javascript">
        window.addEventListener('load', function() {
            var codeInput = document.getElementById('<%= txt2FACode.ClientID %>');
            if (codeInput && codeInput.offsetParent !== null) {
                setTimeout(function() {
                    codeInput.focus();
                }, 500);
            }
        });
    </script>
</asp:Content>
