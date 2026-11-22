using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Controllers;
using backend.Data;
using backend.Entities;
using backend.Repositories;
using backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.Integration;

public class UserIntegrationTests
{
    private readonly AppDbContext _context;
    private readonly UsersController _controller;

    public UserIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        
        var usersRepository = new UsersRepository(_context);
        var service = new UserService(usersRepository);
        _controller = new UsersController(service);
    }
    
    private void SignInWith(string? sub, string? email)
    {
        var claims = new List<Claim>();
        if (sub != null) claims.Add(new Claim("sub", sub));
        if (email != null) claims.Add(new Claim("https://grocery-price-comparison-api/email", email));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };
    }

    // Jonas (google-oauth2|1048, jonas@gmail.com) calls GetMe for the first time: he gets a user with his id and email, and AppUsers has one row.
    [Fact]
    public async Task FirstRequest_CreatesUserFromToken()
    {
        SignInWith("google-oauth2|1048", "jonas@gmail.com");
        var result = await _controller.GetMe();
        var ok = Assert.IsType<OkObjectResult>(result);
        var user = Assert.IsType<AppUser>(ok.Value);
        Assert.Equal("google-oauth2|1048", user.Auth0Id);
        Assert.Equal("jonas@gmail.com", user.Email);
        Assert.Single(_context.AppUsers);
    }
    
    // Jonas calls GetMe twice: both calls return the same user Id, and AppUsers still has one row.
    [Fact]
    public async Task SecondRequest_ReturnsSameUser()
    {
        SignInWith("google-oauth2|1048", "jonas@gmail.com");

        var firstResult = await _controller.GetMe();
        var secondResult = await _controller.GetMe();

        var firstUser = Assert.IsType<AppUser>(Assert.IsType<OkObjectResult>(firstResult).Value);
        var secondUser = Assert.IsType<AppUser>(Assert.IsType<OkObjectResult>(secondResult).Value);
        Assert.Equal(firstUser.Id, secondUser.Id);
        Assert.Single(_context.AppUsers);
    }
    
    // Jonas's token has no email claim (the Auth0 Action is missing): GetMe throws InvalidOperationException and AppUsers stays empty.
    [Fact]
    public async Task TokenWithoutEmail_Throws()
    {
        SignInWith("google-oauth2|1048", null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetMe());

        Assert.Empty(_context.AppUsers);
    }
}