<%@ Page Title="Community Guidelines" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="Guidelines.aspx.cs" Inherits="ChordalWebApp.Guidelines" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Community Guidelines - Chordal
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="Styles/guide-styles.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Header Section with Particle Background -->
    <div class="guidelines-header-section">
        <div id="guidelines-particles-canvas"></div>
        <div class="guidelines-header-content">
            <div class="guidelines-badge">Code of Conduct</div>
            <h1 class="guidelines-header-title">Community Guidelines</h1>
            <p class="guidelines-header-subtitle">Creating a respectful and creative environment for all musicians to share, learn, and grow together</p>
        </div>
    </div>

    <!-- Main Content -->
    <div class="guidelines-container">
        <!-- Guidelines List -->
        <asp:Panel ID="pnlGuidelines" runat="server">
            <asp:Repeater ID="rptGuidelines" runat="server">
                <ItemTemplate>
                    <div class="guideline-section">
                        <div class="guideline-section-number"><%# Container.ItemIndex + 1 %></div>
                        <h2><%# Eval("SectionTitle") %></h2>
                        <p><%# Eval("SectionContent") %></p>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <!-- Empty State -->
        <asp:Panel ID="pnlEmptyState" runat="server" CssClass="guidelines-empty-state" Visible="false">
            <div class="guidelines-empty-icon">📜</div>
            <h3>No Guidelines Available</h3>
            <p>Community guidelines are being prepared. Please check back soon.</p>
        </asp:Panel>

        <!-- Footer Section -->
        <div class="guidelines-footer">
            <div class="last-updated">
                Last updated: <asp:Label ID="lblLastUpdated" runat="server"></asp:Label>
            </div>

            <!-- Admin Edit Link -->
            <asp:Panel ID="pnlAdminLink" runat="server" Visible="false" CssClass="admin-edit-section">
                <asp:HyperLink ID="hlEditGuidelines" runat="server" NavigateUrl="~/ManageGuidelines.aspx" CssClass="admin-edit-btn">
                    <span class="admin-edit-icon">✏️</span> Edit Guidelines (Admin)
                </asp:HyperLink>
            </asp:Panel>
        </div>
    </div>

    <!-- Scripts -->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/p5.js/1.4.0/p5.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/animejs/3.2.1/anime.min.js"></script>
    <script src="Scripts/animations-guide.js"></script>
</asp:Content>
