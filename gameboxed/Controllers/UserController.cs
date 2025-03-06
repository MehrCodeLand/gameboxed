using Api.Leyer.DTOs;
using Application.Leyer.Interfaces;
using Domain.Leyer.Entities;
using gameboxed.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace gameboxed.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController : Controller
{
    private readonly IUserRepository _userRepo;

    public UserController(IUserRepository userRepository)
    {
        _userRepo = userRepository;
    }


    [HttpPost]
    public async Task<IActionResult> Register(UserRegisterDto dto)
    {
        var result = await _userRepo.RegisterASync(dto);

        
        // time to implement Register User


        return Ok();
    }
}
