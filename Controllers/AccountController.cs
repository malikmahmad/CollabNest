using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CollabNest.Data;
using CollabNest.Models;
using CollabNest.ViewModels;
using System.Net;
using System.Net.Mail;

namespace CollabNest.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AccountController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // ── REGISTER ──────────────────────────────────────────────────────
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (await _db.Users.AnyAsync(u => u.Email == vm.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered. Try logging in instead.");
                return View(vm);
            }

            var user = new User
            {
                Name = vm.Name,
                Email = vm.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);

            TempData["Success"] = $"Welcome to CollabNest, {user.Name}! 🎉 Your account is ready.";
            return RedirectToAction("Index", "Home");
        }

        // ── LOGIN ─────────────────────────────────────────────────────────
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == vm.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(vm.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password. Please try again.");
                return View(vm);
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);

            TempData["Success"] = $"Welcome back, {user.Name}! 👋";
            return RedirectToAction("Index", "Home");
        }

        // ── LOGOUT ────────────────────────────────────────────────────────
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You've been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        // ── PROFILE ───────────────────────────────────────────────────────
        public async Task<IActionResult> Profile(int? id)
        {
            var userId = id ?? HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = await _db.Users
                .Include(u => u.Projects)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();
            return View(user);
        }

        public IActionResult EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _db.Users.Find(userId);
            if (user == null) return NotFound();

            var vm = new ProfileVM { Name = user.Name, Bio = user.Bio, Skills = user.Skills };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(ProfileVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.Name = vm.Name;
            user.Bio = vm.Bio;
            user.Skills = vm.Skills;

            await _db.SaveChangesAsync();
            HttpContext.Session.SetString("UserName", user.Name);

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // ── FORGOT PASSWORD ───────────────────────────────────────────────
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Always show the same message to prevent email enumeration
            TempData["Info"] = "If that email is registered with CollabNest, you'll receive a password reset link shortly. Please check your inbox (and spam folder).";

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == vm.Email);
            if (user == null)
            {
                // Don't reveal if email exists — redirect as if successful
                return RedirectToAction("ForgotPassword");
            }

            // Invalidate any existing unused tokens for this user
            var existingTokens = await _db.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed)
                .ToListAsync();

            foreach (var t in existingTokens)
                t.IsUsed = true;

            // Create new token
            var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.Now.AddHours(1)
            };

            _db.PasswordResetTokens.Add(resetToken);
            await _db.SaveChangesAsync();

            // Build reset URL
            var resetUrl = Url.Action("ResetPassword", "Account",
                new { token = token, email = user.Email },
                Request.Scheme)!;

            // Send email
            try
            {
                await SendResetEmailAsync(user.Email, user.Name, resetUrl);
            }
            catch (Exception ex)
            {
                // Log but don't expose error to user
                Console.WriteLine($"[CollabNest] Email send failed: {ex.Message}");
            }

            return RedirectToAction("ForgotPassword");
        }

        // ── RESET PASSWORD ────────────────────────────────────────────────
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Invalid or expired password reset link.";
                return RedirectToAction("Login");
            }

            var resetToken = await _db.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == token &&
                    t.User!.Email == email &&
                    !t.IsUsed &&
                    t.ExpiresAt > DateTime.Now);

            if (resetToken == null)
            {
                TempData["Error"] = "This password reset link is invalid or has expired. Please request a new one.";
                return RedirectToAction("ForgotPassword");
            }

            var vm = new ResetPasswordVM { Token = token, Email = email };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var resetToken = await _db.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == vm.Token &&
                    t.User!.Email == vm.Email &&
                    !t.IsUsed &&
                    t.ExpiresAt > DateTime.Now);

            if (resetToken == null)
            {
                TempData["Error"] = "This password reset link is invalid or has expired. Please request a new one.";
                return RedirectToAction("ForgotPassword");
            }

            // Update password
            resetToken.User!.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.NewPassword);
            resetToken.IsUsed = true;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Your password has been reset successfully. Please log in with your new password.";
            return RedirectToAction("Login");
        }

        // ── EMAIL HELPER ──────────────────────────────────────────────────
        private async Task SendResetEmailAsync(string toEmail, string userName, string resetUrl)
        {
            var smtpHost = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var smtpUser = _config["Email:SmtpUser"] ?? "";
            var smtpPass = _config["Email:SmtpPass"] ?? "";
            var fromAddress = _config["Email:From"] ?? smtpUser;
            var fromName = _config["Email:FromName"] ?? "CollabNest";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='margin:0;padding:0;background:#070b18;font-family:""DM Sans"",Arial,sans-serif;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background:#070b18;padding:40px 20px;'>
    <tr>
      <td align='center'>
        <table width='500' cellpadding='0' cellspacing='0' style='background:#0d1526;border:1px solid rgba(255,255,255,0.07);border-radius:18px;overflow:hidden;max-width:500px;width:100%;'>

          <!-- Header -->
          <tr>
            <td style='background:linear-gradient(135deg,#8b7dff,#b8aefd);padding:32px 40px;text-align:center;'>
              <div style='font-size:2rem;margin-bottom:8px;'>🔐</div>
              <h1 style='color:#fff;font-size:1.5rem;font-weight:800;margin:0;letter-spacing:-0.5px;'>CollabNest</h1>
              <p style='color:rgba(255,255,255,0.85);margin:6px 0 0;font-size:0.9rem;'>Password Reset Request</p>
            </td>
          </tr>

          <!-- Body -->
          <tr>
            <td style='padding:36px 40px;'>
              <p style='color:#8a95b0;font-size:0.9rem;margin:0 0 8px;'>Hello <strong style='color:#eef0f7;'>{WebUtility.HtmlEncode(userName)}</strong>,</p>
              <p style='color:#8a95b0;font-size:0.9rem;line-height:1.65;margin:0 0 24px;'>We received a request to reset your CollabNest password. Click the button below to choose a new one. This link expires in <strong style='color:#b8aefd;'>1 hour</strong>.</p>

              <div style='text-align:center;margin:28px 0;'>
                <a href='{resetUrl}' style='background:linear-gradient(135deg,#8b7dff,#b8aefd);color:#fff;text-decoration:none;font-weight:700;font-size:0.95rem;padding:14px 36px;border-radius:10px;display:inline-block;letter-spacing:0.3px;'>
                  Reset My Password &rarr;
                </a>
              </div>

              <p style='color:#465070;font-size:0.8rem;line-height:1.65;margin:0;'>If you didn't request this, you can safely ignore this email. Your password won't change.</p>

              <div style='margin-top:24px;padding:14px 16px;background:rgba(255,255,255,0.03);border:1px solid rgba(255,255,255,0.07);border-radius:10px;'>
                <p style='color:#465070;font-size:0.75rem;margin:0 0 6px;'>Or copy this link into your browser:</p>
                <p style='color:#8b7dff;font-size:0.73rem;word-break:break-all;margin:0;'>{resetUrl}</p>
              </div>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='padding:20px 40px 30px;border-top:1px solid rgba(255,255,255,0.05);text-align:center;'>
              <p style='color:#465070;font-size:0.75rem;margin:0;'>
                &copy; 2026 CollabNest &mdash; Connect. Collaborate. Build Together.<br>
                <span style='color:#8b7dff;'>Crafted by Malik Muhammad Ahmad</span>
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName),
                Subject = "Reset your CollabNest password",
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);
            await client.SendMailAsync(message);
        }
    }
}