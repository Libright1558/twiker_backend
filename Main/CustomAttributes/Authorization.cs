using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace twiker_backend.CustomAttributes.Authentication
{
    public class JwtAuthorizeAttribute : AuthorizeAttribute
    {
        public JwtAuthorizeAttribute()
        {
            AuthenticationSchemes = "JwtScheme";
        }
    }

    public class AccessAuthorizeAttribute : AuthorizeAttribute
    {
        public AccessAuthorizeAttribute()
        {
            AuthenticationSchemes = "AccessScheme";
        }
    }
}
