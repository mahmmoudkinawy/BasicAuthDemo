using BasicAuthDemo.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BasicAuthDemo.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly ITokenService _tokenService;
    private readonly SignInManager<UserEntity> _signInManager;

    public AccountController(
        UserManager<UserEntity> userManager,
        ITokenService tokenService,
        SignInManager<UserEntity> signInManager
        )
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _signInManager = signInManager;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] UserForRegisterDto userForRegisterDto)
    {
        var user = new UserEntity
        {
            FirstName = userForRegisterDto.FirstName,
            LastName = userForRegisterDto.LastName,
            Email = userForRegisterDto.Email,
            UserName = userForRegisterDto.Email
        };

        var result = await _userManager.CreateAsync(user, userForRegisterDto.Password);

        await _userManager.AddToRoleAsync(user, "Client");

        //Email 
        if (!result.Succeeded)
        {
            var errors = new List<string>();
            foreach (var error in result.Errors)
            {
                errors.Add(error.Description);
            }

            return BadRequest(errors);
        }

        return Ok(await _tokenService.CreateTokenAsync(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] UserForLoginDto userForLoginDto)
    {
        var user = await _userManager.FindByEmailAsync(userForLoginDto.Email);

        if (user is null)
        {
            return Unauthorized("Email or password in correct.");
        }

        var result = await _userManager.CheckPasswordAsync(user, userForLoginDto.Password);
        //var result2 = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDto.Password, 3);

        if (!result)
        {
            return Unauthorized("Email or password in correct.");
        }

        return Ok(await _tokenService.CreateTokenAsync(user));
    }



    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request)
    {
        var d = User.Claims;
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var user = await _userManager.FindByIdAsync(userId.ToString());

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = new List<string>();
            foreach (var error in result.Errors)
            {
                errors.Add(error.Description);
            }

            return BadRequest(errors);
        }

        return Ok("Changed");
    }

}



public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; }

    [Required]
    public string NewPassword { get; set; }
}






public class UserForLoginDto
{

    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string? Password { get; set; }
}

public class UserForRegisterDto
{
    [Required]
    public string? FirstName { get; set; }

    [Required]
    public string? LastName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string? Password { get; set; }
}
