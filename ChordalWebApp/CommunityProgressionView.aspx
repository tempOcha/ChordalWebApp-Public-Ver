<%@ Page Title="View Progression" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="CommunityProgressionView.aspx.cs" Inherits="ChordalWebApp.CommunityProgressionView" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal ID="litPageTitle" runat="server">View Progression</asp:Literal> - Chordal
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Tone.js for audio synthesis -->
    <script src="https://cdn.jsdelivr.net/npm/tone@14.8.49/build/Tone.min.js"></script>
    
    <link rel="stylesheet" href="Styles/comm-styles.css">
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Community Header with Wave Animation -->
    <div class="community-header-section progression-view-header">
        <canvas id="community-wave-canvas"></canvas>
        <div class="container">
            <div class="community-header-content">
  
                <h1 class="community-header-title">
                    <asp:Literal ID="litProgressionTitle" runat="server">Progression Title</asp:Literal>
                </h1>
                <div class="progression-header-meta">
                    <div class="author-section-header">
                        <div class="author-avatar-header">
                            <asp:Literal ID="litAuthorInitial" runat="server">U</asp:Literal>
                        </div>
                        <div>
                            <div class="author-name-header">
                                by <asp:Literal ID="litAuthorName" runat="server">Username</asp:Literal>
                            </div>
                            <div class="share-date-header">
                                <asp:Literal ID="litShareDate" runat="server">Date</asp:Literal>
                            </div>
                            <div class="meta-item-header">
                            <asp:Literal ID="litKey" runat="server">Key</asp:Literal>
                        </div>
                        </div>
                    </div>
                    <div class="header-meta-items">
                        <div class="meta-item-header" runat="server" id="divTempo" visible="false">
                            🎼 <asp:Literal ID="litTempo" runat="server">120</asp:Literal> BPM
                        </div>
                        <div class="meta-item-header" runat="server" id="divCategory" visible="false">
                            📁 <asp:Literal ID="litCategory" runat="server"></asp:Literal>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="container progression-view-container">
        <asp:Panel ID="pnlProgressionContent" runat="server">
            <!-- MIDI Visualization Section -->
            <div class="midi-visualization-section">
                <div class="midi-viz-header">
                    <h2 class="section-title">Chord Progression Visualization</h2>
                    <div class="viz-controls-inline">
                        <button type="button" id="play-button" class="btn-viz-control btn-play" disabled>
                            <span class="btn-icon">▶</span> Play
                        </button>
                        <button type="button" id="stop-button" class="btn-viz-control btn-stop" disabled>
                            <span class="btn-icon">■</span> Stop
                        </button>
                    </div>
                </div>
                
                <!-- MIDI Display Canvas -->
                <div id="midi-visualization-container">
                    <canvas id="progression-canvas"></canvas>
                </div>
                
                <!-- Playback Controls Panel -->
                <div class="playback-controls-panel">
                    <div class="control-group">
                        <label class="control-label">
                            <span class="label-icon"></span>
                            Waveform
                        </label>
                        <select id="waveform-select" class="form-control-viz">
                            <option value="sine">Sine Wave</option>
                            <option value="square">Square Wave</option>
                            <option value="sawtooth">Sawtooth Wave</option>
                            <option value="triangle">Triangle Wave</option>
                        </select>
                    </div>
                    
                    <!--
                    <div class="control-group">
                        <label class="control-label">
                            <span class="label-icon"></span>
                            Tempo: <span id="tempo-value">120</span> BPM
                        </label>
                        <input type="range" id="tempo-slider" class="slider-viz" 
                               min="60" max="200" value="120">
                    </div>
                    -->

                    <div class="control-group">
                        <label class="control-label">
                            <span class="label-icon"></span>
                            Note Color
                        </label>
                        <input type="color" id="note-color-picker" class="color-picker-viz" 
                               value="#177364">
                    </div>
                    
                    <div class="control-group">
                        <label class="control-label">
                            <span class="label-icon"></span>
                            Bar Height: <span id="height-value">1</span>px
                        </label>
                        <div></div>
                        <input type="range" id="bar-height-slider" class="slider-viz" 
                               min="1" max="8" value="1">
                    </div>
                    
                    <div class="control-group">
                        <label class="checkbox-label">
                            <input type="checkbox" id="show-playhead" checked>
                            Show Playhead
                        </label>
                    </div>
                </div>
            </div>

            <!-- Chord Progression Cards Section -->
            <div class="chord-cards-section">
                <h2 class="section-title">Chord Sequence</h2>
                <div class="chord-sequence-grid">
                    <asp:Repeater ID="rptChords" runat="server">
                        <ItemTemplate>
                            <div class="chord-card-modern">
                                <div class="chord-number"><%# Container.ItemIndex + 1 %></div>
                                <div class="chord-name-large"><%# Eval("ChordName") %></div>
                                <div class="chord-roman-modern"><%# Eval("RomanNumeral") %></div>
                                <div class="chord-function-modern"><%# Eval("ChordFunction") %></div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>

            <!-- Description & Rating Section -->
            <div class="description-rating-section">
                <div class="description-column">
                    <div class="content-card">
                        <h2 class="section-title">Description</h2>
                        <div class="description-text">
                            <asp:Literal ID="litDescription" runat="server"></asp:Literal>
                        </div>
                        
                        <asp:Panel ID="pnlTags" runat="server" Visible="false" CssClass="tags-container">
                            <div class="tags-label">Tags:</div>
                            <div class="tags-list">
                                <asp:Literal ID="litTags" runat="server"></asp:Literal>
                            </div>
                        </asp:Panel>
                    </div>
                </div>
                
                <div class="rating-column">
                    <div class="content-card">
                        <h2 class="section-title">Rating</h2>
                        <div class="rating-display-large">
                            <div class="rating-number">
                                <asp:Literal ID="litAverageRating" runat="server">N/A</asp:Literal>
                            </div>
                            <div class="rating-details">
                                <div class="stars-display">
                                    <asp:Literal ID="litStars" runat="server"></asp:Literal>
                                </div>
                                <div class="rating-count-text">
                                    <asp:Literal ID="litRatingCount" runat="server">0</asp:Literal> ratings
                                </div>
                            </div>
                        </div>
                        
                        <asp:Panel ID="pnlUserRating" runat="server" Visible="false" CssClass="user-rating-panel">
                            <div class="rating-prompt">Rate this progression:</div>
                            <div class="star-rating-interactive">
                                <asp:LinkButton ID="lnkStar1" runat="server" CssClass="star-btn" OnClick="RateStar_Click" CommandArgument="1">★</asp:LinkButton>
                                <asp:LinkButton ID="lnkStar2" runat="server" CssClass="star-btn" OnClick="RateStar_Click" CommandArgument="2">★</asp:LinkButton>
                                <asp:LinkButton ID="lnkStar3" runat="server" CssClass="star-btn" OnClick="RateStar_Click" CommandArgument="3">★</asp:LinkButton>
                                <asp:LinkButton ID="lnkStar4" runat="server" CssClass="star-btn" OnClick="RateStar_Click" CommandArgument="4">★</asp:LinkButton>
                                <asp:LinkButton ID="lnkStar5" runat="server" CssClass="star-btn" OnClick="RateStar_Click" CommandArgument="5">★</asp:LinkButton>
                            </div>
                            <asp:Panel ID="pnlCurrentRating" runat="server" Visible="false" CssClass="current-rating-text">
                                Your rating: <strong><asp:Literal ID="litUserRating" runat="server"></asp:Literal></strong> stars
                            </asp:Panel>
                        </asp:Panel>
                        
                        <div class="stats-group">
                            <div class="stat-item-modern">
                                <span class="stat-icon">👁️</span>
                                <span class="stat-value"><asp:Literal ID="litViewCount" runat="server"></asp:Literal></span>
                                <span class="stat-label">views</span>
                            </div>
                            <div class="stat-item-modern">
                                <asp:LinkButton ID="lnkLike" runat="server" OnClick="lnkLike_Click" 
                                               CssClass="stat-item-modern stat-like" CausesValidation="false">
                                    <span class="stat-icon" id="likeIcon" runat="server">🤍</span>
                                    <span class="stat-value"><asp:Literal ID="litLikeCount" runat="server"></asp:Literal></span>
                                    <span class="stat-label">likes</span>
                                </asp:LinkButton>
                            </div>
                            <div class="stat-item-modern">
                                <span class="stat-icon">💬</span>
                                <span class="stat-value"><asp:Literal ID="litCommentCount" runat="server"></asp:Literal></span>
                                <span class="stat-label">comments</span>
                            </div>
                        </div>
                        
                        <div class="action-buttons-modern">
                            <asp:Button ID="btnDownload" runat="server" Text="MIDI Download" 
                                       CssClass="btn-download-modern" OnClick="btnDownload_Click" 
                                       Visible="false" />
                            <asp:LinkButton ID="lnkReport" runat="server" 
                                           CssClass="btn-report-modern" OnClick="lnkReport_Click">
                                Report
                            </asp:LinkButton>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Comments Section -->
            <div class="comments-section-modern">
                <div class="content-card">
                    <h2 class="section-title">
                        Comments (<asp:Literal ID="litCommentCount2" runat="server">0</asp:Literal>)
                    </h2>
                    
                    <asp:Panel ID="pnlComments" runat="server" CssClass="comments-list">
                        <asp:Repeater ID="rptComments" runat="server" OnItemCommand="rptComments_ItemCommand">
                            <ItemTemplate>
                                <div class='comment-item-modern <%# (Eval("ParentCommentID") != DBNull.Value && Convert.ToInt32(Eval("ParentCommentID")) > 0) ? "comment-reply" : "" %>'>
                                    <div class="comment-avatar">
                                        <%# Eval("Username").ToString().Substring(0, 1).ToUpper() %>
                                    </div>
                                    <div class="comment-content">
                                        <div class="comment-header-modern">
                                            <span class="comment-author-modern"><%# Eval("Username") %></span>
                                            <span class="comment-date-modern"><%# Convert.ToDateTime(Eval("CreatedDate")).ToString("MMM dd, yyyy") %></span>
                                        </div>
                                        <div class="comment-text-modern">
                                            <%# Server.HtmlEncode(Eval("CommentText").ToString()).Replace("\n", "<br/>") %>
                                        </div>
                                        <div class="comment-actions-modern" runat="server" visible='<%# Session["UserID"] != null && (Eval("ParentCommentID") == DBNull.Value || Convert.ToInt32(Eval("ParentCommentID")) == 0) %>'>
                                            <asp:LinkButton ID="lnkReply" runat="server" 
                                                           CommandName="Reply" 
                                                           CommandArgument='<%# Eval("CommentID") %>'
                                                           CssClass="comment-action-btn"
                                                           CausesValidation="false">
                                                Reply
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="lnkReportComment" runat="server" 
                                                           OnClick="lnkReportComment_Click"
                                                           CommandArgument='<%# Eval("CommentID") %>'
                                                           CssClass="comment-action-btn comment-report"
                                                           CausesValidation="false">
                                                Report
                                            </asp:LinkButton>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </asp:Panel>
                    
                    <asp:Panel ID="pnlNoComments" runat="server" Visible="false" CssClass="no-comments-message">
                        No comments yet. Be the first to comment!
                    </asp:Panel>
                    
                    <!-- Add Comment Form -->
                    <asp:Panel ID="pnlAddComment" runat="server" Visible="false" CssClass="add-comment-form">
                        <h3 class="add-comment-title">
                            <asp:Literal ID="litCommentTitle" runat="server">Add a Comment</asp:Literal>
                        </h3>
                        <asp:Panel ID="pnlReplyingTo" runat="server" Visible="false" CssClass="replying-to-banner">
                            Replying to: <strong><asp:Literal ID="litReplyToUser" runat="server"></asp:Literal></strong>
                            <asp:LinkButton ID="lnkCancelReply" runat="server" OnClick="lnkCancelReply_Click" 
                                           CausesValidation="false" CssClass="cancel-reply-btn">
                                ✕ Cancel
                            </asp:LinkButton>
                        </asp:Panel>
                        <div class="form-group">
                            <asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" 
                                        Rows="4" CssClass="form-control-modern" 
                                        placeholder="Share your thoughts about this progression..."
                                        MaxLength="1000"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvComment" runat="server" 
                                                      ControlToValidate="txtComment"
                                                      ErrorMessage="Comment text is required" 
                                                      CssClass="validation-error" Display="Dynamic"
                                                      ValidationGroup="AddComment"></asp:RequiredFieldValidator>
                        </div>
                        <asp:Button ID="btnPostComment" runat="server" Text="Post Comment" 
                                   CssClass="btn-post-comment" OnClick="btnPostComment_Click"
                                   ValidationGroup="AddComment" />
                        <asp:HiddenField ID="hdnReplyToCommentID" runat="server" Value="0" />
                    </asp:Panel>
                    
                    <asp:Panel ID="pnlLoginToComment" runat="server" Visible="false" CssClass="login-to-comment">
                        <a href="Login.aspx?returnUrl=<%=Server.UrlEncode(Request.RawUrl) %>" class="btn-login-comment">
                            Login to Comment
                        </a>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>
        
        <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="error-panel">
            <h2>Progression Not Found</h2>
            <p>The shared progression you're looking for doesn't exist or has been removed.</p>
            <a href="CommunityBrowse.aspx" class="btn btn-primary">Browse Community</a>
        </asp:Panel>
        
        <asp:HiddenField ID="hdnSharedProgressionID" runat="server" />
        <asp:HiddenField ID="hdnProgressionJSON" runat="server" />
    </div>
    
    <!-- Debug Script - Check data immediately on page load -->
    <script type="text/javascript">
        console.log('=== PAGE LOAD DEBUG ===');
        console.log('DOM Ready State:', document.readyState);

        // Store the ClientID in a global variable for the external JS to use
        window.chordalProgressionJSONFieldID = '<%= hdnProgressionJSON.ClientID %>';

        // Check immediately if hidden field exists
        window.addEventListener('DOMContentLoaded', function () {
            console.log('=== DOM CONTENT LOADED ===');
            const jsonField = document.getElementById('<%= hdnProgressionJSON.ClientID %>');
            console.log('Hidden field found:', jsonField !== null);
            console.log('Hidden field ID:', '<%= hdnProgressionJSON.ClientID %>');
            if (jsonField) {
                console.log('Hidden field value length:', jsonField.value.length);
                console.log('Hidden field raw value:', jsonField.value);

                if (jsonField.value) {
                    try {
                        const data = JSON.parse(jsonField.value);
                        console.log('✓ JSON is valid!');
                        console.log('✓ Number of chords:', data.length);
                        console.log('✓ First chord:', data[0]);
                    } catch (e) {
                        console.error('✗ JSON parse error:', e);
                    }
                } else {
                    console.warn('⚠ Hidden field is EMPTY');
                }
            } else {
                console.error('✗ Hidden field NOT FOUND in DOM');
            }
        });
    </script>
    
    <script src="Scripts/animations-comm.js"></script>
</asp:Content>
