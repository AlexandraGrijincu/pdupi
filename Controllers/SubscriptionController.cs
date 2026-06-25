using Gym.Models;
using Gym.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Formatter;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Gym.Services;
namespace Gym.Controllers
{
    [Authorize] // 🔒 SECURIZAT PRIN JWT VIA CONTROL GLOBAL
    public class SubscriptionsController : ODataController
    {
        private readonly AppDbContext _context;
        private readonly ITenantService _tenantService;
        public SubscriptionsController(AppDbContext context, ITenantService tenantService) // 2. Injectează
        {
            _context = context;
            _tenantService = tenantService;
        }

        /// <summary>
        /// Obține lista tuturor abonamentelor via OData.
        /// Ruta: GET /odata/Subscriptions
        /// </summary>
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.Subscriptions);
        }

        /// <summary>
        /// Obține un abonament specific după ID.
        /// Ruta: GET /odata/Subscriptions(1)
        /// </summary>
        [EnableQuery]
        public IActionResult Get([FromODataUri] int key)
        {
            var sub = _context.Subscriptions.FirstOrDefault(s => s.Id == key);
            if (sub == null) return NotFound();
            return Ok(sub);
        }

        /// <summary>
        /// Salvează un abonament nou direct în PostgreSQL.
        /// Ruta: POST /odata/Subscriptions
        /// </summary>
        [HttpPost]
        public IActionResult Post([FromBody] Subscription subscription)
        {
            if (subscription == null)
                return BadRequest(new { error = "Data payload is empty." });

            ModelState.Remove("User");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            subscription.CompanyId = _tenantService.GetCompanyId() ?? 0;
            _context.Subscriptions.Add(subscription);
            _context.SaveChanges();

            return Created(subscription);
        }
    }
}