using System.IdentityModel.Tokens.Jwt;

namespace StudyGPT.Web.Services
{
    public class JwtService
    {
        public string? GetRole(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            var jwtToken = handler.ReadJwtToken(token);

            var roleClaim = jwtToken.Claims
                .FirstOrDefault(c =>
                    c.Type == "role" ||
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

            return roleClaim?.Value;
        }
    }
}