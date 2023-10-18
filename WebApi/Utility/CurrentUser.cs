using Azure.Core;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebApi.Utility
{
    public static class CurrentUser
    {

        public static string FindUserId(HttpContext context)
        {
            var _bearer_token = context.Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(_bearer_token);
            var userId = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            return userId;
        }


    }
}
