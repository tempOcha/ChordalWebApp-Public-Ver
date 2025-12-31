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
    public partial class UpdateProfile : System.Web.UI.Page
    {
        private int CurrentUserId => Convert.ToInt32(Session["UserID"]);
        private string CurrentUsername => Session["Username"]?.ToString();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check authentication
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery));
                return;
            }

            if (!IsPostBack)
            {
                LoadCurrentUserData();
            }
        }

        private void LoadCurrentUserData()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            u.Username, 
                            u.Email,
                            ISNULL(up.EmailNotifications, 1) AS EmailNotifications,
                            ISNULL(up.CommunityNotifications, 1) AS CommunityNotifications
                        FROM Users u
                        LEFT JOIN UserPreferences up ON u.UserID = up.UserID
                        WHERE u.UserID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", CurrentUserId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtUsername.Text = reader["Username"].ToString();
                                txtEmail.Text = reader["Email"].ToString();

                                // Load preferences with default to true if null
                                chkEmailNotifications.Checked = Convert.ToBoolean(reader["EmailNotifications"]);
                                chkCommunityNotifications.Checked = Convert.ToBoolean(reader["CommunityNotifications"]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "Error loading profile data.";
                    lblStatus.CssClass = "text-danger";
                    System.Diagnostics.Trace.WriteLine("LoadCurrentUserData Error: " + ex.ToString());
                }
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                lblStatus.Text = "Please correct the errors on the page.";
                lblStatus.CssClass = "text-danger";
                return;
            }

            // Enable password validators if user is attempting to change password
            bool isChangingPassword = !string.IsNullOrWhiteSpace(txtNewPassword.Text) ||
                                     !string.IsNullOrWhiteSpace(txtCurrentPassword.Text);

            if (isChangingPassword)
            {
                revNewPassword.Enabled = true;
                cvPassword.Enabled = true;

                // Re-validate with password validators enabled
                if (!Page.IsValid)
                {
                    lblStatus.Text = "Please correct the password errors.";
                    lblStatus.CssClass = "text-danger";
                    return;
                }
            }

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Check if username or email already exists (for other users)
                    if (!ValidateUniqueFields(conn, txtUsername.Text.Trim(), txtEmail.Text.Trim()))
                    {
                        return; // Error message already set in ValidateUniqueFields
                    }

                    // If changing password, verify current password first
                    if (isChangingPassword)
                    {
                        if (!VerifyCurrentPassword(conn, txtCurrentPassword.Text))
                        {
                            lblStatus.Text = "Current password is incorrect.";
                            lblStatus.CssClass = "text-danger";
                            return;
                        }
                    }

                    // Track what changed for notifications
                    bool emailChanged = false;
                    bool passwordChanged = false;
                    bool usernameChanged = false;

                    string oldEmail = GetCurrentEmail(conn);
                    string oldUsername = CurrentUsername;

                    emailChanged = !oldEmail.Equals(txtEmail.Text.Trim(), StringComparison.OrdinalIgnoreCase);
                    usernameChanged = !oldUsername.Equals(txtUsername.Text.Trim(), StringComparison.Ordinal);
                    passwordChanged = isChangingPassword;

                    // Update user profile
                    UpdateUserProfile(conn, isChangingPassword);

                    // Update session if username changed
                    if (usernameChanged)
                    {
                        Session["Username"] = txtUsername.Text.Trim();
                    }

                    // Send notifications for security-sensitive changes
                    SendSecurityNotifications(emailChanged, passwordChanged, usernameChanged, oldEmail);

                    lblStatus.Text = "Profile updated successfully!";
                    lblStatus.CssClass = "text-success";

                    // Reload data to show updated values
                    LoadCurrentUserData();

                    // Clear password fields
                    txtCurrentPassword.Text = "";
                    txtNewPassword.Text = "";
                    txtConfirmPassword.Text = "";
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "Error updating profile: " + ex.Message;
                    lblStatus.CssClass = "text-danger";
                    System.Diagnostics.Trace.WriteLine("UpdateProfile Error: " + ex.ToString());
                }
            }
        }

        private bool ValidateUniqueFields(SqlConnection conn, string username, string email)
        {
            // Check if username exists for another user
            string checkUsernameQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND UserID != @UserID";
            using (SqlCommand cmd = new SqlCommand(checkUsernameQuery, conn))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@UserID", CurrentUserId);

                int count = (int)cmd.ExecuteScalar();
                if (count > 0)
                {
                    lblStatus.Text = "Username already taken. Please choose another.";
                    lblStatus.CssClass = "text-danger";
                    return false;
                }
            }

            // Check if email exists for another user
            string checkEmailQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email AND UserID != @UserID";
            using (SqlCommand cmd = new SqlCommand(checkEmailQuery, conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@UserID", CurrentUserId);

                int count = (int)cmd.ExecuteScalar();
                if (count > 0)
                {
                    lblStatus.Text = "Email already registered to another account.";
                    lblStatus.CssClass = "text-danger";
                    return false;
                }
            }

            return true;
        }

        private bool VerifyCurrentPassword(SqlConnection conn, string currentPassword)
        {
            string query = "SELECT PasswordHash FROM Users WHERE UserID = @UserID";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserID", CurrentUserId);
                string storedHash = cmd.ExecuteScalar()?.ToString();

                if (string.IsNullOrEmpty(storedHash))
                    return false;

                try
                {
                    PBKDF2Hash hashVerifier = new PBKDF2Hash(currentPassword, storedHash);
                    return hashVerifier.PasswordCheck;
                }
                catch
                {
                    return false;
                }
            }
        }

        private string GetCurrentEmail(SqlConnection conn)
        {
            string query = "SELECT Email FROM Users WHERE UserID = @UserID";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserID", CurrentUserId);
                return cmd.ExecuteScalar()?.ToString() ?? "";
            }
        }

        private void UpdateUserProfile(SqlConnection conn, bool isChangingPassword)
        {
            string updateQuery;

            if (isChangingPassword)
            {
                // Hash the new password
                PBKDF2Hash passwordHasher = new PBKDF2Hash(txtNewPassword.Text);
                string newPasswordHash = passwordHasher.HashedPassword;

                updateQuery = @"
                    UPDATE Users 
                    SET Username = @Username,
                        Email = @Email,
                        PasswordHash = @PasswordHash,
                        LastModified = GETDATE()
                    WHERE UserID = @UserID";

                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
                    cmd.Parameters.AddWithValue("@UserID", CurrentUserId);

                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                // Update without changing password
                updateQuery = @"
                    UPDATE Users 
                    SET Username = @Username,
                        Email = @Email,
                        LastModified = GETDATE()
                    WHERE UserID = @UserID";

                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@UserID", CurrentUserId);

                    cmd.ExecuteNonQuery();
                }
            }

            // Update notification preferences in UserPreferences table
            UpdateUserPreferences(conn);
        }

        private void UpdateUserPreferences(SqlConnection conn)
        {
            // First, check if preference record exists
            string checkQuery = "SELECT COUNT(*) FROM UserPreferences WHERE UserID = @UserID";
            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
            {
                checkCmd.Parameters.AddWithValue("@UserID", CurrentUserId);
                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    // Update existing preferences
                    string updateQuery = @"
                        UPDATE UserPreferences
                        SET EmailNotifications = @EmailNotifications,
                            CommunityNotifications = @CommunityNotifications,
                            LastModified = GETDATE()
                        WHERE UserID = @UserID";

                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@EmailNotifications", chkEmailNotifications.Checked);
                        updateCmd.Parameters.AddWithValue("@CommunityNotifications", chkCommunityNotifications.Checked);
                        updateCmd.Parameters.AddWithValue("@UserID", CurrentUserId);
                        updateCmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    // Insert new preferences
                    string insertQuery = @"
                        INSERT INTO UserPreferences (UserID, EmailNotifications, CommunityNotifications)
                        VALUES (@UserID, @EmailNotifications, @CommunityNotifications)";

                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@UserID", CurrentUserId);
                        insertCmd.Parameters.AddWithValue("@EmailNotifications", chkEmailNotifications.Checked);
                        insertCmd.Parameters.AddWithValue("@CommunityNotifications", chkCommunityNotifications.Checked);
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void SendSecurityNotifications(bool emailChanged, bool passwordChanged, bool usernameChanged, string oldEmail)
        {
            // Log notification in database
            string notificationMessage = "Your profile has been updated.";

            if (emailChanged || passwordChanged || usernameChanged)
            {
                notificationMessage = "Security Update: ";

                if (usernameChanged) notificationMessage += "Username changed. ";
                if (emailChanged) notificationMessage += "Email address changed. ";
                if (passwordChanged) notificationMessage += "Password changed. ";

                notificationMessage += "If you didn't make these changes, please contact support immediately.";

                // Log to database
                NotificationHelper.LogNotification(
                    CurrentUserId,
                    NotificationHelper.NotificationTypes.Account,
                    notificationMessage
                );

                // Send email notification for security-sensitive changes
                if (emailChanged || passwordChanged)
                {
                    string emailSubject = "Security Alert: Your Chordal Account Was Updated";
                    string emailBody = $@"
                        <h2>Account Security Notification</h2>
                        <p>Hello {txtUsername.Text.Trim()},</p>
                        <p>We're writing to inform you that security-sensitive changes were made to your Chordal account:</p>
                        <ul>";

                    if (emailChanged)
                    {
                        emailBody += $"<li>Email address changed from {oldEmail} to {txtEmail.Text.Trim()}</li>";
                    }
                    if (passwordChanged)
                    {
                        emailBody += "<li>Password was changed</li>";
                    }

                    emailBody += @"
                        </ul>
                        <p>If you made these changes, you can safely ignore this email.</p>
                        <p><strong>If you did NOT make these changes, please secure your account immediately by:</strong></p>
                        <ul>
                            <li>Resetting your password</li>
                            <li>Contacting our support team</li>
                        </ul>
                        <p>Best regards,<br/>The Chordal Team</p>
                    ";

                    // Send to OLD email if email was changed, otherwise to current email
                    string emailTo = emailChanged ? oldEmail : txtEmail.Text.Trim();
                    NotificationHelper.SendEmailNotification(emailTo, emailSubject, emailBody, CurrentUserId);

                    // Also send to NEW email if email was changed
                    if (emailChanged)
                    {
                        string newEmailBody = $@"
                            <h2>Email Address Confirmation</h2>
                            <p>Hello {txtUsername.Text.Trim()},</p>
                            <p>This email address has been added to your Chordal account.</p>
                            <p>If you did not make this change, please contact support immediately.</p>
                            <p>Best regards,<br/>The Chordal Team</p>
                        ";

                        NotificationHelper.SendEmailNotification(
                            txtEmail.Text.Trim(),
                            "Your Email Address Was Added to Chordal",
                            newEmailBody,
                            CurrentUserId
                        );
                    }
                }
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/LandingPage.aspx");
        }
    }
}