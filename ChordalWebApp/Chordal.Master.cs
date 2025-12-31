using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChordalWebApp
{
    public partial class Chordal : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check if user is logged in and determine their role
                if (Session["Username"] != null && Session["UserID"] != null)
                {
                    bool isAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];

                    if (isAdmin)
                    {
                        // Show admin navigation only
                        AnonymousUserNav.Visible = false;
                        AuthenticatedUserNav.Visible = false;
                        AdminUserNav.Visible = true;
                    }
                    else
                    {
                        // Show regular user navigation
                        AnonymousUserNav.Visible = false;
                        AuthenticatedUserNav.Visible = true;
                        AdminUserNav.Visible = false;

                        // Display unread notification count for regular users
                        int userId = Convert.ToInt32(Session["UserID"]);
                        int unreadCount = NotificationHelper.GetUnreadCount(userId);

                        if (unreadCount > 0)
                        {
                            lblNotificationCount.Text = unreadCount.ToString();
                            lblNotificationCount.Visible = true;
                        }
                        else
                        {
                            lblNotificationCount.Visible = false;
                        }
                    }
                }
                else
                {
                    // Show anonymous navigation
                    AnonymousUserNav.Visible = true;
                    AuthenticatedUserNav.Visible = false;
                    AdminUserNav.Visible = false;
                }
            }
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            // Regular user logout
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/LandingPage.aspx");
        }

        protected void lnkAdminLogout_Click(object sender, EventArgs e)
        {
            // Admin logout - redirect to admin login page
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/AdminLogin.aspx");
        }
    }
}