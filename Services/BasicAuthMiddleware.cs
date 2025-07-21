/*using System.Net;
using System.Text;

namespace VirtualCard.Services
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _username;
        private readonly string _password;

        public BasicAuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _username = configuration["BasicAuth:Username"];
            _password = configuration["BasicAuth:Password"];
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string authHeader = context.Request.Headers["Authorization"];

            if (authHeader != null && authHeader.StartsWith("Basic "))
            {
                // Extract credentials
                var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
                var decodedBytes = Convert.FromBase64String(encodedUsernamePassword);
                var decodedUsernamePassword = Encoding.UTF8.GetString(decodedBytes);
                var parts = decodedUsernamePassword.Split(':', 2);

                if (parts.Length == 2 && parts[0] == _username && parts[1] == _password)
                {
                    await _next(context); // ✅ Passed
                    return;
                }
            }

            // ❌ Unauthorized
            context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"STB API\"";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Invalid or missing Basic Authentication.");
        }
    }
}
*/