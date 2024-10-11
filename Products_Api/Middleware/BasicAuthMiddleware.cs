using System.Net;
using System.Security.Claims;
using System.Text;

namespace Products_Api.Middleware
{
public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BasicAuthMiddleware> _logger;

        public BasicAuthMiddleware(RequestDelegate next, ILogger<BasicAuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check for the Authorization header
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();

                if (authHeader.StartsWith("Basic "))
                {
                    var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
                    var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials)).Split(':');

                    if (credentials.Length == 2)
                    {
                        var username = credentials[0];
                        var password = credentials[1];

                        // Validate the username and password
                        if (IsValidUser(username, password))
                        {
                            // Create claims and set the user as authenticated
                            var claims = new[] { new Claim(ClaimTypes.Name, username) };
                            var identity = new ClaimsIdentity(claims, "Basic");
                            var principal = new ClaimsPrincipal(identity);

                            context.User = principal;
                        }
                    }
                }
            }

            // If not authenticated, return 401 Unauthorized
            if (!context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            await _next(context);
        }

        private bool IsValidUser(string username, string password)
        {
            // Replace this with your user validation logic (e.g., database check)
            return username == "testuser" && password == "testpassword"; // Example credentials
        }
    }

}
