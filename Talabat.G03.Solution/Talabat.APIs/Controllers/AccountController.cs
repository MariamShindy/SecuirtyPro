﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talabat.APIs.DTOs;
using Talabat.APIs.Errors;
using Talabat.APIs.Extensions;
using Talabat.APIs.Helpers;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Services.Contract;

namespace Talabat.APIs.Controllers
{
	public class AccountController : BaseApiController
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly IAuthService _authService;
		private readonly IMapper _mapper;

		public AccountController(
			UserManager<AppUser> userManager, 
			SignInManager<AppUser> signInManager,
			IAuthService authService,
			IMapper mapper
			)
        {
			_userManager = userManager;
			_signInManager = signInManager;
			_authService = authService;
			_mapper = mapper;
		}

		[HttpPost("login")] // POST: /api/account/login
		public async Task<ActionResult<UserDto>> Login(LoginDto model)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user is null) 
				return Unauthorized(new ApiResponse(401));

			var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

			if (result.Succeeded is false)
				return Unauthorized(new ApiResponse(401));

			return Ok(new UserDto()
			{
				DisplayName = user.DisplayName,
				Email = user.Email,
				Token = await _authService.CreateTokenAsync(user, _userManager)
			});
		}

		[HttpPost("register")] // POST: /api/account/register
		public async Task<ActionResult<UserDto>> Register(RegisterDto model)
		{

			if(CheckEmailExists(model.Email).Result.Value)
				return BadRequest(new ApiValidationErrorResponse() { Errors = new[] {"this email is already in user"}});

			var user = new AppUser()
			{
				DisplayName = EncryptionService.Encrypt( model.DisplayName),
				Email = model.Email,
				UserName = model.Email.Split("@")[0],
				PhoneNumber = model.PhoneNumber
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (result.Succeeded is false) return BadRequest(new ApiResponse(400));

			return Ok(new UserDto()
			{
				DisplayName = EncryptionService.Decrypt(user.DisplayName),
				Email = user.Email,
				Token = await _authService.CreateTokenAsync(user, _userManager)
			});
		}

		[Authorize]
		[HttpGet] // GET : /api/accounts
		public async Task<ActionResult<UserDto>> GetCurrentUser()
		{
			var email = User.FindFirstValue(ClaimTypes.Email);
			var user = await _userManager.FindByEmailAsync(email);

			return Ok(new UserDto()
			{
				DisplayName = EncryptionService.Decrypt(user.DisplayName),
				Email = user.Email,
				Token = await _authService.CreateTokenAsync(user, _userManager)
			});
		}

		[Authorize]
		[HttpGet("address")] // GET : /api/accounts/address
		public async Task<ActionResult<AddressDto>> GetAddress()
		{
			var user = await _userManager.FindUserWithAddressAsync(User);

			//var mappedAddress = _mapper.Map<Address, AddressDto>(user.Address);
			var mappedAddress = _mapper.Map<AddressDto>(user.Address);


			return Ok(mappedAddress);
		}


		[Authorize]
		[HttpPut("address")] // PUT : /api/accounts/address
		public async Task<ActionResult<AddressDto>> UpdateAddress(AddressDto address)
		{
			var updatedAddress = _mapper.Map<AddressDto, Address>(address);

			var user = await _userManager.FindUserWithAddressAsync(User);

			if(user.Address is not null)
				updatedAddress.Id = user.Address.Id;

			user.Address = updatedAddress;


			var result = await _userManager.UpdateAsync(user);

			if (!result.Succeeded) return BadRequest(new ApiResponse(400));

			return Ok(address);
		}


		[HttpGet("emailexists")] // GET : /api/accounts/emailexists?email=ahmed.nasr@linkdev.com
		public async Task<ActionResult<bool>> CheckEmailExists(string email)
		{
			return await _userManager.FindByEmailAsync(email) is not null;
		}
	}
}
