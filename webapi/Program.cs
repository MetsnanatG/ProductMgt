using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using webapi.AppDbContext;
using webapi.Models;
using webapi.Service;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddDbContext<ProductManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<Productservice>();
builder.Services.AddScoped<AuthService>();

// Identity services
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ProductManagementContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});


builder. Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

// Apply migrations and seed roles/users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<ProductManagementContext>();
        await context.Database.MigrateAsync();  // Ensure the database is up to date

        // Call SeedData method to create roles and users
        await SeedData(services);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();




// SeedData method
async Task SeedData(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

    string[] roles = { "Tester", "TestLead", "Manager" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Create a default user (this is optional and you can customize this)
    var user = new User
    {
        UserName = "testuser",
        Email = "testuser@example.com",
        Role = "Tester"
    };
    string userPassword = "Password123!";

    var existingUser = await userManager.FindByEmailAsync(user.Email);
    if (existingUser == null)
    {
        var result = await userManager.CreateAsync(user, userPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Tester");
        }
    }
}



///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
