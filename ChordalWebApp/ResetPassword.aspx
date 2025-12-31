<%@ Page Title="Reset Password" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="ChordalWebApp.ResetPassword" %>

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
                        <h2 class="auth-visual-title">Reset Password</h2>
                        <p class="auth-visual-subtitle">
                            Choose a new, secure password to regain access to your account 
                            and continue your musical journey.
                        </p>
                    </div>
                </div>
            </div>

            <!-- Right Panel - Reset Password Form -->
            <div class="auth-form-panel">
                <div class="auth-form-header">
                    <h2 class="auth-form-title">Create New Password</h2>
                    <p class="auth-form-subtitle">Enter your new password below</p>
                </div>

                <!-- Status Message -->
                <asp:Panel ID="pnlStatusMessage" runat="server" Visible="false" CssClass="auth-status-message">
                    <asp:Label ID="lblStatus" runat="server" />
                </asp:Panel>

                <!-- Reset Form -->
                <asp:Panel ID="pnlResetForm" runat="server">
                    <div class="auth-form-group">
                        <label class="auth-form-label" for="txtNewPassword">New Password</label>
                        <div class="auth-input-wrapper">
                            <asp:TextBox 
                                ID="txtNewPassword" 
                                runat="server" 
                                CssClass="auth-form-input" 
                                TextMode="Password" 
                                placeholder="Enter new password"
                                autocomplete="new-password" />
                            <span class="password-toggle">👁️</span>
                        </div>
                        <asp:RequiredFieldValidator 
                            ID="rfvNewPassword" 
                            runat="server" 
                            ControlToValidate="txtNewPassword"
                            ErrorMessage="Password is required" 
                            CssClass="auth-validation" 
                            Display="Dynamic" />
                        <asp:RegularExpressionValidator 
                            ID="revPassword" 
                            runat="server" 
                            ControlToValidate="txtNewPassword"
                            ErrorMessage="Password must be 8-32 characters" 
                            ValidationExpression=".{8,32}"
                            CssClass="auth-validation" 
                            Display="Dynamic" />
                    </div>

                    <div class="auth-form-group">
                        <label class="auth-form-label" for="txtConfirmPassword">Confirm New Password</label>
                        <div class="auth-input-wrapper">
                            <asp:TextBox 
                                ID="txtConfirmPassword" 
                                runat="server" 
                                CssClass="auth-form-input" 
                                TextMode="Password" 
                                placeholder="Confirm new password"
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
                            ID="cvPasswords" 
                            runat="server" 
                            ControlToValidate="txtConfirmPassword" 
                            ControlToCompare="txtNewPassword"
                            ErrorMessage="Passwords do not match" 
                            CssClass="auth-validation" 
                            Display="Dynamic" />
                    </div>

                    <!-- Password Requirements -->
                    <div class="auth-requirements">
                        <div class="auth-requirements-title">Password Requirements:</div>
                        <ul class="auth-requirements-list">
                            <li>Between 8-32 characters</li>
                            <li>Mix of letters, numbers, and symbols recommended</li>
                            <li>Avoid using common words or patterns</li>
                            <li>Different from your previous password</li>
                        </ul>
                    </div>

                    <!-- Submit Button -->
                    <asp:Button 
                        ID="btnResetPassword" 
                        runat="server" 
                        Text="Reset Password" 
                        OnClick="btnResetPassword_Click" 
                        CssClass="auth-submit-btn" />
                </asp:Panel>

                <!-- Footer Links -->
                <div class="auth-footer">
                    <p class="auth-footer-text">
                        Remember your password? 
                        <asp:HyperLink 
                            ID="hlBackToLogin" 
                            runat="server" 
                            NavigateUrl="~/Login.aspx" 
                            CssClass="auth-footer-link">
                            Back to login
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
