<%@ Page Title="Manage Moderation" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="ManageModeration.aspx.cs" Inherits="ChordalWebApp.ManageModeration" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Manage Moderation - Chordal Admin
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
                            <span class="admin-title-icon"></span>
                            Content Moderation Queue
                        </h1>
                        <p class="admin-subtitle">Review and manage reported content</p>
                    </div>
                    <div class="admin-header-actions">
                        <asp:HyperLink ID="hlBackToDashboard" runat="server" NavigateUrl="~/AdminDashboard.aspx" CssClass="btn-admin-header">
                             Back to Dashboard
                        </asp:HyperLink>
                    </div>
                </div>
            </div>
        </div>

        <div class="admin-container">
            <!-- Statistics Overview -->
            <div class="admin-stats-grid">
                <div class="stat-card warning">
                    <div class="stat-card-header">
                        <div class="stat-card-icon"></div>
                    </div>
                    <div class="stat-label">Pending Reports</div>
                    <div class="stat-number">
                        <asp:Label ID="lblPendingCount" runat="server" Text="0"></asp:Label>
                    </div>
                    <div class="stat-change">Awaiting review</div>
                </div>
                
                <div class="stat-card success">
                    <div class="stat-card-header">
                        <div class="stat-card-icon"></div>
                    </div>
                    <div class="stat-label">Resolved</div>
                    <div class="stat-number">
                        <asp:Label ID="lblResolvedCount" runat="server" Text="0"></asp:Label>
                    </div>
                    <div class="stat-change positive">Completed actions</div>
                </div>
                
                <div class="stat-card info">
                    <div class="stat-card-header">
                        <div class="stat-card-icon"></div>
                    </div>
                    <div class="stat-label">Dismissed</div>
                    <div class="stat-number">
                        <asp:Label ID="lblDismissedCount" runat="server" Text="0"></asp:Label>
                    </div>
                    <div class="stat-change">No action needed</div>
                </div>
            </div>

            <!-- Filters Section -->
            <div class="admin-filters">
                <div class="filters-row">
                    <div class="filter-group">
                        <label class="filter-label">Status:</label>
                        <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="filter-select" 
                            AutoPostBack="true" OnSelectedIndexChanged="ApplyFilters">
                            <asp:ListItem Value="Pending" Selected="True">Pending Only</asp:ListItem>
                            <asp:ListItem Value="All">All Reports</asp:ListItem>
                            <asp:ListItem Value="Resolved">Resolved</asp:ListItem>
                            <asp:ListItem Value="Dismissed">Dismissed</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="filter-group">
                        <label class="filter-label">Violation Type:</label>
                        <asp:DropDownList ID="ddlViolationFilter" runat="server" CssClass="filter-select" 
                            AutoPostBack="true" OnSelectedIndexChanged="ApplyFilters">
                            <asp:ListItem Value="All" Selected="True">All Types</asp:ListItem>
                            <asp:ListItem Value="Inappropriate">Inappropriate</asp:ListItem>
                            <asp:ListItem Value="Copyright">Copyright</asp:ListItem>
                            <asp:ListItem Value="Spam">Spam</asp:ListItem>
                            <asp:ListItem Value="Other">Other</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
            </div>

            <!-- Reports List -->
            <div class="admin-content-card">
                <div class="admin-card-header">
                    <h3 class="admin-card-title"> Reported Content</h3>
                </div>

                <!-- Empty State -->
                <asp:Panel ID="pnlEmptyState" runat="server" CssClass="admin-empty-state" Visible="false">
                    <div class="admin-empty-icon"></div>
                    <h3 class="admin-empty-title">No Pending Reports</h3>
                    <p class="admin-empty-description">All moderation reports have been reviewed. Great work!</p>
                </asp:Panel>

                <!-- Reports List -->
                <div class="admin-data-list">
                    <asp:Repeater ID="rptReports" runat="server" OnItemCommand="rptReports_ItemCommand">
                        <ItemTemplate>
                            <div class="data-item">
                                <div class="data-item-header">
                                    <div>
                                        <span class="data-item-title">Report #<%# Eval("ReportID") %></span>
                                        <span class="admin-badge badge-danger">
                                            <%# Eval("ViolationType") %>
                                        </span>
                                    </div>
                                    <div style="font-size: var(--text-sm); color: var(--color-text-muted);">
                                        <%# Convert.ToDateTime(Eval("ReportDate")).ToString("MMM dd, yyyy hh:mm tt") %>
                                    </div>
                                </div>
                                
                                <div class="data-item-meta">
                                    <div style="margin-bottom: var(--spacing-xs);">
                                        <strong>Reported By:</strong> <%# Eval("ReporterUsername") %>
                                    </div>
                                    <div style="margin-bottom: var(--spacing-xs);">
                                        <strong>Content Type:</strong> 
                                        <%# Eval("SharedProgressionID") != DBNull.Value ? "Shared Progression" : "Comment" %>
                                    </div>
                                    <div style="margin-bottom: var(--spacing-xs);">
                                        <strong>Content Owner:</strong> 
                                        <%# Eval("ContentOwnerUsername") ?? Eval("CommenterUsername") %>
                                    </div>
                                    <%# Eval("ProgressionTitle") != DBNull.Value ? 
                                        "<div style='margin-bottom: var(--spacing-xs);'><strong>Progression:</strong> " + Eval("ProgressionTitle") + "</div>" : "" %>
                                    <div style="margin-top: var(--spacing-sm); padding: var(--spacing-sm); background: var(--admin-gray-50); border-radius: var(--radius-sm);">
                                        <strong>Reason:</strong> <%# Eval("ReportDetails") %>
                                    </div>
                                </div>
                                
                                <div class="data-item-actions">
                                    <asp:Button ID="btnViewDetails" runat="server" Text=" View Details" 
                                        CssClass="btn-admin btn-admin-primary btn-admin-sm" 
                                        CommandName="ViewDetails" 
                                        CommandArgument='<%# Eval("ReportID") %>' />
                                    
                                    <asp:Button ID="btnApprove" runat="server" Text=" Dismiss Report" 
                                        CssClass="btn-admin btn-admin-success btn-admin-sm" 
                                        CommandName="Approve" 
                                        CommandArgument='<%# Eval("ReportID") %>' 
                                        OnClientClick="return confirm('Approve this content and dismiss the report?');" />
                                    
                                    <asp:Button ID="btnWarn" runat="server" Text=" Warn User" 
                                        CssClass="btn-admin btn-admin-warning btn-admin-sm" 
                                        CommandName="Warn" 
                                        CommandArgument='<%# Eval("ReportID") %>' 
                                        OnClientClick="return confirm('Send warning to content owner?');" />
                                    
                                    <asp:Button ID="btnRemove" runat="server" Text=" Remove Content" 
                                        CssClass="btn-admin btn-admin-danger btn-admin-sm" 
                                        CommandName="Remove" 
                                        CommandArgument='<%# Eval("ReportID") %>' 
                                        OnClientClick="return confirm('Remove this content permanently?');" />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>

            <!-- Status Message -->
            <asp:Label ID="lblMessage" runat="server" CssClass="alert" Visible="false" 
                style="display:block; margin-top:var(--spacing-lg); padding:var(--spacing-md); border-radius:var(--radius-md);"></asp:Label>
        </div>
    </div>

    <!-- Report Details Modal -->
    <div id="modalReportDetails" class="admin-modal-overlay">
        <div class="admin-modal-content">
            <div class="admin-modal-header">
                <h2 class="admin-modal-title">Report Details</h2>
                <button type="button" class="admin-modal-close" onclick="closeModal()">&times;</button>
            </div>
            <div id="modalBody">
                <asp:Panel ID="pnlModalDetails" runat="server"></asp:Panel>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function showModal() {
            if (window.adminAnimations) {
                window.adminAnimations.showModal('modalReportDetails');
            } else {
                document.getElementById('modalReportDetails').style.display = 'flex';
            }
        }

        function closeModal() {
            if (window.adminAnimations) {
                window.adminAnimations.closeModal('modalReportDetails');
            } else {
                document.getElementById('modalReportDetails').style.display = 'none';
            }
        }

        // Close modal when clicking outside
        window.onclick = function (event) {
            var modal = document.getElementById('modalReportDetails');
            if (event.target == modal) {
                closeModal();
            }
        }
    </script>

    <!-- Admin Animation Scripts -->
    <script src="<%= ResolveUrl("~/Scripts/animations-admin.js") %>"></script>
</asp:Content>
