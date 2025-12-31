<%@ Page Title="My Progress - Chordal" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="MyProgress.aspx.cs" Inherits="ChordalWebApp.MyProgress" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    My Progress
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="/Styles/progress-styles.css" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    
    <!-- Hero Section with Waves -->
    <section class="progress-hero-section">
        <div id="progress-wave-canvas"></div>
        <div class="progress-hero-overlay"></div>
        <div class="container">
            <div class="progress-hero-content">
                <h1 class="progress-hero-title">Your Learning Journey</h1>
                <p class="progress-hero-subtitle">Track your progress through music theory mastery, one station at a time</p>
            </div>
        </div>
    </section>

    <div class="container" style="max-width: 1200px; margin: 40px auto; padding: 20px;">
        
        <!-- Overall Statistics -->
        <div class="progress-stats-grid">
            <div class="progress-stat-card">
                <span class="progress-stat-number">
                    <asp:Literal ID="litTotalLessons" runat="server" Text="0"></asp:Literal>
                </span>
                <div class="progress-stat-label">Total Lessons</div>
            </div>
            <div class="progress-stat-card">
                <span class="progress-stat-number">
                    <asp:Literal ID="litCompletedLessons" runat="server" Text="0"></asp:Literal>
                </span>
                <div class="progress-stat-label">Completed</div>
            </div>
            <div class="progress-stat-card">
                <span class="progress-stat-number">
                    <asp:Literal ID="litInProgressLessons" runat="server" Text="0"></asp:Literal>
                </span>
                <div class="progress-stat-label">In Progress</div>
            </div>
            <div class="progress-stat-card">
                <span class="progress-stat-number">
                    <asp:Literal ID="litOverallPercentage" runat="server" Text="0"></asp:Literal>%
                </span>
                <div class="progress-stat-label">Overall Progress</div>
            </div>
        </div>

        <!-- Category Progress as Train Journey -->
        <div class="progress-journey-section">
            <h2 class="progress-journey-title">Your Learning Path</h2>
            
            <div class="train-track">
                <div class="track-line"></div>
                
                <asp:Literal ID="litTrainStations" runat="server"></asp:Literal>

                <asp:Panel ID="pnlNoCategoryData" runat="server" Visible="false">
                    <div class="train-station not-started">
                        <div class="station-marker">
                            <div class="station-dot">🎓</div>
                        </div>
                        <div class="station-card">
                            <div class="station-header">
                                <div class="station-category">Start Your Journey</div>
                                <span class="station-badge not-started">Ready to Begin</span>
                            </div>
                            <h3 class="station-title">Begin Your Music Theory Journey</h3>
                            <p class="station-description">No progress yet, but every great journey starts with a single step. Head to the Learning Centre to start your first lesson!</p>
                            <div style="margin-top: 20px;">
                                <a href="LearningCentre.aspx" class="btn btn-primary">Visit Learning Centre</a>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </div>

        <!-- Category Summary Cards -->
        <div class="progress-categories-section">
            <h2 class="progress-section-title">Category Details</h2>
            <div class="category-cards-grid">
                <asp:Repeater ID="rptCategorySummary" runat="server">
                    <ItemTemplate>
                        <div class="category-summary-card">
                            <h3 class="category-name"><%# Eval("CategoryName") %></h3>
                            <div class="lc-progress-bar-container" style="margin: 15px 0;">
                                <div class="lc-progress-bar-fill" style='width: <%# Eval("CompletionPercentage") %>%'></div>
                            </div>
                            <div class="category-stats">
                                <div class="category-stat">
                                    <span class="category-stat-value"><%# Eval("CompletedLessons") %></span>
                                    <span class="category-stat-label">Completed</span>
                                </div>
                                <div class="category-stat">
                                    <span class="category-stat-value"><%# Eval("StartedLessons") %></span>
                                    <span class="category-stat-label">Started</span>
                                </div>
                                <div class="category-stat">
                                    <span class="category-stat-value"><%# Eval("TotalLessons") %></span>
                                    <span class="category-stat-label">Total</span>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <!-- Achievements (if any completed lessons) -->
        <asp:Panel ID="pnlAchievements" runat="server" Visible="false">
            <div class="progress-achievements-section">
                <h2 class="progress-achievements-title">
                    <span></span>
                    <span>Recent Achievements</span>
                </h2>
                <div class="achievement-badges-container">
                    <asp:Literal ID="litAchievements" runat="server"></asp:Literal>
                </div>
            </div>
        </asp:Panel>

        <!-- Recommended Next Steps -->
        <asp:Panel ID="pnlRecommendations" runat="server" Visible="true">
            <div class="progress-recommended-section">
                <h2 class="progress-recommended-title">
                    <span></span>
                    <span>Recommended Next Steps</span>
                </h2>
                <div class="recommended-lessons-grid">
                    <asp:Repeater ID="rptRecommendations" runat="server">
                        <ItemTemplate>
                            <div class="recommended-lesson-card" onclick="window.location.href='Lesson.aspx?slug=<%# Eval("LessonSlug") %>'">
                                <div class="recommended-lesson-icon"></div>
                                <h3 class="recommended-lesson-title"><%# Server.HtmlEncode(Eval("LessonTitle").ToString()) %></h3>
                                <p class="recommended-lesson-description"><%# Server.HtmlEncode(Eval("Description").ToString()) %></p>
                                <div class="recommended-lesson-meta">
                                    <span class='recommended-difficulty-badge recommended-difficulty-<%# Eval("DifficultyLevel").ToString().ToLower() %>'>
                                        <%# Eval("DifficultyLevel") %>
                                    </span>
                                    <span class="recommended-meta-item">
                                         <%# Eval("EstimatedMinutes") != DBNull.Value ? Eval("EstimatedMinutes") + " min" : "N/A" %>
                                    </span>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlNoRecommendations" runat="server" Visible="false">
            <div class="lc-progress-summary" style="text-align: center; margin-top: 40px;">
                <h2 class="lc-progress-title">Excellent Progress! 🎉</h2>
                <p class="lc-progress-description">You're making great strides in your music theory journey. Check out the <a href="LearningCentre.aspx" style="color: var(--color-teal); font-weight: 600;">Learning Centre</a> to explore more lessons.</p>
            </div>
        </asp:Panel>

    </div>

    <!-- Animation Scripts -->
    <script src='<%= ResolveUrl("~/Scripts/animations-progress.js") %>'></script>

</asp:Content>
