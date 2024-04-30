using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Talabat.APIs.Errors;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Services.Contract;
using Talabat.Service;

namespace Talabat.APIs.Controllers
{
	[Authorize]
	public class PaymentsController : BaseApiController
	{
		private readonly IPaymentService _paymentService;
		private readonly ILogger<PaymentService> _logger;
		private const string _whSecret = "whsec_dde6e561593e785d449ad6ca3596e95b7768080d1fee76ca39839d19df1a203a";

		public PaymentsController(IPaymentService paymentService, ILogger<PaymentService> logger)
		{
			_paymentService = paymentService;
			_logger = logger;
		}

		[HttpPost("{basketId}")] // POST : /api/Payments/basketId
		public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
		{
			var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);

			if (basket is null) return BadRequest(new ApiResponse(400, "Problem With Your Basket"));

			return Ok(basket);
		}

		[AllowAnonymous]
		[HttpPost("webhook")]
		public async Task<ActionResult> StripeWebhook()
		{
			var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

			var stripeEvent = EventUtility.ConstructEvent(json,
				Request.Headers["Stripe-Signature"], _whSecret);

			var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;

			Order? order;

			// Handle the event
			switch (stripeEvent.Type)
			{

				case Events.PaymentIntentSucceeded:
					_logger.LogInformation("Payment Succeeded ya Hamda", paymentIntent.Id);
					order = await _paymentService.UpdateOrderStatusAsync(paymentIntent.Id, true);
					_logger.LogInformation("Order Status Updated to Payment Received", order.Id);
					break;
				case Events.PaymentIntentPaymentFailed:
					_logger.LogInformation("Payment Failed ya Hamda :(", paymentIntent.Id);
					order = await _paymentService.UpdateOrderStatusAsync(paymentIntent.Id, false);
					_logger.LogInformation("Order Status Updated to Payment Failed", order.Id);
					break;
			}

			return Ok();
		}
	}
}
