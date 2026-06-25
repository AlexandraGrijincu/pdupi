using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Gym.Services
{
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetCompanyId()
        {
            // Extrage claim-ul "CompanyId" din token-ul JWT al utilizatorului curent
            var claim = _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == "CompanyId")?.Value;

            if (int.TryParse(claim, out var id))
            {
                return id;
            }

            return null;
        }
    }
}