using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.EntityFrameworkCore;
using Gym.Models;
using Gym.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Gym.Controllers
{
    public class CreateClassDto
    {
        public int TrainerId { get; set; }
        public int RoomId { get; set; }
        public string SportTypeName { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public int Duration { get; set; }
        public string StartTime { get; set; }
    }

    [Authorize] // 🔒 TOATE rutele (Get și Post) cer acum bilet JWT valid de pe telefon!
    public class GymClassesController : ODataController
    {
        private readonly AppDbContext _context;

        public GymClassesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obține toate clasele de fitness cu toate relațiile incluse.
        /// Ruta: GET /odata/GymClasses
        /// </summary>
        [EnableQuery]
        public IActionResult Get()
        {
            // 🌟 REPARAT: Încărcăm în lanț toate datele necesare pentru interfața trainerilor și a membrilor
            var gymClasses = _context.GymClasses
                .Include(c => c.SportType)  // Include tipul de sport (Zumba, Pilates etc.)
                .Include(c => c.Room)       // Include detalii despre sală (Room 1, F1)
                .Include(c => c.Trainer)    // Include numele antrenorului care ține clasa
                .Include(c => c.Bookings)   // 👥 Include rezervările atașate clasei
                    .ThenInclude(b => b.Member); // 💖 Include utilizatorii înscriși pentru lista antrenorilor

            return Ok(gymClasses);
        }

        /// <summary>
        /// Obține detaliile unei singure clase specifice după ID.
        /// Ruta: GET /odata/GymClasses(1)
        /// </summary>
        [EnableQuery]
        public IActionResult Get([FromODataUri] int key)
        {
            var gymClass = _context.GymClasses
                .Include(c => c.SportType)
                .Include(c => c.Room)
                .Include(c => c.Trainer)
                .Include(c => c.Bookings)
                    .ThenInclude(b => b.Member)
                .FirstOrDefault(c => c.Id == key);

            if (gymClass == null) return NotFound();
            return Ok(gymClass);
        }

        /// <summary>
        /// Creează o clasă nouă de fitness securizată prin token.
        /// Ruta: POST /odata/GymClasses
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateClassDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.SportTypeName) || string.IsNullOrEmpty(dto.StartTime))
            {
                return BadRequest(new { error = "Class data, Sport Name or Schedule Time is missing!" });
            }

            try
            {
                var userInToken = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"========================================");
                Console.WriteLine($"🌸 Cerere aprobată! Clasa este creată securizat de User ID: {userInToken}");
                Console.WriteLine($"========================================");

                var room = await _context.GymRooms.FindAsync(dto.RoomId);
                if (room == null)
                {
                    return NotFound(new { error = "The selected gym room does not exist!" });
                }

                if (dto.MaxParticipants > room.Capacity)
                {
                    return BadRequest(new { error = $"Capacity exceeded! Max allowed is {room.Capacity}." });
                }

                var sport = await _context.SportTypes
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == dto.SportTypeName.Trim().ToLower());

                if (sport == null)
                {
                    sport = new SportType { Name = dto.SportTypeName.Trim() };
                    _context.SportTypes.Add(sport);
                    await _context.SaveChangesAsync();
                }

                // 🌟 PARSARE UTC CURATĂ: Citim string-ul ISO primit de pe mobil direct ca UTC absolut
                DateTime parsedStartTimeUtc = DateTime.Parse(dto.StartTime).ToUniversalTime();

                var gymClass = new GymClass
                {
                    TrainerId = dto.TrainerId,
                    RoomId = dto.RoomId,
                    SportTypeId = sport.Id,
                    StartTime = parsedStartTimeUtc, // 🔒 Salvat în standardul normal UTC în DB
                    EndTime = parsedStartTimeUtc.AddMinutes(dto.Duration),
                    MaxParticipants = dto.MaxParticipants
                };

                _context.GymClasses.Add(gymClass);
                await _context.SaveChangesAsync();

                return Created(gymClass);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Database error: " + ex.Message });
            }
        }
    }
}