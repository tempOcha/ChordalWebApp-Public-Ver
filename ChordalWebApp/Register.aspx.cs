using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChordalWebApp
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Redirect if already logged in
                if (Session["Username"] != null)
                {
                    Response.Redirect("~/LandingPage.aspx");
                }

                // Hide status panel initially
                pnlStatus.Visible = false;
            }
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                ShowError("Please correct the errors on the page.");
                return;
            }

            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Check if username already exists
                    string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    using (SqlCommand checkUserCmd = new SqlCommand(checkUserQuery, conn))
                    {
                        checkUserCmd.Parameters.AddWithValue("@Username", username);
                        int userCount = (int)checkUserCmd.ExecuteScalar();
                        if (userCount > 0)
                        {
                            ShowError("Username already exists. Please choose another.");
                            return;
                        }
                    }

                    // Check if email already exists
                    string checkEmailQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                    using (SqlCommand checkEmailCmd = new SqlCommand(checkEmailQuery, conn))
                    {
                        checkEmailCmd.Parameters.AddWithValue("@Email", email);
                        int emailCount = (int)checkEmailCmd.ExecuteScalar();
                        if (emailCount > 0)
                        {
                            ShowError("Email already registered. Please use a different email or login.");
                            return;
                        }
                    }

                    // Hash the password using PBKDF2
                    PBKDF2Hash pbkdf2 = new PBKDF2Hash(password);
                    string passwordHashToStore = pbkdf2.HashedPassword;

                    // Insert new user into database
                    string insertQuery = "INSERT INTO Users (Username, Email, PasswordHash) VALUES (@Username, @Email, @PasswordHash); SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@PasswordHash", passwordHashToStore);

                        // Get the new UserID
                        int newUserId = Convert.ToInt32(cmd.ExecuteScalar());

                        // SUCCESS - Auto-login the user
                        Session["Username"] = username;
                        Session["UserID"] = newUserId;

                        // Optional: Log notification for new user
                        try
                        {
                            NotificationHelper.LogNotification(
                                newUserId,
                                "Welcome to Chordal!",
                                "Your account has been created successfully. Start exploring chord progressions!",
                                conn
                            );
                        }
                        catch (Exception notifEx)
                        {
                            // Don't fail registration if notification fails
                            System.Diagnostics.Trace.WriteLine("Notification Error: " + notifEx.Message);
                        }

                        // Trigger wave animation and redirect to landing page
                        TriggerSuccessAnimation("~/LandingPage.aspx");
                    }
                }
                catch (SqlException ex)
                {
                    ShowError("Database error. Please try again.");
                    System.Diagnostics.Trace.WriteLine("SQL Error in Register: " + ex.ToString());
                }
                catch (Exception ex)
                {
                    ShowError("An error occurred. Please try again.");
                    System.Diagnostics.Trace.WriteLine("General Error in Register: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Shows an error message to the user
        /// </summary>
        private void ShowError(string message)
        {
            pnlStatus.Visible = true;
            pnlStatus.CssClass = "auth-status-message auth-status-error";
            lblStatus.Text = message;
        }

        /// <summary>
        /// Shows a success message to the user
        /// </summary>
        private void ShowSuccess(string message)
        {
            pnlStatus.Visible = true;
            pnlStatus.CssClass = "auth-status-message auth-status-success";
            lblStatus.Text = message;
        }

        /// <summary>
        /// Triggers the wave animation and redirects to the specified URL
        /// </summary>
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
                "RegisterSuccess",
                script,
                false
            );
        }
    }
}