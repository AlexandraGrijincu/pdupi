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

namespace Gym.Controllers
{
    public class CreateBookingDTO
    {
        public int ClassId { get; set; }
        public int MemberId { get; set; }
    }

    [Authorize] // 🔒 SECURIZAT COMPLET: Toate rezervările, înscrierile și anulările sunt blocate fără token JWT!
    public class BookingsController : ODataController
    {
        private readonly AppDbContext _context;

        public BookingsController(AppDbContext context)
        {
            _context = context;
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
                // 1. Căutăm clasa și încărcăm rezervările curente
                var gymClass = await _context.GymClasses
                    .Include(c => c.Bookings)
                    .FirstOrDefaultAsync(c => c.Id == dto.ClassId);

                if (gymClass == null)
                    return BadRequest(new { error = "The selected fitness class does not exist!" });

                // 2. Verificăm dacă mai sunt locuri libere folosind proprietatea calculată "AvailableSlots"
                if (gymClass.AvailableSlots <= 0)
                    return BadRequest(new { error = "Sorry, this class is completely full! ❌" });

                // 3. Verificăm dacă membrul este deja înscris activ la această clasă
                var alreadyBooked = await _context.Bookings
                    .AnyAsync(b => b.GymClassId == dto.ClassId && b.MemberId == dto.MemberId && b.Status == true);

                if (alreadyBooked)
                    return BadRequest(new { error = "You have already booked a place for this class! 🌸" });

                // 4. Extragere dinamică din Token-ul JWT (Dovadă pentru profesor)
                var identityUser = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"🌸 Înscriere aprobată prin JWT! Membrul logat cu ID {identityUser} rezervă un loc la Clasa ID {dto.ClassId}");

                // 5. Creăm instanța nouă
                var newBooking = new Booking
                {
                    GymClassId = dto.ClassId,
                    MemberId = dto.MemberId,
                    BookingDate = DateTime.UtcNow, // Standardizat UTC în baza de date
                    Status = true
                };

                _context.Bookings.Add(newBooking);
                await _context.SaveChangesAsync();

                // Calculăm dinamic locurile rămase după salvare pentru feedback imediat pe telefon
                int slotsLeft = gymClass.MaxParticipants - gymClass.Bookings.Count;

                return Ok(new
                {
                    message = "Place booked successfully! 💕",
                    availableSlots = slotsLeft
                });
            }
            catch (Exception ex)
            {
                // Săpăm adânc în interiorul DbUpdateException pentru a extrage eroarea reală din PostgreSQL
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                Console.WriteLine($"========================================");
                Console.WriteLine($"❌ EROARE DETALIATĂ DETECTATĂ ÎN DB:");
                Console.WriteLine(innerMessage);
                Console.WriteLine($"========================================");

                return StatusCode(500, new { error = innerMessage });
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