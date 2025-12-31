<%@ Page Title="" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="Notifications.aspx.cs" Inherits="ChordalWebApp.Notifications" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .notification-item {
            background: white;
            padding: 15px;
            margin-bottom: 10px;
            border-radius: 5px;
            border-left: 4px solid var(--color-teal);
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }
        
        .notification-item.unread {
            background: var(--color-soft-green);
            font-weight: 500;
        }
        
        .notification-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 8px;
        }
        
        .notification-type {
            color: var(--color-teal);
            font-size: 12px;
            font-weight: bold;
            text-transform: uppercase;
        }
        
        .notification-date {
            color: #999;
            font-size: 12px;
        }
        
        .notification-message {
            color: #333;
            line-height: 1.5;
        }
        
        .notification-actions {
            margin-top: 10px;
            text-align: right;
        }
        
        .mark-read-btn {
            background: none;
            border: none;
            color:var(--color-teal);
            font-size: 12px;
            cursor: pointer;
            text-decoration: underline;
        }
        
        .mark-read-btn:hover {
            color: #5588dd;
        }
        
        .no-notifications {
            text-align: center;
            padding: 40px;
            color: #666;
        }
    </style>
    <link href="Styles/guide-styles.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

     <div class="guidelines-header-section" style="padding-bottom: 10px;">
    <div id="guidelines-particles-canvas"></div>
    <div class="guidelines-header-content">
        <h1 class="guidelines-header-title">Notification</h1>
        <p class="guidelines-header-subtitle">Stay informed on everything that matters, alerts, updates, and the latest news from our team!</p>
    </div>
</div>


    <div class="container" style="max-width: 800px; margin-top: 30px;">
        <div style="margin: 20px 0; display: flex; justify-content: space-between; align-items: center;">
            <div>
                <asp:LinkButton ID="lnkShowAll" runat="server" OnClick="lnkShowAll_Click" Text="All" CssClass="btn" style=" margin-right: 10px;" />
                <asp:LinkButton ID="lnkShowUnread" runat="server" OnClick="lnkShowUnread_Click" Text="Unread Only" CssClass="btn" style="margin-right:10px;" />
            </div>
            <asp:LinkButton ID="lnkMarkAllRead" runat="server" OnClick="lnkMarkAllRead_Click" Text="Mark All as Read" CssClass="btn" />
        </div>

        <asp:PlaceHolder ID="phNoNotifications" runat="server" Visible="false">
            <div class="no-notifications">
                <p style="font-size: 18px;"></p>
                <p>You have no notifications.</p>
            </div>
        </asp:PlaceHolder>

        <asp:Repeater ID="rptNotifications" runat="server" OnItemCommand="rptNotifications_ItemCommand">
            <ItemTemplate>
                <div class='notification-item <%# Convert.ToBoolean(Eval("IsRead")) ? "" : "unread" %>'>
                    <div class="notification-header">
                        <span class="notification-type"><%# Eval("NotificationType") %></span>
                        <span class="notification-date"><%# Convert.ToDateTime(Eval("CreatedDate")).ToString("MMM dd, yyyy hh:mm tt") %></span>
                    </div>
                    <div class="notification-message">
                        <%# Server.HtmlEncode(Eval("Message").ToString()) %>
                    </div>
                    <%# !Convert.ToBoolean(Eval("IsRead")) ? 
                        "<div class='notification-actions'><asp:LinkButton runat='server' CommandName='MarkRead' CommandArgument='" + Eval("NotificationID") + "' CssClass='mark-read-btn' Text='Mark as Read' /></div>" : "" %>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <asp:Label ID="lblStatus" runat="server" Text="" CssClass="text-success" style="display: block; margin-top: 20px; text-align: center;"></asp:Label>
    </div>

     <script src="https://cdnjs.cloudflare.com/ajax/libs/p5.js/1.4.0/p5.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/animejs/3.2.1/anime.min.js"></script>
    <script src="Scripts/animations-guide.js"></script>
</asp:Content>
