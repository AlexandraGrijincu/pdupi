using Gym.Data;
using Gym.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Gym.Controllers
{
    public class RequestRegisterDTO
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ConfirmRegisterDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Role { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    public class LoginDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ForgotPasswordDTO
    {
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyResetCodeDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ScanQrDTO
    {
        public string QrToken { get; set; } = string.Empty;
    }

    public class UsersController : ODataController
    {
        private readonly AppDbContext _context;

        private static System.Collections.Concurrent.ConcurrentDictionary<string, string> _temporaryRegisterCodes =
            new System.Collections.Concurrent.ConcurrentDictionary<string, string>();

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.Users);
        }

        [HttpPost("api/Users/RequestRegister")]
        public async Task<IActionResult> RequestRegister([FromBody] RequestRegisterDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email))
                return BadRequest(new { error = "Email is required!" });

            try
            {
                string emailKey = dto.Email.Trim().ToLower();

                if (await _context.Users.AnyAsync(u => u.Email == emailKey))
                {
                    return BadRequest(new { error = "This email is already registered!" });
                }

                Random rand = new Random();
                string verificationCode = rand.Next(1000, 9999).ToString();

                _temporaryRegisterCodes[emailKey] = verificationCode;

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("GlowGym Team 🌸", "alespoty2023@gmail.com"));
                emailMessage.To.Add(new MailboxAddress("", dto.Email));
                emailMessage.Subject = "Confirm Your Email - GlowGym ✨";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #0A0A12; color: #ffffff; text-align: center;'>
                            <h2 style='color: #FF1493;'>GlowGym 🌸</h2>
                            <p style='color: #ffffff; font-size: 16px;'>Welcome to GlowGym!</p>
                            <p style='color: #ffffff;'>Use the 4-digit verification code below to activate your account:</p>
                            <div style='display: inline-block; padding: 15px 30px; background-color: #FF1493; color: white; border-radius: 8px; font-size: 26px; font-weight: bold; letter-spacing: 5px; margin-top: 15px; margin-bottom: 15px;'>
                                {verificationCode}
                            </div>
                        </div>"
                };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync("alespoty2023@gmail.com", "lxze xeto icwg nlnl");
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }

                Console.WriteLine($"!!! REGISTER CODE {verificationCode} sent to {dto.Email}");
                return Ok(new { message = "Verification code sent successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error mail client: " + ex.Message });
            }
        }

        [HttpPost("api/Users/Register")]
        public async Task<IActionResult> RegisterUser([FromBody] ConfirmRegisterDTO dto)
        {
            if (dto == null)
                return BadRequest(new { error = "Data is missing or invalid." });

            try
            {
                string emailKey = dto.Email.Trim().ToLower();

                if (!_temporaryRegisterCodes.TryGetValue(emailKey, out string savedCode))
                {
                    return BadRequest(new { error = "Session expired. Please request a new code! 🌸" });
                }

                if (savedCode != dto.Code.Trim())
                {
                    return BadRequest(new { error = "Invalid verification code! Please check your email. 🌸" });
                }

                if (await _context.Users.AnyAsync(u => u.Email == emailKey))
                {
                    return BadRequest(new { error = "This email was already registered while verifying!" });
                }

                string hashedPassword = BCryptNet.HashPassword(dto.Password);

                var newUser = new User
                {
                    FullName = dto.FullName.Trim(),
                    Email = emailKey,
                    Password = hashedPassword,
                    Role = (UserRole)dto.Role,
                    AccessToken = "QR_" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                    ProfileImage = null,
                    SubscriptionType = null,
                    SessionsLeftThisWeek = null
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                _temporaryRegisterCodes.TryRemove(emailKey, out _);

                return Ok(newUser);
            }
            catch (Exception)
            {
                return BadRequest(new { error = "An error occurred while saving the user." });
            }
        }

        [HttpPost("api/Users/Login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginDTO dto)
        {
            // 1. Verificare date primite
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                return BadRequest(new { error = "Email and password are required!" });

            // 2. Căutare utilizator
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.Trim().ToLower());
            if (user == null)
                return BadRequest(new { error = "Invalid email or password. ✨" });

            // 3. Verificare parolă
            bool isPasswordValid = false;
            try
            {
                isPasswordValid = BCryptNet.Verify(dto.Password, user.Password);
            }
            catch (Exception)
            {
                isPasswordValid = false;
            }

            if (!isPasswordValid)
                return BadRequest(new { error = "Invalid email or password. ✨" });

            // 4. Preluare dată expirare abonament
            // Căutăm cel mai recent abonament activ al utilizatorului
            var latestSub = await _context.Subscriptions
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.ExpiryDate)
                .FirstOrDefaultAsync();

            // 5. Generare Token și actualizare AccessToken
            string jwtToken = GenerateJwtToken(user);
            user.AccessToken = jwtToken;
            await _context.SaveChangesAsync();

            // 6. Returnare răspuns cu ExpiryDate inclus
            return Ok(new
            {
                Token = jwtToken,
                value = new[] {
            new {
                user.Id,
                user.Email,
                user.FullName,
                user.Role,
                user.ProfileImage,
                user.SubscriptionType,
                user.SessionsLeftThisWeek,
                ExpiryDate = latestSub?.ExpiryDate, // <--- Data extrasă din Subscriptions
                Token = jwtToken
            }
        }
            });
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("CheiaMeaSuperSecretaGlowGym2026!!!🌸");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [Authorize]
        [HttpPost("api/Users/ScanQR")]
        public async Task<IActionResult> ScanQrCode([FromBody] ScanQrDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.QrToken))
                return BadRequest(new { error = "Invalid QR code data!" });

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.AccessToken == dto.QrToken.Trim());

                if (user == null)
                    return BadRequest(new { error = "Access Denied! Invalid or unrecognized QR Code. ❌" });

                return Ok(new
                {
                    message = "Access Granted! 🟢",
                    fullName = user.FullName,
                    email = user.Email,
                    role = user.Role.ToString()
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Server error during QR scanning process." });
            }
        }

        [HttpPost("api/Users/ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email))
                return BadRequest(new { error = "Email is required!" });

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.Trim().ToLower());
                if (user == null)
                {
                    return Ok(new { message = "If this email is registered, you will receive a reset code shortly." });
                }

                Random rand = new Random();
                string resetCode = rand.Next(1000, 9999).ToString();

                user.AccessToken = "RESET_" + resetCode;
                await _context.SaveChangesAsync();

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("GlowGym Team 🌸", "alespoty2023@gmail.com"));
                emailMessage.To.Add(new MailboxAddress("", dto.Email));
                emailMessage.Subject = "Your Password Reset Code - GlowGym ✨";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #0A0A12; color: #ffffff; text-align: center;'>
                            <h2 style='color: #FF1493;'>GlowGym 🌸</h2>
                            <p style='color: #ffffff; font-size: 16px;'>Hello, {user.FullName}!</p>
                            <p style='color: #ffffff;'>Use the 4-digit code below inside the app to reset your password:</p>
                            <div style='display: inline-block; padding: 15px 30px; background-color: #FF1493; color: white; border-radius: 8px; font-size: 26px; font-weight: bold; letter-spacing: 5px; margin-top: 15px; margin-bottom: 15px;'>
                                {resetCode}
                            </div>
                        </div>"
                };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync("alespoty2023@gmail.com", "lxze xeto icwg nlnl");
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }

                return Ok(new { message = "Reset code sent successfully!" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error mail client" });
            }
        }

        [HttpPost("api/Users/VerifyResetCode")]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Code) || string.IsNullOrEmpty(dto.NewPassword))
                return BadRequest(new { error = "All fields are required!" });

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.Trim().ToLower());
                if (user == null)
                    return BadRequest(new { error = "User not found!" });

                string expectedToken = "RESET_" + dto.Code.Trim();
                if (user.AccessToken != expectedToken)
                {
                    return BadRequest(new { error = "Invalid reset code! Please check your email again. 🌸" });
                }

                user.Password = BCryptNet.HashPassword(dto.NewPassword);
                user.AccessToken = "QR_" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
                await _context.SaveChangesAsync();

                return Ok(new { message = "Password updated successfully!" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Database error" });
            }
        }

        [Authorize]
        [HttpPost("api/Users/UpdateProfileImage")]
        public async Task<IActionResult> UpdateProfileImage([FromBody] System.Text.Json.JsonElement body)
        {
            try
            {
                int userId = body.GetProperty("UserId").GetInt32();
                string base64Image = body.GetProperty("ProfileImage").GetString() ?? string.Empty;

                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound(new { error = "User not found!" });

                user.ProfileImage = base64Image;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Profile picture updated successfully! 🌸" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 👑 ACTUALIZAT: Configurat cu matricea de credite lunare totale (4, 8, 12) + reset curat la 0
        [Authorize]
        [HttpPost("api/Users/BuySubscription")]
        public async Task<IActionResult> BuySubscription([FromBody] System.Text.Json.JsonElement body)
        {
            try
            {
                // Validăm existența proprietăților pentru a evita erori de tip "Property not found"
                if (!body.TryGetProperty("UserId", out var userIdProp) || !body.TryGetProperty("SubscriptionTypeId", out var subIdProp))
                {
                    return BadRequest(new { error = "Invalid request payload." });
                }

                int userId = userIdProp.GetInt32();
                int subscriptionId = subIdProp.GetInt32();

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound(new { error = "Glow Member not found!" });

                // Resetare abonament
                if (subscriptionId == 0)
                {
                    user.SubscriptionType = null;
                    user.SessionsLeftThisWeek = 0;
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Subscription fully cleared! 🔒" });
                }

                // Definire pachete
                string typeName = "Glow Bronze 🌸";
                int sessions = 4;

                if (subscriptionId == 2) { typeName = "Glow Silver 💖"; sessions = 8; }
                else if (subscriptionId == 3) { typeName = "Glow Gold ✨"; sessions = 12; }
                else if (subscriptionId == 4) { typeName = "Glow Diamond Unlimited 👑"; sessions = 9999; }

                // Actualizare utilizator
                user.SubscriptionType = typeName;
                user.SessionsLeftThisWeek = sessions;

                // Salvare abonament cu expirare fixă la 30 de zile
                var newSub = new Subscription
                {
                    UserId = user.Id,
                    Type = typeName,
                    ExpiryDate = DateTime.UtcNow.AddDays(30), // Fix 30 de zile
                    RemainingSessions = sessions
                };

                _context.Subscriptions.Add(newSub);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Subscription activated successfully!",
                    expiryDate = newSub.ExpiryDate,
                    sessions = sessions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Subscription activation error: " + ex.Message });
            }
        }

        [Authorize]
        [HttpPost("api/Users/PayPerClass")]
        public async Task<IActionResult> PayPerClass([FromBody] System.Text.Json.JsonElement body)
        {
            try
            {
                int userId = body.GetProperty("UserId").GetInt32();
                int classId = body.GetProperty("ClassId").GetInt32();

                Console.WriteLine($"CNF: Membrul ID {userId} a achitat 35 RON pentru clasa {classId}");
                return Ok(new { message = "Single token session paid successfully! 💳✨" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 🌟 ENDPOINTLipsă RESTAURAT: Suportă POST + PUT și actualizează creditele scăzute din PostgreSQL în timp real
        [Authorize]
        [HttpPost("api/Users/UpdateCredits")]
        [HttpPut("api/Users/UpdateCredits")]
        public async Task<IActionResult> UpdateCredits([FromBody] System.Text.Json.JsonElement body)
        {
            try
            {
                int userId = 0;
                if (body.TryGetProperty("UserId", out var u1)) userId = u1.GetInt32();
                else if (body.TryGetProperty("userId", out var u2)) userId = u2.GetInt32();
                else if (body.TryGetProperty("Id", out var u3)) userId = u3.GetInt32();
                else if (body.TryGetProperty("id", out var u4)) userId = u4.GetInt32();

                int newCredits = 0;
                if (body.TryGetProperty("NewCredits", out var c1)) newCredits = c1.GetInt32();
                else if (body.TryGetProperty("newCredits", out var c2)) newCredits = c2.GetInt32();
                else if (body.TryGetProperty("SessionsLeftThisWeek", out var c3)) newCredits = c3.GetInt32();
                else if (body.TryGetProperty("sessionsLeftThisWeek", out var c4)) newCredits = c4.GetInt32();

                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound(new { error = "Glow Member not found!" });

                user.SessionsLeftThisWeek = newCredits;

                if (newCredits <= 0)
                {
                    user.SubscriptionType = null;
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = $"Credits updated to {newCredits} in PostgreSQL! 🟢" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Database credit update error: " + ex.Message });
            }
        }
    }
}