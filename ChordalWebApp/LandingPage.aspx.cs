using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChordalWebApp
{
    public partial class LandingPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["Username"] != null)
                {
                    LandingPageAnonymousLinks.Visible = false;
                    LandingPageAuthenticatedLinks.Visible = true;
                }
                else
                {
                    LandingPageAnonymousLinks.Visible = true;
                    LandingPageAuthenticatedLinks.Visible = false;
                }
            }
        }
    }
}