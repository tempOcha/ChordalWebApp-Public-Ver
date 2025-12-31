<%@ Page Title="Lesson" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="Lesson.aspx.cs" Inherits="ChordalWebApp.Lesson" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Piano Roll CSS/JS -->
    <script src="Scripts/ChordalPianoRoll.js"></script>
    
    <!-- Lesson Interactivity - MUST be loaded to prevent button postbacks -->
    <script src="Scripts/LessonInteractivity.js"></script>
    
    <style>
        /* Lesson Page Styles */
        .lesson-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            border-radius: 8px;
            margin-bottom: 30px;
        }

        .breadcrumb {
            color: rgba(255, 255, 255, 0.8);
            font-size: 14px;
            margin-bottom: 10px;
        }

        .breadcrumb a {
            color: rgba(255, 255, 255, 0.9);
            text-decoration: none;
        }

        .breadcrumb a:hover {
            text-decoration: underline;
        }

        .lesson-title {
            font-size: 32px;
            font-weight: bold;
            margin-bottom: 10px;
        }

        .lesson-meta {
            display: flex;
            gap: 20px;
            flex-wrap: wrap;
            align-items: center;
        }

        .meta-badge {
            padding: 5px 12px;
            border-radius: 20px;
            font-size: 13px;
            font-weight: 600;
        }

        .difficulty-beginner { background: #10b981; color: white; }
        .difficulty-intermediate { background: #f59e0b; color: white; }
        .difficulty-advanced { background: #ef4444; color: white; }

        .status-completed { background: #10b981; color: white; }
        .status-in-progress { background: #3b82f6; color: white; }

        .interactive-badge {
            background: rgba(255, 255, 255, 0.2);
            color: white;
        }

        .lesson-content-container {
            background: white;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }

        .lesson-actions {
            margin-top: 30px;
            padding-top: 20px;
            border-top: 2px solid #e5e7eb;
            display: flex;
            gap: 15px;
            justify-content: center;
        }

        .btn-complete {
            background: #10b981;
            color: white;
            padding: 12px 30px;
            border: none;
            border-radius: 6px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: background 0.3s;
        }

        .btn-complete:hover:not(:disabled) {
            background: #059669;
        }

        .btn-complete:disabled {
            background: #6b7280;
            cursor: not-allowed;
        }

        .complete-message {
            text-align: center;
            color: #10b981;
            font-weight: 600;
            margin-top: 10px;
        }

        /* Quiz Styles */
        .quiz-question {
            background: #f9fafb;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 20px;
            border-left: 4px solid #667eea;
        }

        .quiz-answer-btn {
            display: block;
            width: 100%;
            padding: 12px;
            margin: 8px 0;
            background: white;
            border: 2px solid #e5e7eb;
            border-radius: 6px;
            cursor: pointer;
            text-align: left;
            font-size: 15px;
            transition: all 0.2s;
        }

        .quiz-answer-btn:hover:not(:disabled) {
            border-color: #667eea;
            background: #f3f4f6;
        }

        .quiz-answer-btn:disabled {
            cursor: not-allowed;
        }

        /* Piano Play Button Styles */
        .play-chord-btn {
            background: #667eea;
            color: white;
            padding: 8px 16px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
            margin: 5px;
            transition: background 0.2s;
        }

        .play-chord-btn:hover {
            background: #5568d3;
        }

        /* Error Panel */
        .error-panel {
            background: #fee2e2;
            border-left: 4px solid #ef4444;
            padding: 20px;
            border-radius: 8px;
            color: #991b1b;
        }
    </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Error Panel (shown when lesson not found) -->
    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="error-panel">
        <h3>⚠️ Error Loading Lesson</h3>
        <asp:Literal ID="litErrorMessage" runat="server"></asp:Literal>
        <p style="margin-top: 15px;">
            <a href="LearningCentre.aspx" class="btn">← Back to Learning Centre</a>
        </p>
    </asp:Panel>

    <!-- Lesson Content Panel (shown when lesson loads successfully) -->
    <asp:Panel ID="pnlLessonContent" runat="server" Visible="false">
        <!-- Lesson Header -->
        <div class="lesson-header">
            <!-- Breadcrumb -->
            <div class="breadcrumb">
                <a href="LearningCentre.aspx">Learning Centre</a> 
                / 
                <asp:Literal ID="litCategoryName" runat="server"></asp:Literal>
                / 
                <asp:Literal ID="litLessonTitleBreadcrumb" runat="server"></asp:Literal>
            </div>

            <!-- Lesson Title -->
            <h1 class="lesson-title">
                <asp:Literal ID="litLessonTitle" runat="server"></asp:Literal>
            </h1>

            <!-- Lesson Meta Info -->
            <div class="lesson-meta">
                <span class="meta-badge difficulty-<%= DifficultyLevel.ToLower() %>">
                    <asp:Literal ID="litDifficulty" runat="server"></asp:Literal>
                </span>

                <span class="meta-badge" style="background: rgba(255,255,255,0.2); color: white;">
                    ⏱️ <asp:Literal ID="litEstimatedMinutes" runat="server"></asp:Literal> minutes
                </span>

                <asp:Panel ID="pnlInteractiveBadge" runat="server" Visible="false">
                    <span class="meta-badge interactive-badge">
                        🎹 Interactive
                    </span>
                </asp:Panel>

                <asp:Panel ID="pnlStatusBadge" runat="server" Visible="false">
                    <span class="meta-badge status-<%= LessonStatus.ToLower().Replace(" ", "-") %>">
                        <asp:Literal ID="litStatus" runat="server"></asp:Literal>
                    </span>
                </asp:Panel>
            </div>

            <!-- Lesson Description -->
            <p style="margin-top: 15px; font-size: 16px; line-height: 1.6;">
                <asp:Literal ID="litLessonDescription" runat="server"></asp:Literal>
            </p>
        </div>

        <!-- Main Lesson Content (HTML injected from file) -->
        <div class="lesson-content-container">
            <div class="lesson-content-body" data-lesson-content>
                <asp:Literal ID="litLessonContent" runat="server" Mode="PassThrough"></asp:Literal>
            </div>

            <!-- Lesson Actions -->
            <div class="lesson-actions">
                <asp:Button ID="btnMarkComplete" runat="server" 
                    Text="Mark as Complete" 
                    OnClick="btnMarkComplete_Click" 
                    CssClass="btn-complete" />
                
                <a href="LearningCentre.aspx" class="btn" style="padding: 12px 30px;">
                    Back to Learning Centre
                </a>
            </div>

            <asp:Label ID="lblCompleteMessage" runat="server" 
                CssClass="complete-message" 
                Visible="false"></asp:Label>
        </div>
    </asp:Panel>

    <!-- Hidden Page Title for SEO -->
    <asp:Literal ID="litPageTitle" runat="server" Visible="false"></asp:Literal>
</asp:Content>
