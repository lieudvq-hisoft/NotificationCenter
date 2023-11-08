using System;
using System.Security.Claims;

namespace Services.ClaimExtensions
{
    public static class ClaimExtensions
    {
        public static string GetId(this ClaimsPrincipal user)
        {
            var idClaim = user.Claims.FirstOrDefault(i => i.Type.Equals("UserId"));
            if (idClaim != null)
            {
                return idClaim.Value;
            }
            return "";
        }
    }
}

