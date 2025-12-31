using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChordalWebApp
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Hide status panel initially
            if (!IsPostBack)
            {
                pnlStatus.Visible = false;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsernameLogin.Text.Trim();
            string passwordAttempt = txtPasswordLogin.Text;

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);

            string sql = @"SELECT UserID, PasswordHash, IsEnabled FROM Users WHERE Username = @username";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@username", username);

            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    int userId = Convert.ToInt32(reader["UserID"]);
                    string storedPasswordHash = reader["PasswordHash"].ToString();
                    bool isEnabled = Convert.ToBoolean(reader["IsEnabled"]);

                    reader.Close();

                    if (!isEnabled)
                    {
                        ShowError("Your account is disabled. Please contact support.");
                        return;
                    }

                    bool passwordIsValid;
                    PBKDF2Hash hashVerifier;
                    try
                    {
                        hashVerifier = new PBKDF2Hash(passwordAttempt, storedPasswordHash);
                        passwordIsValid = hashVerifier.PasswordCheck;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("PBKDF2Hash Error: " + ex.Message);
                        passwordIsValid = false;
                    }

                    if (passwordIsValid)
                    {
                        // SUCCESS - Set session
                        Session["Username"] = username;
                        Session["UserID"] = userId;

                        // Handle Remember Me if checked
                        if (chkRememberMe.Checked)
                        {
                            // Optional: Implement remember me cookie
                            // Response.Cookies["RememberMe"].Value = GenerateSecureToken();
                            // Response.Cookies["RememberMe"].Expires = DateTime.Now.AddDays(30);
                        }

                        // Trigger wave animation and redirect to landing page
                        TriggerSuccessAnimation("~/LandingPage.aspx");
                    }
                    else
                    {
                        ShowError("Incorrect password. Please try again.");
                    }
                }
                else // Username not found
                {
                    ShowError("Username not found. Please check your credentials.");
                }
            }
            catch (SqlException ex)
            {
                ShowError("Database error. Please try again later.");
                System.Diagnostics.Trace.WriteLine("SQL Error in Login: " + ex.ToString());
            }
            catch (Exception ex)
            {
                ShowError("An error occurred. Please try again.");
                System.Diagnostics.Trace.WriteLine("General Error in Login: " + ex.ToString());
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        /// Shows an error message to the user
        private void ShowError(string message)
        {
            pnlStatus.Visible = true;
            pnlStatus.CssClass = "auth-status-message auth-status-error";
            lblLoginStatus.Text = message;
        }

        /// Triggers the wave animation and redirects to the specified URL
        private void TriggerSuccessAnimation(string redirectUrl)
        {
            // Resolve the URL to handle ~/ paths
            string resolvedUrl = ResolveUrl(redirectUrl);

            // Register JavaScript to trigger the wave animation
            string script = $@"
                <script type='text/javascript'>
                    window.addEventListener('load', function() {{
                        // Wait a tiny bit for p5.js to initialize
                        setTimeout(function() {{
                            if (typeof handleAuthSuccess !== 'undefined') {{
                                // Trigger wave animation with 2 second duration
                                handleAuthSuccess('{resolvedUrl}', 2000);
                            }} else {{
                                // Fallback if animation script isn't loaded
                                window.location.href = '{resolvedUrl}';
                            }}
                        }}, 100);
                    }});
                </script>
            ";

            // Register the script to run on page load
            ClientScript.RegisterStartupScript(
                this.GetType(),
                "LoginSuccess",
                script,
                false // addScriptTags is false because we're including <script> tags
            );
        }

    }
}