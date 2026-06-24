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
    [Authorize] // 🔒 SECURIZAT COMPLET: Nimeni nu poate vedea sau crea tipuri de sport fără token JWT valid
    public class SportTypesController : ODataController
    {
        private readonly AppDbContext _context;

        public SportTypesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obține toate tipurile de sport direct din tabelul SportTypes.
        /// Ruta: GET /odata/SportTypes
        /// </summary>
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.SportTypes);
        }

        /// <summary>
        /// Obține un tip de sport după ID.
        /// Ruta: GET /odata/SportTypes(1)
        /// </summary>
        [EnableQuery]
        public IActionResult Get([FromODataUri] int key)
        {
            var sport = _context.SportTypes.FirstOrDefault(s => s.Id == key);
            if (sport == null) return NotFound();
            return Ok(sport);
        }

        /// <summary>
        /// Adaugă un sport nou direct în baza de date reală.
        /// Ruta: POST /odata/SportTypes
        /// </summary>
        [HttpPost]
        public IActionResult Post([FromBody] SportType sportType)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.SportTypes.Add(sportType);
            _context.SaveChanges();

            return Created(sportType);
        }
    }
}