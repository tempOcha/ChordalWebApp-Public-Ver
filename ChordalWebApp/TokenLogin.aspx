<%@ Page Title="VST Plugin Login" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="TokenLogin.aspx.cs" Inherits="ChordalWebApp.TokenLogin" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("Styles/auth-styles.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="auth-page">
        <div class="auth-container">
            <!-- Left Panel - VST Visual with Wave Animation -->
            <div class="auth-visual-panel" style="background: linear-gradient(135deg, #5588dd 0%, #77aaff 100%);">
                <div id="auth-wave-canvas"></div>
                <div class="auth-visual-content">
                    <!-- Logo and Tagline -->
                    <div class="auth-logo">
                        <div class="auth-logo-icon" style="background: white; color: #5588dd; font-size: 2rem;">
                            C
                        </div>
                        <div class="auth-logo-text">
                            <h1 class="auth-brand">Chordal VST</h1>
                            <p class="auth-tagline">Plugin Authentication</p>
                        </div>
                    </div>

                    <!-- Visual Message -->
                    <div class="auth-visual-text">
                        <h2 class="auth-visual-title">VST Plugin Login</h2>
                        <p class="auth-visual-subtitle">
                            Seamlessly connect your Chordal VST plugin to your online account.
                            Access your progressions and settings anywhere.
                        </p>
                    </div>
                </div>
            </div>

            <!-- Right Panel - Token Authentication Status -->
            <div class="auth-form-panel">
                <!-- Loading Panel -->
                <asp:Panel ID="pnlLoading" runat="server" Visible="true">
                    <div class="auth-form-header">
                        <h2 class="auth-form-title">Authenticating...</h2>
                        <p class="auth-form-subtitle">Please wait while we verify your VST plugin token</p>
                    </div>

                    <!-- Loading Spinner -->
                    <div style="text-align: center; padding: 3rem 0;">
                        <div style="
                            border: 4px solid #f3f3f3;
                            border-top: 4px solid var(--color-teal);
                            border-radius: 50%;
                            width: 60px;
                            height: 60px;
                            animation: spin 1s linear infinite;
                            margin: 0 auto 1.5rem;
                        "></div>
                        <p style="color: var(--color-text-muted);">Connecting to your account...</p>
                    </div>

                    <style>
                        @keyframes spin {
                            0% { transform: rotate(0deg); }
                            100% { transform: rotate(360deg); }
                        }
                    </style>
                </asp:Panel>

                <!-- Status Panel (Success or Error) -->
                <asp:Panel ID="pnlStatus" runat="server" Visible="false">
                    <div class="auth-form-header">
                        <h2 class="auth-form-title" id="statusTitle" runat="server">Authentication Status</h2>
                    </div>

                    <!-- Status Message -->
                    <div id="statusMessage" runat="server" class="auth-status-message" style="margin-bottom: 1.5rem;">
                    </div>

                    <!-- Redirect Info (Success) -->
                    <asp:Panel ID="pnlRedirectInfo" runat="server" Visible="false">
                        <div style="text-align: center; padding: 1.5rem; background: var(--color-soft-green); border: 2px solid var(--color-teal); border-radius: var(--radius-md); margin-bottom: 1.5rem;">
                            <p style="margin: 0 0 0.5rem 0; color: var(--color-text-secondary); font-weight: 600;">
                                Redirecting to your dashboard...
                            </p>
                            <p style="margin: 0; font-size: 2rem; font-weight: 700; color: var(--color-teal);">
                                <span id="countdown">5</span>s
                            </p>
                        </div>

                        <asp:HyperLink 
                            ID="lnkManualRedirect" 
                            runat="server" 
                            NavigateUrl="~/LandingPage.aspx" 
                            CssClass="auth-submit-btn"
                            style="text-align: center; display: block;">
                            Go to Dashboard Now →
                        </asp:HyperLink>
                    </asp:Panel>

                    <!-- Login Prompt (Error) -->
                    <asp:Panel ID="pnlLoginPrompt" runat="server" Visible="false">
                        <div class="auth-footer">
                            <p class="auth-footer-text">
                                Please 
                                <asp:HyperLink 
                                    ID="lnkLogin" 
                                    runat="server" 
                                    NavigateUrl="~/Login.aspx"
                                    CssClass="auth-footer-link">
                                    log in
                                </asp:HyperLink>
                                to continue using the VST plugin.
                            </p>
                        </div>
                    </asp:Panel>
                </asp:Panel>

                <!-- Info Box -->
                <div class="auth-requirements" style="margin-top: 1.5rem;">
                    <div class="auth-requirements-title">Secure VST Integration</div>
                    <ul class="auth-requirements-list">
                        <li>Token-based authentication from your VST plugin</li>
                        <li>Automatic login without password entry</li>
                        <li>Tokens expire after 5 minutes for security</li>
                        <li>Email notification sent on successful login</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>

    <!-- Success Wave Overlay -->
    <div id="auth-success-wave"></div>

    <script src="<%= ResolveUrl("Scripts/auth-animations.js") %>"></script>

    <!-- Countdown Timer Script -->
    <script type="text/javascript">
        function startCountdown() {
            var seconds = 5;
            var countdownElement = document.getElementById('countdown');

            var interval = setInterval(function () {
                seconds--;
                if (countdownElement) {
                    countdownElement.textContent = seconds;
                }

                if (seconds <= 0) {
                    clearInterval(interval);

                    // Trigger wave animation before redirect
                    if (typeof handleAuthSuccess !== 'undefined') {
                        handleAuthSuccess('<%= ResolveUrl("~/LandingPage.aspx") %>', 1000);
                    } else {
                        window.location.href = '<%= ResolveUrl("~/LandingPage.aspx") %>';
                    }
                }
            }, 1000);
        }

        // Check if we should start countdown
        window.onload = function() {
            var shouldRedirect = <%= ShouldAutoRedirect.ToString().ToLower() %>;
            if (shouldRedirect) {
                startCountdown();
            }
        };
    </script>
</asp:Content>
