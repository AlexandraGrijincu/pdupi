using Gym.Models;
using Gym.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Formatter;
using System.Linq;
using Microsoft.AspNetCore.Authorization; // 🌟 ADĂUGAT: Namespace-ul pentru securitate

namespace Gym.Controllers
{
    [Authorize] // 🔒 SECURIZAT COMPLET: Doar utilizatorii logați pot citi sau modifica sălile de sport
    public class GymRoomsController : ODataController
    {
        private readonly AppDbContext _context;

        public GymRoomsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obține lista tuturor sălilor direct din baza de date.
        /// Ruta: GET /odata/GymRooms
        /// </summary>
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.GymRooms);
        }

        /// <summary>
        /// Obține detaliile unei săli după ID din PostgreSQL.
        /// Ruta: GET /odata/GymRooms(1)
        /// </summary>
        [EnableQuery]
        public IActionResult Get([FromODataUri] int key)
        {
            var room = _context.GymRooms.FirstOrDefault(r => r.Id == key);
            if (room == null) return NotFound();
            return Ok(room);
        }

        /// <summary>
        /// Adaugă o sală nouă în baza de date reală.
        /// Ruta: POST /odata/GymRooms
        /// </summary>
        [HttpPost]
        public IActionResult Post([FromBody] GymRoom room)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.GymRooms.Add(room);
            _context.SaveChanges();

            return Created(room);
        }
    }
}