<%@ Page Title="Upload Progression" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="UploadProgression.aspx.cs" Inherits="ChordalWebApp.UploadProgression" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Upload Progression - Chordal
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="Styles/upload-styles.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="upload-page-container">
        <!-- Page Header -->
        <div class="upload-page-header">
            <h1 class="upload-page-title">Upload Your Progression</h1>
            <p class="upload-page-subtitle">Share your chord progressions with the community by uploading your exported JSON file</p>
        </div>

        <!-- Upload Box with Wave Animation Canvas -->
        <div class="upload-box-container">
            <canvas id="upload-wave-canvas"></canvas>
            
            <div class="upload-content">
                <!-- Upload Icon -->
                <div class="upload-icon">☁️</div>

                <!-- Instructions -->
                <div class="upload-instructions">
                    <p class="upload-main-text">Drag and Drop files to upload</p>
                    <p class="upload-sub-text">or</p>
                </div>

                <!-- File Input -->
                <div class="file-input-wrapper">
                    <asp:FileUpload ID="jsonFileUpload" runat="server" CssClass="file-input" accept=".json" />
                    <label for="<%= jsonFileUpload.ClientID %>" class="file-input-label">
                        Browse Files
                    </label>
                    <div class="file-name-display" style="display: none;"></div>
                </div>

                <p class="upload-file-types">Supported files: JSON</p>

                <!-- Progress Bar -->
                <div class="upload-progress">
                    <div class="upload-progress-bar"></div>
                </div>

                <!-- Upload Button -->
                <asp:Button ID="btnUploadFile" runat="server" Text="Upload Progression" 
                    OnClick="btnUploadFile_Click" CssClass="btn-upload" />

                <!-- Status Message -->
                <div class="upload-status">
                    <asp:Label ID="lblUploadStatus" runat="server" EnableViewState="false"></asp:Label>
                </div>
            </div>
        </div>

        <!-- JSON Preview (Hidden by default) -->
        <div class="json-preview-section">
            <div class="json-preview-header">
                <h3 class="json-preview-title">File Preview</h3>
            </div>
            <div class="json-preview-content">
                <asp:Literal ID="litJsonContent" runat="server" Visible="false" Mode="PassThrough"></asp:Literal>
            </div>
        </div>

        <!-- Helpful Tips -->
        <div class="upload-tips">
            <h3 class="upload-tips-title">💡 Upload Tips</h3>
            <ul class="upload-tips-list">
                <li>Export your chord progression from the Chordal VST plugin as a JSON file</li>
                <li>Ensure your file includes progression title, key information, and chord data</li>
                <li>Maximum file size: 5MB</li>
                <li>After successful upload, you can add additional details and share with the community</li>
            </ul>
        </div>
    </div>

    <!-- Scripts -->
   
    <script src="Scripts/animations-upload.js"></script>

    <script type="text/javascript">
        // Called from code-behind after successful upload
        function showUploadSuccess() {
            const statusDiv = document.querySelector('.upload-status');
            if (statusDiv) {
                statusDiv.classList.add('visible', 'success');
            }
            
            // Trigger wave animation
            triggerUploadSuccessAnimation();
        }

        function showUploadError(message) {
            const statusDiv = document.querySelector('.upload-status');
            if (statusDiv) {
                statusDiv.classList.add('visible', 'error');
            }
        }

        function showUploadInfo(message) {
            const statusDiv = document.querySelector('.upload-status');
            if (statusDiv) {
                statusDiv.classList.add('visible', 'info');
            }
        }
    </script>
</asp:Content>
