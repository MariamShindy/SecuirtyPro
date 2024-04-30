using System.Net;
using System.Text.Json;
using Talabat.APIs.Errors;

namespace Talabat.APIs.Middlewares
{
	// By Convension
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate next;
		private readonly ILogger<ExceptionMiddleware> logger;
		private readonly IHostEnvironment env;

		public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
			this.next = next;
			this.logger = logger;
			this.env = env;
		}
    
		public async Task InvokeAsync(HttpContext httpContext)
		{
			try
			{
				// Request
				await next.Invoke(httpContext);
				// Response
			}
			catch (Exception ex)
			{
				logger.LogError(ex, ex.Message);
				// Log Exceptions in [Database, Files] --> Production 

				httpContext.Response.ContentType = "application/json";
				httpContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

				var response = env.IsDevelopment()?
					new ApiExceptionResponse((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace.ToString())
					:new ApiExceptionResponse((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace.ToString());

				var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

				var json = JsonSerializer.Serialize(response, options);

				await httpContext.Response.WriteAsync(json);
			}
		}
	}
}
