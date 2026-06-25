using Gym.Models;
using Gym.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Gym.Services;

namespace Gym.Controllers
{
    public class CreateBookingDTO
    {
        public int ClassId { get; set; }
        public int MemberId { get; set; }
        public bool IsPaidSeparately { get; set; }
    }

    [Authorize] // 🔒 SECURIZAT COMPLET: Toate rezervările, înscrierile și anulările sunt blocate fără token JWT!
    public class BookingsController : ODataController
    {
        private readonly AppDbContext _context;
        private readonly ITenantService _tenantService;

        public BookingsController(AppDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        /// <summary>
        /// 🌟 ENDPOINT DEDICAT TRAINERILOR: Trage toate rezervările cu numele membrilor atașate, fără blocaje OData.
        /// Ruta: GET /api/Bookings/AllWithMembers
        /// </summary>
        [HttpGet("api/Bookings/AllWithMembers")]
        public async Task<IActionResult> GetAllWithMembers()
        {
            try
            {
                var bookings = await _context.Bookings
                    .Include(b => b.Member)
                    .Where(b => b.Status == true)
                    .Select(b => new {
                        b.Id,
                        b.GymClassId,
                        MemberName = b.Member != null ? b.Member.FullName : "Anonymous Member"
                    })
                    .ToListAsync();

                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține toate rezervările direct din baza de date via OData.
        /// Ruta: GET /odata/Bookings
        /// </summary>
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.Bookings);
        }

        /// <summary>
        /// Obține o rezervare specifică după ID via OData.
        /// Ruta: GET /odata/Bookings(1)
        /// </summary>
        [EnableQuery]
        public IActionResult Get([FromODataUri] int key)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.Id == key);
            if (booking == null) return NotFound();

            return Ok(booking);
        }

        /// <summary>
        /// Creează o rezervare nouă standard via OData.
        /// Ruta: POST /odata/Bookings
        /// </summary>
        [HttpPost]
        public IActionResult Post([FromBody] Booking booking)
        {
            if (booking == null)
                return BadRequest(new { error = "Booking data is missing!" });

            ModelState.Remove("Member");
            ModelState.Remove("GymClass");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            booking.BookingDate = DateTime.UtcNow; // Salvăm data rezervării în standardul global UTC
            booking.CompanyId = _tenantService.GetCompanyId() ?? 0;

            _context.Bookings.Add(booking);
            _context.SaveChanges();

            return Created(booking);
        }

        /// <summary>
        /// 🌟 ENDPOINT SPECIAL: Rezervă un loc cu verificare automată de locuri libere.
        /// Ruta: POST /api/Bookings/BookPlace
        /// </summary>
        [HttpPost("api/Bookings/BookPlace")]
        public async Task<IActionResult> BookPlace([FromBody] CreateBookingDTO dto)
        {
            if (dto == null || dto.ClassId <= 0 || dto.MemberId <= 0)
                return BadRequest(new { error = "Invalid booking details requested." });

            try
            {
                var gymClass = await _context.GymClasses
                    .Include(c => c.Bookings)
                    .FirstOrDefaultAsync(c => c.Id == dto.ClassId);

                if (gymClass == null)
                    return BadRequest(new { error = "The selected fitness class does not exist!" });

                var user = await _context.Users.FindAsync(dto.MemberId);
                if (user == null) return NotFound("User not found.");

                // 🌟 LOGICA MODIFICATĂ: 
                // Verificăm creditele DOAR dacă nu a plătit separat (dto.IsPaidSeparately == false)
                bool isDiamond = user.SubscriptionType == "Glow Diamond Unlimited 👑";
                bool hasCredits = user.SessionsLeftThisWeek != null && user.SessionsLeftThisWeek > 0;

                if (!dto.IsPaidSeparately && !isDiamond && !hasCredits)
                {
                    return BadRequest(new { error = "You do not have enough credits! Please pay the 35 RON fee or renew your subscription. 🌸" });
                }

                if (gymClass.AvailableSlots <= 0)
                    return BadRequest(new { error = "Sorry, this class is completely full! ❌" });

                var alreadyBooked = await _context.Bookings
                    .AnyAsync(b => b.GymClassId == dto.ClassId && b.MemberId == dto.MemberId && b.Status == true);

                if (alreadyBooked)
                    return BadRequest(new { error = "You have already booked a place for this class! 🌸" });

                var newBooking = new Booking
                {
                    GymClassId = dto.ClassId,
                    MemberId = dto.MemberId,
                    BookingDate = DateTime.UtcNow,
                    Status = true,
                    CompanyId = _tenantService.GetCompanyId() ?? 0
                };

                // Scădem credite doar dacă NU a plătit separat și NU e Diamond
                if (!dto.IsPaidSeparately && !isDiamond)
                {
                    user.SessionsLeftThisWeek -= 1;
                }

                _context.Bookings.Add(newBooking);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Place booked successfully! 💕",
                    remainingSessions = user.SessionsLeftThisWeek
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Șterge o rezervare din baza de date (Anulare).
        /// Ruta: DELETE /odata/Bookings(1)
        /// </summary>
        [HttpDelete]
        public IActionResult Delete([FromODataUri] int key)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.Id == key);
            if (booking == null) return NotFound();

            _context.Bookings.Remove(booking);
            _context.SaveChanges();

            return NoContent();
        }
    }
}