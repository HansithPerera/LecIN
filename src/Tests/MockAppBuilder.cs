using Backend;
using Backend.Auth;
using Backend.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Program = Backend.Program;

namespace Tests;

public class MockAppBuilder : WebApplicationFactory<Program>
{
    
}