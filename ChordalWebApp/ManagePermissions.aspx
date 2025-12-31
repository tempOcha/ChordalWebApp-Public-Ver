<%@ Page Title="Manage Permissions" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="ManagePermissions.aspx.cs" Inherits="ChordalWebApp.ManagePermissions" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Manage Permissions - Chordal Admin
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
                            Manage User Permissions
                        </h1>
                        <p class="admin-subtitle">Control user roles and account status</p>
                    </div>
                    <div class="admin-header-actions">
                        <asp:HyperLink ID="hlBackToDashboard" runat="server" NavigateUrl="~/AdminDashboard.aspx" CssClass="btn-admin-header">
                             Dashboard
                        </asp:HyperLink>
                    </div>
                </div>
            </div>
        </div>

        <div class="admin-container">
            <!-- Search & Filter Section -->
            <div class="admin-filters">
                <div class="filters-row">
                    <div class="filter-group" style="flex: 2;">
                        <label class="filter-label">Search Users:</label>
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="filter-input" 
                            placeholder="Search by username or email..."></asp:TextBox>
                    </div>
                    
                    <div class="filter-group">
                        <label class="filter-label">Role Filter:</label>
                        <asp:DropDownList ID="ddlRoleFilter" runat="server" CssClass="filter-select">
                            <asp:ListItem Value="All" Selected="True">All Roles</asp:ListItem>
                            <asp:ListItem Value="Admin">Admin Only</asp:ListItem>
                            <asp:ListItem Value="User">Users Only</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    
                    <div class="filter-group">
                        <label class="filter-label">&nbsp;</label>
                        <asp:Button ID="btnSearch" runat="server" Text=" Search" 
                            OnClick="btnSearch_Click" CssClass="btn-admin btn-admin-primary" />
                    </div>
                </div>
            </div>

            <!-- Users Table -->
            <div class="admin-content-card">
                <div class="admin-card-header">
                    <h3 class="admin-card-title"> User Directory</h3>
                </div>

                <!-- Empty State -->
                <asp:Panel ID="pnlEmptyState" runat="server" CssClass="admin-empty-state" Visible="false">
                    <div class="admin-empty-icon"></div>
                    <h3 class="admin-empty-title">No Users Found</h3>
                    <p class="admin-empty-description">Try adjusting your search criteria.</p>
                </asp:Panel>

                <!-- Users Table -->
                <div style="overflow-x: auto;">
                    <asp:GridView ID="gvUsers" runat="server" 
                        CssClass="admin-table" 
                        AutoGenerateColumns="False"
                        OnRowCommand="gvUsers_RowCommand"
                        EmptyDataText="No users found.">
                        <HeaderStyle CssClass="admin-table-header" />
                        <RowStyle CssClass="admin-table-row" />
                        <Columns>
                            <asp:BoundField DataField="Username" HeaderText="Username" />
                            <asp:BoundField DataField="Email" HeaderText="Email" />
                            <asp:TemplateField HeaderText="Role">
                                <ItemTemplate>
                                    <span class='admin-badge <%# Convert.ToBoolean(Eval("IsAdmin")) ? "badge-danger" : "badge-active" %>'>
                                        <%# Convert.ToBoolean(Eval("IsAdmin")) ? "Admin" : "User" %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <span class='admin-badge <%# Convert.ToBoolean(Eval("IsEnabled")) ? "badge-active" : "badge-inactive" %>'>
                                        <%# Convert.ToBoolean(Eval("IsEnabled")) ? "Enabled" : "Disabled" %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="RegistrationDate" HeaderText="Joined" DataFormatString="{0:MMM dd, yyyy}" />
                            <asp:BoundField DataField="ProgressionCount" HeaderText="Progressions" />
                            <asp:BoundField DataField="SharedCount" HeaderText="Shared" />
                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <asp:Button ID="btnEdit" runat="server" Text=" Edit" 
                                        CssClass="btn-admin btn-admin-primary btn-admin-sm" 
                                        CommandName="EditUser" 
                                        CommandArgument='<%# Eval("UserID") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

            <!-- Status Message -->
            <asp:Label ID="lblMessage" runat="server" CssClass="alert" Visible="false" 
                style="display:block; margin-top:var(--spacing-lg); padding:var(--spacing-md); border-radius:var(--radius-md);"></asp:Label>
        </div>
    </div>

    <!-- Edit User Modal -->
    <div id="modalEditUser" class="admin-modal-overlay">
        <div class="admin-modal-content">
            <div class="admin-modal-header">
                <h2 class="admin-modal-title">Edit User Permissions</h2>
                <button type="button" class="admin-modal-close" onclick="closeModal()">&times;</button>
            </div>
            
            <asp:Panel ID="pnlEditUser" runat="server">
                <div class="admin-form-group">
                    <label class="admin-form-label">Username:</label>
                    <asp:Label ID="lblEditUsername" runat="server" CssClass="admin-form-input" 
                        style="background:var(--admin-gray-50); border:none; display:block; padding:0.75rem 1rem;"></asp:Label>
                </div>
                
                <div class="admin-form-group">
                    <label class="admin-form-label">Email:</label>
                    <asp:Label ID="lblEditEmail" runat="server" CssClass="admin-form-input" 
                        style="background:var(--admin-gray-50); border:none; display:block; padding:0.75rem 1rem;"></asp:Label>
                </div>
                
                <!-- User Stats -->
                <div class="admin-stats-grid" style="grid-template-columns: repeat(4, 1fr); margin: var(--spacing-lg) 0;">
                    <div class="stat-card info">
                        <div class="stat-label">Progressions</div>
                        <div class="stat-number" style="font-size: 1.5rem;">
                            <asp:Label ID="lblStatProgressions" runat="server">0</asp:Label>
                        </div>
                    </div>
                    <div class="stat-card success">
                        <div class="stat-label">Shared</div>
                        <div class="stat-number" style="font-size: 1.5rem;">
                            <asp:Label ID="lblStatShared" runat="server">0</asp:Label>
                        </div>
                    </div>
                    <div class="stat-card warning">
                        <div class="stat-label">Comments</div>
                        <div class="stat-number" style="font-size: 1.5rem;">
                            <asp:Label ID="lblStatComments" runat="server">0</asp:Label>
                        </div>
                    </div>
                    <div class="stat-card alert">
                        <div class="stat-label">Reports</div>
                        <div class="stat-number" style="font-size: 1.5rem;">
                            <asp:Label ID="lblStatReports" runat="server">0</asp:Label>
                        </div>
                    </div>
                </div>
                
                <div class="admin-form-group">
                    <div style="display: flex; align-items: center; gap: var(--spacing-sm); padding: var(--spacing-md); background: var(--admin-gray-50); border-radius: var(--radius-md);">
                        <asp:CheckBox ID="chkIsAdmin" runat="server" />
                        <label for="<%= chkIsAdmin.ClientID %>" class="admin-form-label" style="margin: 0; cursor: pointer;">
                             Grant Administrator Privileges
                        </label>
                    </div>
                    <span class="admin-form-help">Admins have full system access</span>
                </div>
                
                <div class="admin-form-group">
                    <div style="display: flex; align-items: center; gap: var(--spacing-sm); padding: var(--spacing-md); background: var(--admin-gray-50); border-radius: var(--radius-md);">
                        <asp:CheckBox ID="chkIsEnabled" runat="server" />
                        <label for="<%= chkIsEnabled.ClientID %>" class="admin-form-label" style="margin: 0; cursor: pointer;">
                             Account Enabled
                        </label>
                    </div>
                    <span class="admin-form-help">Disabled accounts cannot login</span>
                </div>
                
                <div class="admin-form-group">
                    <label class="admin-form-label">Admin Notes (internal only):</label>
                    <asp:TextBox ID="txtAdminNotes" runat="server" TextMode="MultiLine" CssClass="admin-form-textarea" 
                        placeholder="Add notes about this user (e.g., warnings, special circumstances...)"></asp:TextBox>
                    <span class="admin-form-help">These notes are only visible to administrators</span>
                </div>
                
                <asp:HiddenField ID="hfEditUserID" runat="server" />
                
                <div class="admin-mt-lg">
                    <asp:Button ID="btnSavePermissions" runat="server" Text=" Save Changes" 
                        OnClick="btnSavePermissions_Click" CssClass="btn-admin btn-admin-success" />
                    <button type="button" class="btn-admin btn-admin-outline" onclick="closeModal()">Cancel</button>
                </div>
            </asp:Panel>
        </div>
    </div>

    <script type="text/javascript">
        function showModal() {
            if (window.adminAnimations) {
                window.adminAnimations.showModal('modalEditUser');
            } else {
                document.getElementById('modalEditUser').style.display = 'flex';
            }
        }

        function closeModal() {
            if (window.adminAnimations) {
                window.adminAnimations.closeModal('modalEditUser');
            } else {
                document.getElementById('modalEditUser').style.display = 'none';
            }
        }

        window.onclick = function (event) {
            var modal = document.getElementById('modalEditUser');
            if (event.target == modal) {
                closeModal();
            }
        }
    </script>

    <!-- Admin Animation Scripts -->
    <script src="<%= ResolveUrl("~/Scripts/animations-admin.js") %>"></script>

    <style>
        /* Table Styling */
        .admin-table {
            width: 100%;
            border-collapse: collapse;
        }
        
        .admin-table-header {
            background: var(--admin-gray-50);
            padding: var(--spacing-md);
            text-align: left;
            font-weight: 600;
            border-bottom: 2px solid var(--border-color);
            color: var(--color-text-primary);
        }
        
        .admin-table-row {
            border-bottom: 1px solid var(--border-color);
            transition: background var(--transition-base);
        }
        
        .admin-table-row:hover {
            background: var(--admin-gray-50);
        }
        
        .admin-table td {
            padding: var(--spacing-md);
        }
    </style>
</asp:Content>
