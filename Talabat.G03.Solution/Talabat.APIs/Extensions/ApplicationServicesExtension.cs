using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Talabat.APIs.Errors;
using Talabat.APIs.Helpers;
using Talabat.Core;
using Talabat.Core.Repositories;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Services.Contract;
using Talabat.Repository;
using Talabat.Service;

namespace Talabat.APIs.Extensions
{
	public static class ApplicationServicesExtension
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			services.AddSingleton(typeof(IResponseCacheService), typeof(ResponseCacheService));

			services.AddScoped(typeof(IPaymentService), typeof(PaymentService));

			services.AddScoped(typeof(IProductService), typeof(ProductService));

			services.AddScoped(typeof(IOrderService), typeof(OrderService));

			services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));

			//services.AddScoped<IBasketRepository, BasketRepository>();
			services.AddScoped(typeof(IBasketRepository), typeof(BasketRepository));

			//services.AddScoped<IGenericRepository<Product>, GenericRepository<Product>>();
			//services.AddScoped<IGenericRepository<ProductBrand>, GenericRepository<ProductBrand>>();
			//services.AddScoped<IGenericRepository<ProductType>, GenericRepository<ProductType>>();

			//services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));


			//services.AddAutoMapper(M => M.AddProfile(new MappingProfiles()));

			services.AddAutoMapper(typeof(MappingProfiles));

			services.Configure<ApiBehaviorOptions>(options =>
			{
				options.InvalidModelStateResponseFactory = (actionContext) =>
				{

					var errors = actionContext.ModelState.Where(P => P.Value.Errors.Count > 0)
														 .SelectMany(P => P.Value.Errors)
														 .Select(E => E.ErrorMessage)
														 .ToList();

					var validationErrorResponse = new ApiValidationErrorResponse()
					{
						Errors = errors
					};

					return new BadRequestObjectResult(validationErrorResponse);
				};
			});

			return services;

		}
	}
}
