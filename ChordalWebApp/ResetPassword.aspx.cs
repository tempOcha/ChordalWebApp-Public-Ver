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
    public partial class ResetPassword : System.Web.UI.Page
    {
        private string resetToken;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Hide status panel initially
                pnlStatusMessage.Visible = false;

                // Get token from query string
                resetToken = Request.QueryString["token"];

                if (string.IsNullOrEmpty(resetToken))
                {
                    ShowError("Invalid password reset link. Please request a new one.");
                    pnlResetForm.Visible = false;
                    return;
                }

                // Validate token
                if (!ValidateToken(resetToken))
                {
                    ShowError("This password reset link has expired or is invalid. Please request a new one.");
                    pnlResetForm.Visible = false;
                    return;
                }

                // Store token in ViewState for postback
                ViewState["ResetToken"] = resetToken;
            }
        }

        protected void btnResetPassword_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                ShowError("Please correct the errors on the page.");
                return;
            }

            resetToken = ViewState["ResetToken"]?.ToString();

            if (string.IsNullOrEmpty(resetToken))
            {
                ShowError("Session expired. Please request a new password reset link.");
                pnlResetForm.Visible = false;
                return;
            }

            string newPassword = txtNewPassword.Text;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Validate token again and get UserID
                    int userId = GetUserIdFromToken(conn, resetToken);

                    if (userId == -1)
                    {
                        ShowError("This password reset link has expired or is invalid.");
                        pnlResetForm.Visible = false;
                        return;
                    }

                    // Hash the new password
                    PBKDF2Hash pbkdf2 = new PBKDF2Hash(newPassword);
                    string newPasswordHash = pbkdf2.HashedPassword;

                    // Update password in database
                    string updateQuery = "UPDATE Users SET PasswordHash = @PasswordHash WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.ExecuteNonQuery();
                    }

                    // Mark token as used
                    string markTokenQuery = "UPDATE PasswordResetTokens SET IsUsed = 1 WHERE Token = @Token";
                    using (SqlCommand cmd = new SqlCommand(markTokenQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Token", resetToken);
                        cmd.ExecuteNonQuery();
                    }

                    // Log notification
                    try
                    {
                        NotificationHelper.LogNotification(
                            userId,
                            "Password Changed",
                            "Your password has been successfully changed.",
                            conn
                        );
                    }
                    catch (Exception notifEx)
                    {
                        // Don't fail password reset if notification fails
                        System.Diagnostics.Trace.WriteLine("Notification Error: " + notifEx.Message);
                    }

                    // Hide the form and show success message with animation
                    pnlResetForm.Visible = false;
                    ShowSuccess("Your password has been successfully reset!");

                    // Trigger wave animation and redirect to login
                    TriggerSuccessAnimation("~/Login.aspx");
                }
                catch (Exception ex)
                {
                    ShowError("An error occurred while resetting your password. Please try again.");
                    System.Diagnostics.Trace.WriteLine("ResetPassword Error: " + ex.ToString());
                }
            }
        }

        private bool ValidateToken(string token)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    return GetUserIdFromToken(conn, token) != -1;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("ValidateToken Error: " + ex.ToString());
                    return false;
                }
            }
        }

        private int GetUserIdFromToken(SqlConnection conn, string token)
        {
            string query = @"
                SELECT UserID 
                FROM PasswordResetTokens 
                WHERE Token = @Token 
                  AND IsUsed = 0 
                  AND ExpirationTime > GETDATE()";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Token", token);
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
            }

            return -1; // Token not found, expired, or already used
        }

        /// <summary>
        /// Shows an error message to the user
        /// </summary>
        private void ShowError(string message)
        {
            pnlStatusMessage.Visible = true;
            pnlStatusMessage.CssClass = "auth-status-message auth-status-error";
            lblStatus.Text = message;
        }

        /// <summary>
        /// Shows a success message to the user
        /// </summary>
        private void ShowSuccess(string message)
        {
            pnlStatusMessage.Visible = true;
            pnlStatusMessage.CssClass = "auth-status-message auth-status-success";
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
                                // Trigger wave animation with 2.5 second duration (slightly longer for success message to be seen)
                                handleAuthSuccess('{resolvedUrl}', 2500);
                            }} else {{
                                // Fallback if animation script isn't loaded
                                setTimeout(function() {{
                                    window.location.href = '{resolvedUrl}';
                                }}, 1500);
                            }}
                        }}, 100);
                    }});
                </script>
            ";

            // Register the script to run on page load
            ClientScript.RegisterStartupScript(
                this.GetType(),
                "ResetPasswordSuccess",
                script,
                false
            );
        }
    }
}


























