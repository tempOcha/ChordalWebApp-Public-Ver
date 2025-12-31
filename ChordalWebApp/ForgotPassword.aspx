<%@ Page Title="Forgot Password - Chordal" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="ChordalWebApp.ForgotPassword" %>


<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container" style="max-width: 500px; margin-top: 50px; margin-bottom: 50px;">
        <div style="background: white; padding: 50px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
            <h2 style="text-align: center; margin-bottom: 10px;">Forgot Password</h2>
            <p style="text-align: center; color: #666; margin-bottom: 30px;">
                Enter your email address and we'll send you a link to reset your password.
            </p>

            <asp:Label ID="lblStatus" runat="server" Text="" style="display: block; margin-bottom: 15px; text-align: center;"></asp:Label>

            <asp:Panel ID="pnlEmailForm" runat="server">
                <div class="form-group">
                    <label for="txtEmail">Email Address:</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="Enter your registered email"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" 
                        ControlToValidate="txtEmail" 
                        ErrorMessage="Email is required" 
                        CssClass="text-danger" 
                        Display="Dynamic">
                    </asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="revEmail" runat="server" 
                        ControlToValidate="txtEmail" 
                        ErrorMessage="Please enter a valid email address" 
                        ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" 
                        CssClass="text-danger" 
                        Display="Dynamic">
                    </asp:RegularExpressionValidator>
                </div>

                <div class="form-group" style="margin-top: 20px;">
                    <asp:Button ID="btnSubmit" runat="server" Text="Send Reset Link" CssClass="btn" OnClick="btnSubmit_Click" style="width: 100%;" />
                </div>
            </asp:Panel>

            <div style="text-align: center; margin-top: 20px;">
                <p>Remember your password? <a href="Login.aspx">Login here</a></p>
            </div>
        </div>
        <div>

        </div>
    </div>
</asp:Content>

