<%@ Page Title="Manage Guidelines" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="ManageGuidelines.aspx.cs" Inherits="ChordalWebApp.ManageGuidelines" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Manage Guidelines - Chordal Admin
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
                       
                            Manage Community Guidelines
                        </h1>
                        <p class="admin-subtitle">Edit and organize community rules and guidelines</p>
                    </div>
                    <div class="admin-header-actions">
                        <asp:HyperLink ID="hlPreview" runat="server" NavigateUrl="~/Guidelines.aspx" CssClass="btn-admin-header" Target="_blank">
                             Preview
                        </asp:HyperLink>
                        <asp:HyperLink ID="hlBackToDashboard" runat="server" NavigateUrl="~/AdminDashboard.aspx" CssClass="btn-admin-header">
                            ← Dashboard
                        </asp:HyperLink>
                    </div>
                </div>
            </div>
        </div>

        <div class="admin-container">
            <!-- Add New Button -->
            <div class="admin-mb-lg">
                <asp:Button ID="btnAddNew" runat="server" Text="+ Add New Guideline Section" 
                    CssClass="btn-admin btn-admin-success" OnClick="btnAddNew_Click" />
            </div>

            <!-- Guidelines List -->
            <div class="admin-content-card">
                <div class="admin-card-header">
                    <h3 class="admin-card-title">Active Guidelines</h3>
                </div>
                
                <div class="admin-data-list">
                    <asp:Repeater ID="rptGuidelinesAdmin" runat="server" OnItemCommand="rptGuidelinesAdmin_ItemCommand">
                        <ItemTemplate>
                            <div class="data-item">
                                <div class="data-item-header">
                                    <div>
                                        <span class="data-item-title"><%# Eval("SectionTitle") %></span>
                                        <span class='admin-badge <%# Convert.ToBoolean(Eval("IsActive")) ? "badge-active" : "badge-inactive" %>'>
                                            <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                        </span>
                                    </div>
                                </div>
                                <div class="data-item-meta">
                                    <p style="margin: var(--spacing-sm) 0; color: var(--color-text-secondary);">
                                        <%# Eval("SectionContent") %>
                                    </p>
                                    <div style="font-size: var(--text-sm); color: var(--color-text-muted); margin-top: var(--spacing-sm);">
                                        Order: <%# Eval("SectionOrder") %> | 
                                        Version: <%# Eval("Version") %> | 
                                        Last Modified: <%# Convert.ToDateTime(Eval("LastModified")).ToString("MMM dd, yyyy HH:mm") %>
                                        <%# Eval("ModifiedByUsername") != DBNull.Value ? " by " + Eval("ModifiedByUsername") : "" %>
                                    </div>
                                </div>
                                <div class="data-item-actions">
                                    <asp:Button ID="btnEdit" runat="server" Text=" Edit" CssClass="btn-admin btn-admin-primary btn-admin-sm" 
                                        CommandName="Edit" CommandArgument='<%# Eval("GuidelineID") %>' />
                                    <asp:Button ID="btnToggle" runat="server" 
                                        Text='<%# Convert.ToBoolean(Eval("IsActive")) ? " Deactivate" : " Activate" %>' 
                                        CssClass="btn-admin btn-admin-warning btn-admin-sm" 
                                        CommandName="Toggle" CommandArgument='<%# Eval("GuidelineID") %>' />
                                    <asp:Button ID="btnDelete" runat="server" Text=" Delete" CssClass="btn-admin btn-admin-danger btn-admin-sm" 
                                        CommandName="Delete" CommandArgument='<%# Eval("GuidelineID") %>' 
                                        OnClientClick="return confirm('Are you sure you want to delete this guideline section?');" />
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

    <!-- Edit/Add Modal -->
    <div id="modalEditGuideline" class="admin-modal-overlay">
        <div class="admin-modal-content">
            <div class="admin-modal-header">
                <h2 class="admin-modal-title">
                    <asp:Label ID="lblModalTitle" runat="server" Text="Edit Guideline Section"></asp:Label>
                </h2>
                <button type="button" class="admin-modal-close" onclick="closeModal()">&times;</button>
            </div>
            
            <asp:Panel ID="pnlEditGuideline" runat="server">
                <div class="admin-form-group">
                    <label class="admin-form-label">Section Title:</label>
                    <asp:TextBox ID="txtSectionTitle" runat="server" CssClass="admin-form-input" MaxLength="200" 
                        placeholder="e.g., Be Respectful"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtSectionTitle"
                        ErrorMessage="Section title is required." CssClass="text-danger" Display="Dynamic" ValidationGroup="Guideline"></asp:RequiredFieldValidator>
                </div>
                
                <div class="admin-form-group">
                    <label class="admin-form-label">Section Content:</label>
                    <asp:TextBox ID="txtSectionContent" runat="server" TextMode="MultiLine" CssClass="admin-form-textarea" 
                        placeholder="Detailed description of this guideline..."></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvContent" runat="server" ControlToValidate="txtSectionContent"
                        ErrorMessage="Section content is required." CssClass="text-danger" Display="Dynamic" ValidationGroup="Guideline"></asp:RequiredFieldValidator>
                </div>
                
                <div class="admin-form-group">
                    <label class="admin-form-label">Display Order:</label>
                    <asp:TextBox ID="txtSectionOrder" runat="server" CssClass="admin-form-input" TextMode="Number" 
                        placeholder="1" Text="1"></asp:TextBox>
                    <span class="admin-form-help">Lower numbers appear first</span>
                    <asp:RequiredFieldValidator ID="rfvOrder" runat="server" ControlToValidate="txtSectionOrder"
                        ErrorMessage="Display order is required." CssClass="text-danger" Display="Dynamic" ValidationGroup="Guideline"></asp:RequiredFieldValidator>
                </div>
                
                <div class="admin-form-group">
                    <div style="display: flex; align-items: center; gap: var(--spacing-sm);">
                        <asp:CheckBox ID="chkIsActive" runat="server" Checked="true" />
                        <label for="<%= chkIsActive.ClientID %>" class="admin-form-label" style="margin: 0;">
                            Active (visible to users)
                        </label>
                    </div>
                </div>
                
                <div class="admin-form-group">
                    <label class="admin-form-label">Change Note (optional):</label>
                    <asp:TextBox ID="txtChangeNote" runat="server" CssClass="admin-form-input" MaxLength="500" 
                        placeholder="Brief description of changes made..."></asp:TextBox>
                    <span class="admin-form-help">Help track version history</span>
                </div>
                
                <asp:HiddenField ID="hfEditGuidelineID" runat="server" />
                
                <div class="admin-mt-lg">
                    <asp:Button ID="btnSaveGuideline" runat="server" Text="💾 Save Changes" OnClick="btnSaveGuideline_Click" 
                        CssClass="btn-admin btn-admin-success" ValidationGroup="Guideline" />
                    <button type="button" class="btn-admin btn-admin-outline" onclick="closeModal()">Cancel</button>
                </div>
            </asp:Panel>
        </div>
    </div>

    <script type="text/javascript">
        function showModal() {
            document.getElementById('modalEditGuideline').style.display = 'flex';
            if (window.adminAnimations) {
                window.adminAnimations.showModal('modalEditGuideline');
            }
        }

        function closeModal() {
            if (window.adminAnimations) {
                window.adminAnimations.closeModal('modalEditGuideline');
            } else {
                document.getElementById('modalEditGuideline').style.display = 'none';
            }
        }

        window.onclick = function (event) {
            var modal = document.getElementById('modalEditGuideline');
            if (event.target == modal) {
                closeModal();
            }
        }
    </script>

    <!-- Admin Animation Scripts -->
    <script src="<%= ResolveUrl("~/Scripts/animations-admin.js") %>"></script>
</asp:Content>
