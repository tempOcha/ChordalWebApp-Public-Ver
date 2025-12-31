<%@ Page Title="Learning Centre - Chordal" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="LearningCentre.aspx.cs" Inherits="ChordalWebApp.LearningCentre" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Learning Centre
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="/Styles/lc-styles.css" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    
    <!-- Hero Section with Waves -->
    <section class="lc-hero-section">
        <div id="lc-wave-canvas"></div>
        <div class="lc-hero-overlay"></div>
        <div class="container">
            <div class="lc-hero-content">
                <h1 class="lc-hero-title">Music Theory Learning Centre</h1>
                <p class="lc-hero-subtitle">Master chord progressions and harmonic concepts through structured lessons and interactive practice</p>
            </div>
        </div>
    </section>

    <div class="container" style="max-width: 1200px; margin: 40px auto;">
        
        <!-- Progress Summary (for logged-in users) -->
        <asp:Panel ID="pnlProgressSummary" runat="server" CssClass="lc-progress-summary" Visible="false">
            <h2 class="lc-progress-title">Your Learning Progress</h2>
            <p class="lc-progress-description">Keep up the great work! Here's your progress across all categories.</p>
            
            <div class="lc-stats-grid">
                <div class="lc-stat-card">
                    <span class="lc-stat-value"><asp:Literal ID="litTotalLessons" runat="server">0</asp:Literal></span>
                    <div class="lc-stat-label">Total Lessons</div>
                </div>
                <div class="lc-stat-card">
                    <span class="lc-stat-value"><asp:Literal ID="litCompletedLessons" runat="server">0</asp:Literal></span>
                    <div class="lc-stat-label">Completed</div>
                </div>
                <div class="lc-stat-card">
                    <span class="lc-stat-value"><asp:Literal ID="litInProgressLessons" runat="server">0</asp:Literal></span>
                    <div class="lc-stat-label">In Progress</div>
                </div>
                <div class="lc-stat-card">
                    <span class="lc-stat-value"><asp:Literal ID="litOverallProgress" runat="server">0</asp:Literal>%</span>
                    <div class="lc-stat-label">Overall Progress</div>
                </div>
            </div>
        </asp:Panel>
        
        <!-- Recommended Lessons (for logged-in users) -->
        <asp:Panel ID="pnlRecommended" runat="server" CssClass="lc-recommended-section" Visible="false">
            <h2 class="lc-recommended-title">📚 Recommended for You</h2>
            <div class="lc-lessons-grid">
                <asp:Repeater ID="rptRecommended" runat="server">
                    <ItemTemplate>
                        <div class="lc-lesson-card" onclick="window.location.href='Lesson.aspx?slug=<%# Eval("LessonSlug") %>'">
                            <div class='lc-lesson-status <%# Convert.ToBoolean(Eval("IsStarted")) ? "in-progress" : "not-started" %>'>
                                <%# Convert.ToBoolean(Eval("IsStarted")) ? "📖" : "⭐" %>
                            </div>
                            <div class="lc-lesson-title"><%# Server.HtmlEncode(Eval("LessonTitle").ToString()) %></div>
                            <div class="lc-lesson-description"><%# Server.HtmlEncode(Eval("Description").ToString()) %></div>
                            <div class="lc-lesson-meta">
                                <span class='lc-difficulty-badge lc-difficulty-<%# Eval("DifficultyLevel").ToString().ToLower() %>'>
                                    <%# Eval("DifficultyLevel") %>
                                </span>
                                <span class="lc-meta-item">
                                    ⏱️ <%# Eval("EstimatedMinutes") %> min
                                </span>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>
        
        <!-- Lesson Categories -->
        <asp:Repeater ID="rptCategories" runat="server" OnItemDataBound="rptCategories_ItemDataBound">
            <ItemTemplate>
                <div class="lc-category-section">
                    <div class="lc-category-header">
                        <div class="lc-category-info">
                            <h2 class="lc-category-title"><%# Server.HtmlEncode(Eval("CategoryName").ToString()) %></h2>
                            <p class="lc-category-description"><%# Server.HtmlEncode(Eval("Description").ToString()) %></p>
                        </div>
                        <asp:Panel runat="server" ID="pnlCategoryProgress" CssClass="lc-category-progress" Visible='<%# Session["UserID"] != null %>'>
                            <div class="lc-progress-bar-container">
                                <div class="lc-progress-bar-fill" style='width: <%# Eval("CompletionPercentage") %>%'></div>
                            </div>
                            <div class="lc-progress-text">
                                <%# Eval("CompletedLessons") %> of <%# Eval("TotalLessons") %> completed
                            </div>
                        </asp:Panel>
                    </div>
                    
                    <div class="lc-lessons-grid">
                        <asp:Repeater ID="rptLessons" runat="server">
                            <ItemTemplate>
                                <div class="lc-lesson-card" onclick="window.location.href='Lesson.aspx?slug=<%# Eval("LessonSlug") %>'">
                                    <div class='lc-lesson-status <%# GetLessonStatusClass(Eval("Status")) %>'>
                                        <%# GetLessonStatusIcon(Eval("Status")) %>
                                    </div>
                                    <div class="lc-lesson-title"><%# Server.HtmlEncode(Eval("LessonTitle").ToString()) %></div>
                                    <div class="lc-lesson-description"><%# Server.HtmlEncode(Eval("Description").ToString()) %></div>
                                    <div class="lc-lesson-meta">
                                        <span class='lc-difficulty-badge lc-difficulty-<%# Eval("DifficultyLevel").ToString().ToLower() %>'>
                                            <%# Eval("DifficultyLevel") %>
                                        </span>
                                        <span class="lc-meta-item">
                                            ⏱️ <%# Eval("EstimatedMinutes") %> min
                                        </span>
                                        <%# Convert.ToBoolean(Eval("HasTutorial")) ? "<span class='lc-meta-item'>🎹 Interactive</span>" : "" %>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <!-- Animation Scripts -->
    <script src='<%= ResolveUrl("~/Scripts/animations-lc.js") %>'></script>

</asp:Content>
