using Gym.Models;
using Gym.Data; // Adăugăm accesul la baza de date
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Gym.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        // Injectăm baza de date și logger-ul (pentru erori)
        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Aici poți, de exemplu, să numeri câți membri ai în sală ca să afișezi pe prima pagină
            // var totalMembri = _context.Users.Count();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}