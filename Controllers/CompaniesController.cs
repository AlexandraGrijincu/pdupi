using Microsoft.AspNetCore.Mvc;
using Gym.Data; // Asigură-te că ai importat namespace-ul corect pentru AppDbContext
using System.Linq;

namespace Gym.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase // 1. Moștenește din ControllerBase
    {
        private readonly AppDbContext _context;

        // 2. Injectează contextul bazei de date
        public CompaniesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("get-companies")]
        public IActionResult GetCompanies()
        {
            var companies = _context.Companies.ToList();
            return Ok(companies);
        }
    }
}