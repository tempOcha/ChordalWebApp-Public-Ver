<%@ Page Title="" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="UpdateProfile.aspx.cs" Inherits="ChordalWebApp.UpdateProfile" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
     <style>
        .profile-section {
            background: white;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            max-width: 600px;
            margin: 0 auto;
        }
        .profile-header {
            text-align: center;
            margin-bottom: 30px;
            padding-bottom: 20px;
            border-bottom: 2px solid #77aaff;
        }
        .form-section {
            margin-bottom: 25px;
        }
        .form-section-title {
            font-size: 18px;
            font-weight: bold;
            color: #333;
            margin-bottom: 15px;
            padding-bottom: 10px;
            border-bottom: 1px solid #ddd;
        }
        .info-message {
            background-color: #e7f3ff;
            border-left: 4px solid #77aaff;
            padding: 12px;
            margin-bottom: 20px;
            border-radius: 4px;
        }
        .btn-save {
            background-color: #77aaff;
            color: white;
            padding: 12px 30px;
            border: none;
            border-radius: 5px;
            font-size: 16px;
            cursor: pointer;
            width: 100%;
        }
        .btn-save:hover {
            background-color: #5588dd;
        }
        .btn-cancel {
            background-color: #6c757d;
            color: white;
            padding: 12px 30px;
            border: none;
            border-radius: 5px;
            font-size: 16px;
            cursor: pointer;
            width: 100%;
            margin-top: 10px;
        }
        .btn-cancel:hover {
            background-color: #5a6268;
        }
    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container" style="margin-top: 40px; margin-bottom: 40px;">
        <div class="profile-section">
            <div class="profile-header">
                <h2>Update Your Profile</h2>
                <p style="color: #666;">Manage your account information and preferences</p>
            </div>

            <asp:Label ID="lblStatus" runat="server" Text="" style="display: block; margin-bottom: 15px;"></asp:Label>

            <div class="info-message">
                <strong>Note:</strong> Changes to your email or password will trigger a security notification.
            </div>

            <!-- Account Information Section -->
            <div class="form-section">
                <div class="form-section-title">Account Information</div>
                
                <div class="form-group">
                    <label for="txtUsername">Username</label>
                    <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" placeholder="Enter new username"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvUsername" runat="server" 
                        ControlToValidate="txtUsername" 
                        ErrorMessage="Username is required" 
                        CssClass="text-danger" 
                        Display="Dynamic"
                        ValidationGroup="UpdateProfile">
                    </asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="revUsername" runat="server" 
                        ControlToValidate="txtUsername"
                        ValidationExpression="^[a-zA-Z0-9_]{3,20}$"
                        ErrorMessage="Username must be 3-20 characters (letters, numbers, underscore only)"
                        CssClass="text-danger"
                        Display="Dynamic"
                        ValidationGroup="UpdateProfile">
                    </asp:RegularExpressionValidator>
                </div>

                <div class="form-group">
                    <label for="txtEmail">Email Address</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="Enter new email address"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" 
                        ControlToValidate="txtEmail" 
                        ErrorMessage="Email is required" 
                        CssClass="text-danger" 
                        Display="Dynamic"
                        ValidationGroup="UpdateProfile">
                    </asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="revEmail" runat="server" 
                        ControlToValidate="txtEmail"
                        ValidationExpression="^[\w\.-]+@[\w\.-]+\.\w+$"
                        ErrorMessage="Please enter a valid email address"
                        CssClass="text-danger"
                        Display="Dynamic"
                        ValidationGroup="UpdateProfile">
                    </asp:RegularExpressionValidator>
                </div>
            </div>

            <!-- Password Change Section -->
            <div class="form-section">
                <div class="form-section-title">Change Password (Optional)</div>
                <p style="color: #666; font-size: 14px; margin-bottom: 15px;">Leave blank if you don't want to change your password</p>
                
                <div class="form-group">
                    <label for="txtCurrentPassword">Current Password</label>
                    <asp:TextBox ID="txtCurrentPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Enter current password"></asp:TextBox>
                </div>

                <div class="form-group">
                    <label for="txtNewPassword">New Password</label>
                    <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Enter new password"></asp:TextBox>
                    <asp:RegularExpressionValidator ID="revNewPassword" runat="server" 
                        ControlToValidate="txtNewPassword"
                        ValidationExpression="^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*#?&]{8,}$"
                        ErrorMessage="Password must be at least 8 characters with letters and numbers"
                        CssClass="text-danger"
                        Display="Dynamic"
                        ValidationGroup="UpdateProfile"
                        Enabled="false">
                    </asp:RegularExpressionValidator>
                </div>

                <div class="form-group">
                    <label for="txtConfirmPassword">Confirm New Password</label>
                    <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Confirm new password"></asp:TextBox>
                    <asp:CompareValidator ID="cvPassword" runat="server" 
                        ControlToValidate="txtConfirmPassword"
                        ControlToCompare="txtNewPassword"
                        ErrorMessage="Passwords do not match"
                        CssClass="text-danger"
                        Display="Dynamic"
                        ValidationGroup="UpdateProfile"
                        Enabled="false">
                    </asp:CompareValidator>
                </div>
            </div>

            <!-- Preferences Section -->
            <div class="form-section">
                <div class="form-section-title">Notification Preferences</div>
                
                <div class="form-group">
                    <asp:CheckBox ID="chkEmailNotifications" runat="server" Text=" Send me email notifications" />
                </div>
                
                <div class="form-group">
                    <asp:CheckBox ID="chkCommunityNotifications" runat="server" Text=" Notify me about community interactions" />
                </div>
            </div>

            <!-- Action Buttons -->
            <div class="form-group">
                <asp:Button ID="btnSaveChanges" runat="server" Text="Save Changes" CssClass="btn-save" OnClick="btnSaveChanges_Click" ValidationGroup="UpdateProfile" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-cancel" OnClick="btnCancel_Click" CausesValidation="false" />
            </div>
        </div>
    </div>
</asp:Content>
