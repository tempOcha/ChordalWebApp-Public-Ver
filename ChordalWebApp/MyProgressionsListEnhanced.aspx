<%@ Page Title="My Progressions" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="MyProgressionsListEnhanced.aspx.cs" Inherits="ChordalWebApp.MyProgressionsListEnhanced" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    My Progressions - Chordal
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- My Progressions Styles -->
    <link href="/Styles/myprog-styles.css" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- My Progressions Header with Wave Animation -->
    <div class="myprog-header-section">
        <canvas id="myprog-wave-canvas"></canvas>
        <div class="container">
            <div class="myprog-header-content">
                <h1 class="myprog-header-title">My Chord Progressions</h1>
                <p class="myprog-header-subtitle">Organize, manage, and share your musical creations</p>
            </div>
        </div>
    </div>
    
    <div class="container">
        <!-- Status Messages -->
        <asp:Panel ID="pnlMessages" runat="server" Visible="false">
            <asp:Literal ID="litMessage" runat="server"></asp:Literal>
        </asp:Panel>
        
        <div class="myprog-browse-layout">
            <!-- Sidebar Filters -->
            <aside class="filters-sidebar">
                <div class="filters-header">
                    <h2 class="filters-title">Filters</h2>
                    <asp:LinkButton ID="lnkResetFilters" runat="server" OnClick="btnClearSearch_Click" CssClass="filters-reset">
                        Reset
                    </asp:LinkButton>
                </div>
                
                <div class="filter-group">
                    <label class="filter-group-title">Search</label>
                    <asp:TextBox ID="txtSearch" runat="server" 
                                placeholder="Search progressions..." 
                                CssClass="filter-input"></asp:TextBox>
                </div>
                
                <div class="filter-group">
                    <label class="filter-group-title">Category</label>
                    <asp:DropDownList ID="ddlCategoryFilter" runat="server" CssClass="filter-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCategoryFilter_SelectedIndexChanged">
                        <asp:ListItem Value="" Text="All Categories"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                
                <div class="filter-group">
                    <asp:Button ID="btnSearch" runat="server" Text="Apply Filters" 
                               CssClass="btn-search" OnClick="btnSearch_Click" CausesValidation="false" />
                </div>
                
                <div class="filter-group action-buttons-group">
                    <asp:HyperLink NavigateUrl="~/UploadProgression.aspx" runat="server" CssClass="btn-action-primary">
                        Upload New Progression
                    </asp:HyperLink>
                    <asp:HyperLink NavigateUrl="~/ProgressionCategories.aspx" runat="server" CssClass="btn-action-secondary">
                        Manage Categories
                    </asp:HyperLink>
                </div>
            </aside>
            
            <!-- Main Content Area -->
            <div class="browse-main-content">
                <!-- Results Bar -->
                <div class="results-bar">
                    <div class="results-count">
                        <asp:Literal ID="litResultsCount" runat="server" Text="My Progressions"></asp:Literal>
                    </div>
                </div>
                
                <!-- Results Info -->
                <asp:Panel ID="pnlResultsInfo" runat="server" CssClass="results-info-banner" Visible="false">
                    <asp:Literal ID="litResultsInfo" runat="server"></asp:Literal>
                </asp:Panel>
                
                <!-- No Progressions Message -->
                <asp:PlaceHolder ID="phNoProgressions" runat="server" Visible="false">
                    <div class="no-results">
                        <div class="no-results-icon">🎵</div>
                        <h3>No Progressions Found</h3>
                        <p>
                            <asp:Literal ID="litNoProgressionsMessage" runat="server" 
                                Text="You haven't uploaded any chord progressions yet."></asp:Literal>
                        </p>
                        <asp:HyperLink NavigateUrl="~/UploadProgression.aspx" runat="server" CssClass="btn-search">
                            Upload Your First Progression
                        </asp:HyperLink>
                    </div>
                </asp:PlaceHolder>
                
                <!-- Progressions Grid -->
                <asp:Repeater ID="rptProgressions" runat="server" OnItemCommand="rptProgressions_ItemCommand">
                    <HeaderTemplate>
                        <div class="progressions-grid">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <div class="progression-card">
                            
                            <!-- Colored Content Area (clickable) -->
                            <asp:LinkButton ID="lnkViewDetails" runat="server" 
                                           CommandName="ViewDetails" 
                                           CommandArgument='<%# Eval("ProgressionID") %>'
                                           CssClass="card-clickable-area">
                                <div class="card-content-area" 
                                     style="background-color: <%# !string.IsNullOrEmpty(Eval("CategoryColor").ToString()) ? Eval("CategoryColor").ToString() : "#E8F5F3" %>;">
                                    
                                    <div class="card-date">
                                        <%# Convert.ToDateTime(Eval("UploadDate")).ToString("MMM dd, yyyy") %>
                                    </div>
                                    
                                    <div class="card-company">
                                        <%# !string.IsNullOrEmpty(Eval("CategoryName").ToString()) ? 
                                            Server.HtmlEncode(Eval("CategoryName").ToString()) : 
                                            "Uncategorized" %>
                                    </div>
                                    
                                    <h3 class="card-title">
                                        <%# Server.HtmlEncode(Eval("ProgressionTitle").ToString()) %>
                                    </h3>
                                    
                                    <div class="card-badges">
                                        <span class="card-badge">
                                            <%# Eval("KeySignature") %>
                                        </span>
                                        <span class="card-badge">
                                            <%# Eval("ChordEventCount") %> chords
                                        </span>
                                    </div>
                                    
                                    <p class="card-description">
                                        Click to view progression details
                                    </p>
                                </div>
                            </asp:LinkButton>
                            
                            <!-- White Footer -->
                            <div class="card-footer">
                                <div class="card-meta-left">
                                    <span>Uploaded <%# Convert.ToDateTime(Eval("UploadDate")).ToString("MMM dd, yyyy") %></span>
                                </div>
                                <div class="card-footer-actions">
                                    <asp:LinkButton runat="server" 
                                        CommandName="ViewDetails" 
                                        CommandArgument='<%# Eval("ProgressionID") %>'
                                        CssClass="btn-details">
                                        View Details
                                    </asp:LinkButton>
                                    <asp:LinkButton runat="server" 
                                        CommandName="Categorize" 
                                        CommandArgument='<%# Eval("ProgressionID") %>'
                                        CssClass="btn-categorize"
                                        ToolTip="Assign to category">
                                        📁
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                    <FooterTemplate>
                        </div>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>
    
    <!-- Category Assignment Modal -->
    <asp:Panel ID="pnlCategoryModal" runat="server" Visible="false" CssClass="category-modal-overlay">
        <div class="category-modal-content">
            <h3>Assign Category</h3>
            <asp:HiddenField ID="hfSelectedProgressionId" runat="server" />
            
            <div class="form-group">
                <label>Select Category:</label>
                <asp:DropDownList ID="ddlCategories" runat="server">
                </asp:DropDownList>
            </div>
            
            <div class="modal-button-group">
                <asp:Button ID="btnAssignCategory" runat="server" Text="Assign" CssClass="btn-modal-primary" OnClick="btnAssignCategory_Click" CausesValidation="false" />
                <asp:Button ID="btnCancelCategory" runat="server" Text="Cancel" CssClass="btn-modal-secondary" OnClick="btnCancelCategory_Click" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>
    
    <!-- Wave Animation Script -->
    <script src="/Scripts/animations-myprog.js"></script>
</asp:Content>
