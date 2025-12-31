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
    public partial class AdminLogin : System.Web.UI.Page
    {
        private int failedAttempts = 0;
        private const int MAX_FAILED_ATTEMPTS = 5;
        private const int LOCKOUT_DURATION_MINUTES = 15;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check if already logged in as admin
                if (Session["IsAdmin"] != null && (bool)Session["IsAdmin"] == true)
                {
                    Response.Redirect("~/AdminDashboard.aspx");
                }

                // Check if account is locked out
                if (Session["AdminLockoutTime"] != null)
                {
                    DateTime lockoutTime = (DateTime)Session["AdminLockoutTime"];
                    if (DateTime.Now < lockoutTime)
                    {
                        int remainingMinutes = (int)(lockoutTime - DateTime.Now).TotalMinutes + 1;
                        ShowError($"Account temporarily locked due to multiple failed attempts. Try again in {remainingMinutes} minutes.");
                        btnAdminLogin.Enabled = false;
                        return;
                    }
                    else
                    {
                        Session.Remove("AdminLockoutTime");
                        Session.Remove("AdminFailedAttempts");
                    }
                }

                // Update expiration timer if in 2FA step
                if (pnl2FA.Visible && Session["2FAExpiration"] != null)
                {
                    UpdateExpirationDisplay();
                }
            }
        }

        protected void btnAdminLogin_Click(object sender, EventArgs e)
        {
            // Check lockout
            if (Session["AdminLockoutTime"] != null)
            {
                DateTime lockoutTime = (DateTime)Session["AdminLockoutTime"];
                if (DateTime.Now < lockoutTime)
                {
                    int remainingMinutes = (int)(lockoutTime - DateTime.Now).TotalMinutes + 1;
                    ShowError($"Account temporarily locked. Try again in {remainingMinutes} minutes.");
                    return;
                }
            }

            string username = txtAdminUsername.Text.Trim();
            string passwordAttempt = txtAdminPassword.Text;

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Validate admin credentials
                    using (SqlCommand cmd = new SqlCommand("sp_ValidateAdminCredentials", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = Convert.ToInt32(reader["UserID"]);
                                string storedPasswordHash = reader["PasswordHash"].ToString();
                                bool isEnabled = Convert.ToBoolean(reader["IsEnabled"]);
                                bool isAdmin = Convert.ToBoolean(reader["IsAdmin"]);
                                string email = reader["Email"].ToString();

                                reader.Close();

                                if (!isEnabled || !isAdmin)
                                {
                                    ShowError("Invalid admin credentials.");
                                    IncrementFailedAttempts();
                                    return;
                                }

                                // Verify password
                                bool passwordIsValid = false;
                                try
                                {
                                    PBKDF2Hash hashVerifier = new PBKDF2Hash(passwordAttempt, storedPasswordHash);
                                    passwordIsValid = hashVerifier.PasswordCheck;
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine("PBKDF2Hash Error: " + ex.Message);
                                    passwordIsValid = false;
                                }

                                if (passwordIsValid)
                                {
                                    // Clear failed attempts
                                    Session.Remove("AdminFailedAttempts");
                                    Session.Remove("AdminLockoutTime");

                                    // Generate 2FA code
                                    string code = Generate2FACode(userId, conn);

                                    // Send 2FA code via email (simulated)
                                    Send2FACodeEmail(email, username, code, userId);

                                    // Store temporary session data
                                    Session["2FA_UserID"] = userId;
                                    Session["2FA_Username"] = username;
                                    Session["2FA_Email"] = email;
                                    Session["2FAExpiration"] = DateTime.Now.AddMinutes(5);

                                    // Show 2FA panel
                                    pnlCredentials.Visible = false;
                                    pnl2FA.Visible = true;
                                    UpdateExpirationDisplay();

                                    lblAdminLoginStatus.Text = "";
                                }
                                else
                                {
                                    ShowError("Invalid login credentials.");
                                    IncrementFailedAttempts();
                                }
                            }
                            else
                            {
                                ShowError("Invalid login credentials.");
                                IncrementFailedAttempts();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError("An error occurred during login. Please try again.");
                    System.Diagnostics.Debug.WriteLine("Admin Login Error: " + ex.Message);
                }
            }
        }

        protected void btnVerify2FA_Click(object sender, EventArgs e)
        {
            if (Session["2FA_UserID"] == null)
            {
                ShowError("Session expired. Please login again.");
                ResetToLogin();
                return;
            }

            // Check expiration
            if (Session["2FAExpiration"] != null)
            {
                DateTime expiration = (DateTime)Session["2FAExpiration"];
                if (DateTime.Now > expiration)
                {
                    ShowError("Verification code expired. Please request a new code.");
                    return;
                }
            }

            int userId = (int)Session["2FA_UserID"];
            string code = txt2FACode.Text.Trim();

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_VerifyAdminTwoFactorCode", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@Code", code);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read() && Convert.ToBoolean(reader["IsValid"]))
                            {
                                reader.Close();

                                // Successful 2FA verification - complete login
                                string username = Session["2FA_Username"].ToString();
                                string email = Session["2FA_Email"].ToString();

                                Session["Username"] = username;
                                Session["UserID"] = userId;
                                Session["IsAdmin"] = true;
                                Session["AdminEmail"] = email;

                                // Clear 2FA session data
                                Session.Remove("2FA_UserID");
                                Session.Remove("2FA_Username");
                                Session.Remove("2FA_Email");
                                Session.Remove("2FAExpiration");

                                // Update last login
                                UpdateLastLoginDate(userId, conn);
                                LogAdminActivity(userId, "Admin Login", "Successful admin login with 2FA", conn);

                                // ✅ TRIGGER WAVE ANIMATION AND REDIRECT
                                TriggerSuccessAnimation("~/AdminDashboard.aspx");
                            }
                            else
                            {
                                ShowError("Invalid verification code. Please try again.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError("An error occurred during verification.");
                    System.Diagnostics.Debug.WriteLine("2FA Verification Error: " + ex.Message);
                }
            }
        }

        protected void btnResendCode_Click(object sender, EventArgs e)
        {
            if (Session["2FA_UserID"] == null)
            {
                ShowError("Session expired. Please login again.");
                ResetToLogin();
                return;
            }

            int userId = (int)Session["2FA_UserID"];
            string email = Session["2FA_Email"].ToString();
            string username = Session["2FA_Username"].ToString();

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string code = Generate2FACode(userId, conn);
                    Send2FACodeEmail(email, username, code, userId);

                    Session["2FAExpiration"] = DateTime.Now.AddMinutes(5);
                    UpdateExpirationDisplay();

                    ShowSuccess("A new verification code has been sent to your email.");
                }
                catch (Exception ex)
                {
                    ShowError("Error resending code.");
                    System.Diagnostics.Debug.WriteLine("Resend Code Error: " + ex.Message);
                }
            }
        }

        protected void lnkBackToLogin_Click(object sender, EventArgs e)
        {
            ResetToLogin();
        }

        private void ResetToLogin()
        {
            Session.Remove("2FA_UserID");
            Session.Remove("2FA_Username");
            Session.Remove("2FA_Email");
            Session.Remove("2FAExpiration");

            pnlCredentials.Visible = true;
            pnl2FA.Visible = false;
            txt2FACode.Text = "";
            lblAdminLoginStatus.Text = "";
        }

        private string Generate2FACode(int userId, SqlConnection conn)
        {
            string ipAddress = Request.UserHostAddress;

            using (SqlCommand cmd = new SqlCommand("sp_GenerateAdminTwoFactorCode", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@IPAddress", (object)ipAddress ?? DBNull.Value);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["Code"].ToString();
                    }
                }
            }

            return null;
        }

        private void Send2FACodeEmail(string email, string username, string code, int userId)
        {
            // Simulate sending email (similar to password reset)
            // In production, use actual email service

            string subject = "Chordal Admin Login - Verification Code";
            string body = $@"
                Hello {username},

                Your admin login verification code is: {code}

                This code will expire in 5 minutes.

                If you did not attempt to login, please contact support immediately.

                Best regards,
                Chordal Team
            ";

            // Log the email for simulation
            System.Diagnostics.Debug.WriteLine($"=== SIMULATED 2FA EMAIL ===");
            System.Diagnostics.Debug.WriteLine($"To: {email}");
            System.Diagnostics.Debug.WriteLine($"Subject: {subject}");
            System.Diagnostics.Debug.WriteLine($"Code: {code}");
            System.Diagnostics.Debug.WriteLine($"==========================");

            // Create notification for the user
            CreateNotification(userId, "Admin Login", $"2FA Code: {code} (Expires in 5 minutes)");
        }

        private void CreateNotification(int userId, string type, string message)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"INSERT INTO UserNotifications (UserID, NotificationType, Message) 
                                  VALUES (@UserID, @Type, @Message)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@Type", type);
                        cmd.Parameters.AddWithValue("@Message", message);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch { }
            }
        }

        private void UpdateExpirationDisplay()
        {
            if (Session["2FAExpiration"] != null)
            {
                DateTime expiration = (DateTime)Session["2FAExpiration"];
                TimeSpan remaining = expiration - DateTime.Now;

                if (remaining.TotalSeconds > 0)
                {
                    lblExpiration.Text = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
                }
                else
                {
                    lblExpiration.Text = "EXPIRED";
                }
            }
        }

        private void IncrementFailedAttempts()
        {
            if (Session["AdminFailedAttempts"] == null)
            {
                Session["AdminFailedAttempts"] = 1;
            }
            else
            {
                int attempts = (int)Session["AdminFailedAttempts"];
                attempts++;
                Session["AdminFailedAttempts"] = attempts;

                if (attempts >= MAX_FAILED_ATTEMPTS)
                {
                    Session["AdminLockoutTime"] = DateTime.Now.AddMinutes(LOCKOUT_DURATION_MINUTES);
                    ShowError($"Too many failed attempts. Account locked for {LOCKOUT_DURATION_MINUTES} minutes.");
                    btnAdminLogin.Enabled = false;
                }
                else
                {
                    int remainingAttempts = MAX_FAILED_ATTEMPTS - attempts;
                    lblAdminLoginStatus.Text += $" ({remainingAttempts} attempts remaining)";
                }
            }
        }

        private void UpdateLastLoginDate(int userId, SqlConnection conn)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateLastLoginDate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Update Last Login Error: " + ex.Message);
            }
        }

        private void LogAdminActivity(int userId, string activityType, string activityDetails, SqlConnection conn)
        {
            try
            {
                string sql = @"INSERT INTO UserNotifications (UserID, NotificationType, Message, IsRead) 
                              VALUES (@UserID, @NotificationType, @Message, 1)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@NotificationType", activityType);
                    cmd.Parameters.AddWithValue("@Message", activityDetails + " at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Log Admin Activity Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Shows an error message to the user
        /// </summary>
        private void ShowError(string message)
        {
            lblAdminLoginStatus.Text = message;
            lblAdminLoginStatus.CssClass = "auth-status-message auth-status-error";
            lblAdminLoginStatus.Style["display"] = "block";
        }

        /// <summary>
        /// Shows a success message to the user
        /// </summary>
        private void ShowSuccess(string message)
        {
            lblAdminLoginStatus.Text = message;
            lblAdminLoginStatus.CssClass = "auth-status-message auth-status-success";
            lblAdminLoginStatus.Style["display"] = "block";
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
                "AdminLoginSuccess",
                script,
                false
            );
        }
    }
}