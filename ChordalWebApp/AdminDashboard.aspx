<%@ Page Title="Admin Dashboard" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="ChordalWebApp.AdminDashboard" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Admin Dashboard - Chordal
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Styles/admin-styles.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="admin-page">
        <!-- Admin Header with Particles -->
        <div class="admin-header">
            <div id="admin-particles-canvas"></div>
            <div class="admin-header-content">
                <div class="admin-header-top">
                    <div>
                        <h1 class="admin-title">
 
                            Admin Dashboard
                        </h1>
                        <p class="admin-welcome">Welcome back, <asp:Label ID="lblAdminName" runat="server"></asp:Label></p>
                    </div>
                    <div class="admin-header-actions">
                        <asp:Button ID="btnLogout" runat="server" Text="Logout" OnClick="btnLogout_Click" CssClass="btn-admin-header btn-admin-logout" />
                    </div>
                </div>
            </div>
        </div>

        <div class="admin-container">
            <!-- Statistics Overview -->
            <div class="admin-stats-grid">
                <div class="stat-card alert">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">
                            
                        </div>
                    </div>
                    <div class="stat-label">Pending Reports</div>
                    <div class="stat-number">
                        <asp:Label ID="lblPendingReports" runat="server" Text="0"></asp:Label>
                    </div>
                    <div class="stat-change negative">
                        <span>Requires attention</span>
                    </div>
                </div>
                
                <div class="stat-card warning">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">
                            
                        </div>
                    </div>
                    <div class="stat-label">Under Review</div>
                    <div class="stat-number">
                        <asp:Label ID="lblUnderReview" runat="server" Text="0"></asp:Label>
                    </div>
                    <div class="stat-change">
                        <span>In progress</span>
                    </div>
                </div>
                
                <div class="stat-card success">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">
                            
                        </div>
                    </div>
                    <div class="stat-label">Resolved Reports</div>
                    <div class="stat-number">
                        <asp:Label ID="lblResolvedReports" runat="server" Text="0"></asp:Label>
                    </div>
                    <div class="stat-change positive">
                        <span>↑ +12% this week</span>
                    </div>
                </div>
                
                <div class="stat-card info">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">
                            
                        </div>
                    </div>
                    <div class="stat-label">Removed Content</div>
                    <div class="stat-number">
                        <asp:Label ID="lblRemovedContent" runat="server" Text="0"></asp:Label>
                    </div>
                    <div class="stat-change">
                        <span>Total actions</span>
                    </div>
                </div>
            </div>

            <!-- Quick Actions -->
            <div class="admin-quick-actions">
                <h2 class="section-title-admin">
                    <span></span>
                    Quick Actions
                </h2>
                <div class="actions-grid">
                    <asp:HyperLink ID="hlManageModeration" runat="server" NavigateUrl="~/ManageModeration.aspx" CssClass="action-card danger">
                        <div class="action-card-icon"></div>
                        <div class="action-card-content">
                            <div class="action-card-title">Manage Moderation</div>
                            <div class="action-card-description">Review reports & content</div>
                        </div>
                    </asp:HyperLink>
                    
                    <asp:HyperLink ID="hlManagePermissions" runat="server" NavigateUrl="~/ManagePermissions.aspx" CssClass="action-card warning">
                        <div class="action-card-icon"></div>
                        <div class="action-card-content">
                            <div class="action-card-title">Manage Permissions</div>
                            <div class="action-card-description">User roles & access</div>
                        </div>
                    </asp:HyperLink>
                    
                    <asp:HyperLink ID="hlViewAnalytics" runat="server" NavigateUrl="~/ViewAnalytics.aspx" CssClass="action-card success">
                        <div class="action-card-icon"></div>
                        <div class="action-card-content">
                            <div class="action-card-title">View Analytics</div>
                            <div class="action-card-description">Platform statistics</div>
                        </div>
                    </asp:HyperLink>
                    
                    <asp:HyperLink ID="hlManageGuidelines" runat="server" NavigateUrl="~/ManageGuidelines.aspx" CssClass="action-card info">
                        <div class="action-card-icon"></div>
                        <div class="action-card-content">
                            <div class="action-card-title">Manage Guidelines</div>
                            <div class="action-card-description">Edit community rules</div>
                        </div>
                    </asp:HyperLink>
                    
                    <asp:HyperLink ID="hlViewUsers" runat="server" NavigateUrl="~/ManagePermissions.aspx" CssClass="action-card info">
                        <div class="action-card-icon"></div>
                        <div class="action-card-content">
                            <div class="action-card-title">View Users</div>
                            <div class="action-card-description">Browse user accounts</div>
                        </div>
                    </asp:HyperLink>
                </div>
            </div>

            <!-- System Overview -->
            <div class="admin-content-card">
                <div class="admin-card-header">
                    <h3 class="admin-card-title">System Overview</h3>
                </div>
                <div class="admin-data-list">
                    <div class="data-item">
                        <div class="data-item-header">
                            <span class="data-item-title">Last Login</span>
                        </div>
                        <div class="data-item-meta">
                            <asp:Label ID="lblLastLogin" runat="server"></asp:Label>
                        </div>
                    </div>
                    <div class="data-item">
                        <div class="data-item-header">
                            <span class="data-item-title">System Status</span>
                            <span class="admin-badge badge-active">● Operational</span>
                        </div>
                        <div class="data-item-meta">
                            All systems running normally
                        </div>
                    </div>
                    <div class="data-item">
                        <div class="data-item-header">
                            <span class="data-item-title">Admin Role</span>
                        </div>
                        <div class="data-item-meta">
                            System Administrator
                        </div>
                    </div>
                    <div class="data-item">
                        <div class="data-item-header">
                            <span class="data-item-title">Active Sessions</span>
                        </div>
                        <div class="data-item-meta">
                            1 active admin session
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Admin Animation Scripts -->
    <script src="<%= ResolveUrl("~/Scripts/animations-admin.js") %>"></script>
</asp:Content>
