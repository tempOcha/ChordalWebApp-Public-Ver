<%@ Page Title="Community Progressions" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="CommunityBrowse.aspx.cs" Inherits="ChordalWebApp.CommunityBrowse" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Community - Chordal
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Community Styles -->
    <link href="/Styles/comm-styles.css" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Community Header with Wave Animation -->
    <div class="community-header-section">
        <canvas id="community-wave-canvas"></canvas>
        <div class="container">
            <div class="community-header-content">
                <h1 class="community-header-title">Community Progressions</h1>
                <p class="community-header-subtitle">Discover and explore chord progressions shared by musicians worldwide</p>
            </div>
        </div>
    </div>
    
    <div class="container">
        <!-- Status Messages -->
        <asp:Panel ID="pnlMessages" runat="server" Visible="false">
            <asp:Literal ID="litMessage" runat="server"></asp:Literal>
        </asp:Panel>
        
        <div class="community-browse-layout">
            <!-- Sidebar Filters -->
            <aside class="filters-sidebar">
                <div class="filters-header">
                    <h2 class="filters-title">Filters</h2>
                    <asp:LinkButton ID="lnkResetFilters" runat="server" OnClick="btnClearFilters_Click" CssClass="filters-reset">
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
                    <label class="filter-group-title">Genre/Style</label>
                    <asp:DropDownList ID="ddlGenre" runat="server" CssClass="filter-select">
                        <asp:ListItem Value="" Text="All Genres"></asp:ListItem>
                        <asp:ListItem Value="Jazz" Text="Jazz"></asp:ListItem>
                        <asp:ListItem Value="Blues" Text="Blues"></asp:ListItem>
                        <asp:ListItem Value="Rock" Text="Rock"></asp:ListItem>
                        <asp:ListItem Value="Pop" Text="Pop"></asp:ListItem>
                        <asp:ListItem Value="Classical" Text="Classical"></asp:ListItem>
                        <asp:ListItem Value="Folk" Text="Folk"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                
                <div class="filter-group">
                    <label class="filter-group-title">Key</label>
                    <asp:DropDownList ID="ddlKey" runat="server" CssClass="filter-select">
                        <asp:ListItem Value="" Text="All Keys"></asp:ListItem>
                        <asp:ListItem Value="0,1" Text="C Major"></asp:ListItem>
                        <asp:ListItem Value="0,0" Text="C Minor"></asp:ListItem>
                        <asp:ListItem Value="2,1" Text="D Major"></asp:ListItem>
                        <asp:ListItem Value="2,0" Text="D Minor"></asp:ListItem>
                        <asp:ListItem Value="4,1" Text="E Major"></asp:ListItem>
                        <asp:ListItem Value="4,0" Text="E Minor"></asp:ListItem>
                        <asp:ListItem Value="5,1" Text="F Major"></asp:ListItem>
                        <asp:ListItem Value="5,0" Text="F Minor"></asp:ListItem>
                        <asp:ListItem Value="7,1" Text="G Major"></asp:ListItem>
                        <asp:ListItem Value="7,0" Text="G Minor"></asp:ListItem>
                        <asp:ListItem Value="9,1" Text="A Major"></asp:ListItem>
                        <asp:ListItem Value="9,0" Text="A Minor"></asp:ListItem>
                        <asp:ListItem Value="11,1" Text="B Major"></asp:ListItem>
                        <asp:ListItem Value="11,0" Text="B Minor"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                
                <div class="filter-group">
                    <asp:Button ID="btnSearch" runat="server" Text="Apply Filters" 
                               CssClass="btn-search" OnClick="btnSearch_Click" />
                </div>
            </aside>
            
            <!-- Main Content Area -->
            <div class="browse-main-content">
                <!-- Results Bar -->
                <div class="results-bar">
                    <div class="results-count">
                        <asp:Literal ID="litResultsCount" runat="server"></asp:Literal>
                    </div>
                    <div class="sort-container">
                        <label class="sort-label">Sort by:</label>
                        <asp:DropDownList ID="ddlSort" runat="server" AutoPostBack="true" 
                                        OnSelectedIndexChanged="ddlSort_SelectedIndexChanged"
                                        CssClass="sort-select">
                            <asp:ListItem Value="Recent" Text="Most Recent"></asp:ListItem>
                            <asp:ListItem Value="Popular" Text="Most Popular"></asp:ListItem>
                            <asp:ListItem Value="MostLiked" Text="Most Liked"></asp:ListItem>
                            <asp:ListItem Value="MostViewed" Text="Most Viewed"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                
                <!-- Progressions Grid -->
                <asp:Panel ID="pnlProgressions" runat="server">
                    <asp:Repeater ID="rptProgressions" runat="server" OnItemCommand="rptProgressions_ItemCommand">
                        <HeaderTemplate>
                            <div class="progressions-grid">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkCard" runat="server" 
                                           CommandName="ViewDetails" 
                                           CommandArgument='<%# Eval("SharedProgressionID") %>'
                                           CssClass='<%# "progression-card genre-" + GetGenreClass(Eval("Tags")) %>'>
                                
                                <!-- Colored Content Area -->
                                <div class="card-content-area">
                                    <div class="card-date">
                                        <%# GetFormattedDate(Eval("ShareDate")) %>
                                    </div>
                                    
                                    <div class="card-company">
                                        <%# Server.HtmlEncode(Eval("OwnerUsername").ToString()) %>
                                    </div>
                                    
                                    <h3 class="card-title">
                                        <%# Server.HtmlEncode(Eval("ShareTitle").ToString()) %>
                                    </h3>
                                    
                                    <div class="card-badges">
                                        <span class="card-badge genre <%# "genre-" + Eval("Tags").ToString().ToLower().Split(',')[0].Trim() %>">
                                         <%# Eval("Tags").ToString().Split(',')[0].Trim() %>
                                        </span>
                                        <span class="card-badge">
                                            <%# GetKeyName(Eval("KeyRoot"), Eval("IsKeyMajor")) %>
                                        </span>
                                        <span class="card-badge">
                                            <%# Eval("ChordCount") %> chords
                                        </span>
                                    </div>
                                    
                                    <p class="card-description">
                                        <%# Server.HtmlEncode(
                                            Eval("ShareDescription") == DBNull.Value || string.IsNullOrEmpty(Eval("ShareDescription").ToString()) 
                                            ? "No description provided" 
                                            : Eval("ShareDescription").ToString()
                                        ) %>
                                    </p>
                                </div>
                                
                                <!-- White Footer -->
                                <div class="card-footer">
                                    <div class="card-meta-left">
                                        <div class="card-rating">
                                            <span class="card-rating-stars">★</span>
                                            <span class="card-rating-text">
                                                <%# Eval("AverageRating") != DBNull.Value ? 
                                                    string.Format("{0:0.0}", Eval("AverageRating")) : "N/A" %>
                                            </span>
                                        </div>
                                        <span>•</span>
                                        <span><%# Eval("ViewCount") %> views</span>
                                    </div>
                                    <button type="button" class="btn-details" onclick="return true;">Details</button>
                                </div>
                            </asp:LinkButton>
                        </ItemTemplate>
                        <FooterTemplate>
                            </div>
                        </FooterTemplate>
                    </asp:Repeater>
                </asp:Panel>
                
                <!-- No Results -->
                <asp:Panel ID="pnlNoResults" runat="server" Visible="false" CssClass="no-results">
                    <div class="no-results-icon">🎵</div>
                    <h3>No progressions found</h3>
                    <p>Try adjusting your search filters or browse all progressions</p>
                    <asp:Button ID="btnClearFilters" runat="server" Text="Clear Filters" 
                               CssClass="btn-search" OnClick="btnClearFilters_Click" style="max-width: 200px; margin: 0 auto;" />
                </asp:Panel>
                
                <!-- Pagination -->
                <asp:Panel ID="pnlPagination" runat="server" CssClass="pagination">
                    <asp:Literal ID="litPagination" runat="server"></asp:Literal>
                </asp:Panel>
            </div>
        </div>
    </div>
    
    <!-- Wave Animation Script -->
    <script src="/Scripts/animations-comm.js"></script>
</asp:Content>
