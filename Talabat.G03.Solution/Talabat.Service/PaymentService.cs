using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Stripe;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Services.Contract;
using Talabat.Core.Specifications.Order_Specs;
using Product = Talabat.Core.Entities.Product;

namespace Talabat.Service
{
	public class PaymentService : IPaymentService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IBasketRepository _basketsRepo;
		private readonly IConfiguration _configuration;

		public PaymentService(
			IUnitOfWork unitOfWork,
			IBasketRepository basketsRepo,
			IConfiguration configuration)
        {
			_unitOfWork = unitOfWork;
			_basketsRepo = basketsRepo;
			_configuration = configuration;
		}
        public async Task<CustomerBasket?> CreateOrUpdatePaymentIntent(string basketId)
		{
			StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];

			var basket = await _basketsRepo.GetBasketAsync(basketId);

			if (basket is null) return null;

			var shippingPrice = 0m;

			if (basket.DeliveryMethodId.HasValue)
			{
				var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(basket.DeliveryMethodId.Value);
				basket.ShippingPrice = deliveryMethod.Cost;
				shippingPrice = deliveryMethod.Cost;
			}


			if(basket.Items?.Count > 0)
			{
				var productsRepo = _unitOfWork.Repository<Product>();
				foreach (var item in basket.Items)
				{
					var product = await productsRepo.GetByIdAsync(item.Id);
					if (item.Price != product.Price)
						item.Price = product.Price;
				}
			}

			PaymentIntent paymentIntent;
			PaymentIntentService paymentIntentService = new PaymentIntentService();

			if(string.IsNullOrEmpty(basket.PaymentIntentId)) // Create New Payment Intent
			{
				var createOptions = new PaymentIntentCreateOptions()
				{
					Amount = (long) basket.Items.Sum(item => item.Price * 100 * item.Quantity) + (long) shippingPrice * 100,
					Currency = "usd",
					PaymentMethodTypes = new List<string>() { "card"}
				};

				paymentIntent = await paymentIntentService.CreateAsync(createOptions); // Integrate With Stripe
				basket.PaymentIntentId = paymentIntent.Id;
				basket.ClientSecret = paymentIntent.ClientSecret;
			}
			else // Update Existing Payment Intent With NEW Amount
			{
				var updateOptions = new PaymentIntentUpdateOptions()
				{
					Amount = (long)basket.Items.Sum(item => item.Price * 100 * item.Quantity) + (long)shippingPrice * 100,
				};

				await paymentIntentService.UpdateAsync(basket.PaymentIntentId, updateOptions);
			}

			await _basketsRepo.UpdateBasketAsync(basket);

			return basket;
		}

		public async Task<Order?> UpdateOrderStatusAsync(string paymentIntentId, bool isPaid)
		{
			var ordersRepo = _unitOfWork.Repository<Order>();

			var spec = new OrderWithPaymentIntentSpecifications(paymentIntentId);

			var order = await ordersRepo.GetEntityWithSpecAsync(spec);

			if (order == null) return null;

			if (isPaid)
				order.Status = OrderStatus.PaymentReceived;
			else
				order.Status = OrderStatus.PaymentFailed;

			ordersRepo.Update(order);

			await _unitOfWork.CompleteAsync();

			return order;

		}
	}
}
