<%@ Page Title="Share Progression" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="ShareProgression.aspx.cs" Inherits="ChordalWebApp.ShareProgression" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Share Progression - Chordal
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Share Progression Styles -->
    <link href="/Styles/share-styles.css" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Share Header with Wave Animation -->
    <div class="share-header-section">
        <canvas id="share-wave-canvas"></canvas>
        <div class="container">
            <div class="share-header-content">
                <h1 class="share-header-title">Share Your Progression</h1>
                <p class="share-header-subtitle">Share your musical creation with the Chordal community</p>
            </div>
        </div>
    </div>
    
    <div class="container">
        <div class="share-container">
            <!-- Status Messages -->
            <asp:Panel ID="pnlMessages" runat="server" Visible="false">
                <asp:Literal ID="litMessage" runat="server"></asp:Literal>
            </asp:Panel>
            
            <!-- Progression Preview -->
            <div class="progression-preview">
                <h3><asp:Literal ID="litProgressionTitle" runat="server"></asp:Literal></h3>
                <div class="preview-detail">
                    <strong>Key:</strong> <asp:Literal ID="litKey" runat="server"></asp:Literal>
                </div>
                <div class="preview-detail">
                    <strong>Chords:</strong> <asp:Literal ID="litChordCount" runat="server"></asp:Literal> chords
                </div>
                <div class="preview-detail">
                    <strong>Uploaded:</strong> <asp:Literal ID="litUploadDate" runat="server"></asp:Literal>
                </div>
            </div>
            
            <!-- Sharing Details Section -->
            <div class="form-section">
                <h3>Sharing Details</h3>
                
                <div class="form-group">
                    <label for="<%= txtShareTitle.ClientID %>">
                        Share Title *
                        <span class="char-counter" id="titleCounter">0/255</span>
                    </label>
                    <asp:TextBox ID="txtShareTitle" runat="server" CssClass="form-control" 
                                 MaxLength="255" placeholder="Enter a descriptive title for your shared progression"
                                 onkeyup="updateCharCount(this, 255, 'titleCounter')"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvShareTitle" runat="server" 
                                              ControlToValidate="txtShareTitle"
                                              ErrorMessage="Share title is required" 
                                              CssClass="text-danger" Display="Dynamic"></asp:RequiredFieldValidator>
                </div>
                
                <div class="form-group">
                    <label for="<%= txtShareDescription.ClientID %>">
                        Description
                        <span class="char-counter" id="descCounter">0/1000</span>
                    </label>
                    <asp:TextBox ID="txtShareDescription" runat="server" TextMode="MultiLine" 
                                 Rows="4" CssClass="form-control" MaxLength="1000"
                                 placeholder="Describe your progression, its inspiration, or usage tips..."
                                 onkeyup="updateCharCount(this, 1000, 'descCounter')"></asp:TextBox>
                    <div class="info-text">Help others understand and use your progression effectively</div>
                </div>
                
                <div class="form-group">
                    <label for="<%= txtTags.ClientID %>">
                        Tags (comma-separated)
                        <span class="char-counter" id="tagsCounter">0/500</span>
                    </label>
                    <div class="tag-input-container">
                        <asp:TextBox ID="txtTags" runat="server" CssClass="form-control" 
                                     MaxLength="500" placeholder="e.g., Jazz, Blues, Am, Lick"
                                     onkeyup="updateCharCount(this, 500, 'tagsCounter')"></asp:TextBox>
                    </div>
                    <div class="info-text">Add tags to help others find your progression (e.g., genre, mood, key)</div>
                </div>
            </div>
            
            <!-- Sharing Permissions Section -->
            <div class="form-section">
                <h3>Sharing Permissions</h3>
                
                <div class="permission-options">
                    <div class="permission-option">
                        <asp:RadioButton ID="rbPublic" runat="server" GroupName="Visibility" 
                                       Text="" Checked="true" />
                        <strong>Public</strong>
                        <div class="info-text">Everyone can view this progression, including non-registered users</div>
                    </div>
                    
                    <div class="permission-option">
                        <asp:RadioButton ID="rbRegisteredOnly" runat="server" GroupName="Visibility" 
                                       Text="" />
                        <strong>Registered Users Only</strong>
                        <div class="info-text">Only registered and logged-in users can view this progression</div>
                    </div>
                </div>
                
                <div class="checkbox-option">
                    <asp:CheckBox ID="chkAllowDownload" runat="server" Checked="true" />
                    <strong>Allow others to download this progression</strong>
                    <div class="info-text">Users will be able to download the JSON file of your progression</div>
                </div>
            </div>
            
            <!-- Action Buttons -->
            <div class="btn-group-share">
                <asp:Button ID="btnShare" runat="server" Text="🌐 Share Progression" 
                           CssClass="btn-share-primary" OnClick="btnShare_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" 
                           CssClass="btn-share-secondary" OnClick="btnCancel_Click" 
                           CausesValidation="false" />
            </div>
            
            <asp:HiddenField ID="hdnProgressionID" runat="server" />
        </div>
    </div>
    
    <!-- Animation Scripts -->
    <script src="/Scripts/animations-share.js"></script>
    
    <script type="text/javascript">
        // Initialize character counters on page load
        window.addEventListener('DOMContentLoaded', function() {
            var titleInput = document.getElementById('<%= txtShareTitle.ClientID %>');
            var descInput = document.getElementById('<%= txtShareDescription.ClientID %>');
            var tagsInput = document.getElementById('<%= txtTags.ClientID %>');

            if (titleInput && titleInput.value) {
                updateCharCount(titleInput, 255, 'titleCounter');
            }
            if (descInput && descInput.value) {
                updateCharCount(descInput, 1000, 'descCounter');
            }
            if (tagsInput && tagsInput.value) {
                updateCharCount(tagsInput, 500, 'tagsCounter');
            }
        });
    </script>
</asp:Content>
