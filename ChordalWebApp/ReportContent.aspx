<%@ Page Title="Report Content" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="ReportContent.aspx.cs" Inherits="ChordalWebApp.ReportContent" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Report Content - Chordal
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .report-container {
            max-width: 700px;
            margin: 40px auto;
            background: white;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        
        .report-header {
            margin-bottom: 30px;
            padding-bottom: 20px;
            border-bottom: 2px solid #f0f0f0;
        }
        
        .report-header h1 {
            color: #dc3545;
            margin: 0 0 10px 0;
        }
        
        .report-header p {
            color: #666;
            margin: 0;
            line-height: 1.6;
        }
        
        .content-preview {
            background: #f8f9fa;
            padding: 15px;
            border-radius: 5px;
            margin-bottom: 25px;
            border-left: 4px solid #dc3545;
        }
        
        .content-preview h3 {
            margin: 0 0 10px 0;
            color: #333;
            font-size: 16px;
        }
        
        .content-preview p {
            margin: 5px 0;
            color: #555;
            font-size: 14px;
        }
        
        .form-section {
            margin-bottom: 25px;
        }
        
        .form-section label {
            display: block;
            font-weight: 600;
            margin-bottom: 8px;
            color: #333;
        }
        
        .violation-options {
            display: grid;
            gap: 10px;
        }
        
        .violation-option {
            padding: 12px 15px;
            border: 2px solid #ddd;
            border-radius: 6px;
            cursor: pointer;
            transition: all 0.2s;
            display: flex;
            align-items: flex-start;
            gap: 10px;
        }
        
        .violation-option:hover {
            border-color: #dc3545;
            background: #fff5f5;
        }
        
        .violation-option input[type="radio"] {
            margin-top: 3px;
        }
        
        .violation-option.selected {
            border-color: #dc3545;
            background: #fff5f5;
        }
        
        .violation-label {
            flex: 1;
        }
        
        .violation-title {
            font-weight: 600;
            color: #333;
            margin-bottom: 3px;
        }
        
        .violation-description {
            font-size: 13px;
            color: #666;
        }
        
        .guidelines-box {
            background: #fff3cd;
            border: 1px solid #ffc107;
            border-radius: 5px;
            padding: 15px;
            margin: 20px 0;
        }
        
        .guidelines-box h4 {
            margin: 0 0 10px 0;
            color: #856404;
        }
        
        .guidelines-box ul {
            margin: 0;
            padding-left: 20px;
            color: #856404;
        }
        
        .guidelines-box li {
            margin: 5px 0;
        }
        
        .btn-group {
            display: flex;
            gap: 10px;
            margin-top: 25px;
        }
        
        .btn-danger {
            background: #dc3545;
            color: white;
            border: none;
            padding: 12px 24px;
            border-radius: 5px;
            cursor: pointer;
            font-size: 15px;
            transition: background 0.2s;
        }
        
        .btn-danger:hover {
            background: #c82333;
        }
        
        .btn-secondary {
            background: #6c757d;
            color: white;
            border: none;
            padding: 12px 24px;
            border-radius: 5px;
            cursor: pointer;
            font-size: 15px;
            text-decoration: none;
            display: inline-block;
            transition: background 0.2s;
        }
        
        .btn-secondary:hover {
            background: #5a6268;
        }
        
        .alert {
            padding: 12px 15px;
            margin-bottom: 20px;
            border-radius: 5px;
        }
        
        .alert-success {
            background: #d4edda;
            border: 1px solid #c3e6cb;
            color: #155724;
        }
        
        .alert-error {
            background: #f8d7da;
            border: 1px solid #f5c6cb;
            color: #721c24;
        }
        
        .char-counter {
            float: right;
            color: #999;
            font-size: 13px;
        }
        
        .success-container {
            text-align: center;
            padding: 40px 20px;
        }
        
        .success-icon {
            font-size: 64px;
            color: #28a745;
            margin-bottom: 20px;
        }
        
        .success-container h2 {
            color: #28a745;
            margin-bottom: 15px;
        }
        
        .success-container p {
            color: #666;
            margin-bottom: 25px;
            line-height: 1.6;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="report-container">
        <asp:Panel ID="pnlReportForm" runat="server" Visible="true">
            <div class="report-header">
                <h1>🚩 Report Content</h1>
                <p>Help us maintain a positive community by reporting content that violates our guidelines. All reports are reviewed by our moderation team.</p>
            </div>
            
            <asp:Panel ID="pnlMessages" runat="server" Visible="false">
                <asp:Literal ID="litMessage" runat="server"></asp:Literal>
            </asp:Panel>
            
            <!-- Content Preview -->
            <div class="content-preview">
                <h3>Content Being Reported:</h3>
                <p><strong>Type:</strong> <asp:Literal ID="litContentType" runat="server"></asp:Literal></p>
                <p><strong>Content:</strong> <asp:Literal ID="litContentPreview" runat="server"></asp:Literal></p>
            </div>
            
            <!-- Violation Type Selection -->
            <div class="form-section">
                <label>Why are you reporting this content? *</label>
                <asp:RadioButtonList ID="rblViolationType" runat="server" CssClass="violation-options">
                    <asp:ListItem Value="Inappropriate">
                        <div class="violation-label">
                            <div class="violation-title">Inappropriate Content</div>
                            <div class="violation-description">Contains offensive, abusive, or explicit material</div>
                        </div>
                    </asp:ListItem>
                    <asp:ListItem Value="Copyright">
                        <div class="violation-label">
                            <div class="violation-title">Copyright Infringement</div>
                            <div class="violation-description">Unauthorized use of copyrighted material</div>
                        </div>
                    </asp:ListItem>
                    <asp:ListItem Value="Spam">
                        <div class="violation-label">
                            <div class="violation-title">Spam or Advertising</div>
                            <div class="violation-description">Unsolicited promotional content or repetitive posts</div>
                        </div>
                    </asp:ListItem>
                    <asp:ListItem Value="Harassment">
                        <div class="violation-label">
                            <div class="violation-title">Harassment or Bullying</div>
                            <div class="violation-description">Content that targets or attacks other users</div>
                        </div>
                    </asp:ListItem>
                    <asp:ListItem Value="Misleading">
                        <div class="violation-label">
                            <div class="violation-title">Misleading Information</div>
                            <div class="violation-description">False or deceptive content</div>
                        </div>
                    </asp:ListItem>
                    <asp:ListItem Value="Other">
                        <div class="violation-label">
                            <div class="violation-title">Other Violation</div>
                            <div class="violation-description">Other policy violations not listed above</div>
                        </div>
                    </asp:ListItem>
                </asp:RadioButtonList>
                <asp:RequiredFieldValidator ID="rfvViolationType" runat="server" 
                                          ControlToValidate="rblViolationType"
                                          ErrorMessage="Please select a violation type" 
                                          CssClass="text-danger" Display="Dynamic"></asp:RequiredFieldValidator>
            </div>
            
            <!-- Additional Details -->
            <div class="form-section">
                <label for="txtReportDetails">
                    Additional Details *
                    <span class="char-counter">
                        <asp:Literal ID="litCharCount" runat="server">0/1000</asp:Literal>
                    </span>
                </label>
                <asp:TextBox ID="txtReportDetails" runat="server" TextMode="MultiLine" 
                             Rows="5" CssClass="form-control" MaxLength="1000"
                             placeholder="Please provide specific details about why this content violates our guidelines..."
                             onkeyup="updateCharCount(this, 1000)"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvReportDetails" runat="server" 
                                          ControlToValidate="txtReportDetails"
                                          ErrorMessage="Please provide details about the violation" 
                                          CssClass="text-danger" Display="Dynamic"></asp:RequiredFieldValidator>
            </div>
            
            <!-- Community Guidelines -->
            <div class="guidelines-box">
                <h4>📋 Community Guidelines Reminder</h4>
                <ul>
                    <li>All content should be respectful and constructive</li>
                    <li>No spam, advertising, or self-promotion</li>
                    <li>Respect intellectual property and copyright</li>
                    <li>No harassment, hate speech, or discriminatory content</li>
                    <li>Keep content relevant to music theory and chord progressions</li>
                </ul>
            </div>
            
            <!-- Submit Buttons -->
            <div class="btn-group">
                <asp:Button ID="btnSubmitReport" runat="server" Text="Submit Report" 
                           CssClass="btn-danger" OnClick="btnSubmitReport_Click" />
                <asp:HyperLink ID="lnkCancel" runat="server" CssClass="btn-secondary">Cancel</asp:HyperLink>
            </div>
            
            <asp:HiddenField ID="hdnContentType" runat="server" />
            <asp:HiddenField ID="hdnContentID" runat="server" />
        </asp:Panel>
        
        <!-- Success Message -->
        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <div class="success-container">
                <div class="success-icon">✓</div>
                <h2>Report Submitted Successfully</h2>
                <p>
                    Thank you for helping us maintain a positive community. Your report has been received 
                    and will be reviewed by our moderation team within 24-48 hours.
                </p>
                <p>
                    You will be notified once the review is complete. You can track your reports 
                    in your notifications.
                </p>
                <asp:HyperLink ID="lnkBackToCommunity" runat="server" NavigateUrl="~/CommunityBrowse.aspx" 
                              CssClass="btn btn-primary" style="margin-top: 20px;">
                    Return to Community
                </asp:HyperLink>
            </div>
        </asp:Panel>
    </div>
    
    <script type="text/javascript">
        function updateCharCount(textbox, maxLength) {
            var currentLength = textbox.value.length;
            var counter = document.getElementById('<%= litCharCount.ClientID %>');
            if (counter) {
                counter.innerHTML = currentLength + '/' + maxLength;
            }
        }
    </script>
</asp:Content>
