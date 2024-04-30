using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Net;
using System.Text.Json;
using Talabat.APIs.Errors;
using Talabat.APIs.Extensions;
using Talabat.APIs.Helpers;
using Talabat.APIs.Middlewares;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Services.Contract;
using Talabat.Repository;
using Talabat.Repository.Data;
using Talabat.Repository.Identity;
using Talabat.Service;

namespace Talabat.APIs
{
	public class Program
	{

		// Entry Point
		public static async Task Main(string[] args)
		{

			var webApplicationBuilder = WebApplication.CreateBuilder(args);

			#region Configure Services
			// Add services to the DI container.

			webApplicationBuilder.Services.AddControllers();
			//	.AddNewtonsoftJson(options =>
			//{
			//	options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			//});
			// Register Required Web APIs Services to the DI Container


			webApplicationBuilder.Services.AddSwaggerServices();


			webApplicationBuilder.Services.AddDbContext<StoreContext>(options =>
			{
				options.UseSqlServer(webApplicationBuilder.Configuration.GetConnectionString("DefaultConnection"));
			});

			webApplicationBuilder.Services.AddDbContext<AppIdentityDbContext>((optionsBuilder) =>
			{
				optionsBuilder.UseSqlServer(webApplicationBuilder.Configuration.GetConnectionString("IdentityConnection"));
			});

			webApplicationBuilder.Services.AddSingleton<IConnectionMultiplexer>((serviceProvider) =>
			{
				var connection = webApplicationBuilder.Configuration.GetConnectionString("Redis");
				return ConnectionMultiplexer.Connect(connection);
			});

			webApplicationBuilder.Services.AddApplicationServices();

			webApplicationBuilder.Services.AddIdentityServices(webApplicationBuilder.Configuration);

			webApplicationBuilder.Services.AddCors(corsOptions =>
			{
				corsOptions.AddPolicy("MyPolicy", corsPolicyBuilder =>
				{
					corsPolicyBuilder.AllowAnyHeader().AllowAnyMethod().WithOrigins(webApplicationBuilder.Configuration["FrontBaseUrl"]);
				});
			});

			#endregion

			var app = webApplicationBuilder.Build();

			using var scope = app.Services.CreateScope();

			var services = scope.ServiceProvider;

			var _dbContext = services.GetRequiredService<StoreContext>();
			var _identityDbContext = services.GetRequiredService<AppIdentityDbContext>();
			// ASK CLR for Creating Object from DbContext Explicitly

			var loggerFactory = services.GetRequiredService<ILoggerFactory>();
			var logger = loggerFactory.CreateLogger<Program>();

			try
			{
				await _dbContext.Database.MigrateAsync(); // Update-Database
				await StoreContextSeed.SeedAsync(_dbContext); // Data Seeding

				await _identityDbContext.Database.MigrateAsync(); // Update-Database

				var _userManager = services.GetRequiredService<UserManager<AppUser>>(); // Explicitly 
				await AppIdentityDbContextSeed.SeedUsersAsync(_userManager);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "an error has been occured during apply the migration");

			}

			#region Configure Kestrel Middlewares
			// Configure the HTTP request pipeline.

			app.UseMiddleware<ExceptionMiddleware>();

			if (app.Environment.IsDevelopment())
			{
			}
				app.UseSwaggerMiddlewares();

			app.UseStatusCodePagesWithReExecute("/errors/{0}");

			app.UseHttpsRedirection();

			app.UseStaticFiles();

			app.UseCors("MyPolicy");

			app.MapControllers();

			app.UseAuthentication();

			app.UseAuthorization();

			#endregion

			app.Run();
		}
	}
}