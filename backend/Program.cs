using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapControllers();

// --- AUTOMATIC SEEDING ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // If no users exist, create the test user required by the Controller
    if (!context.Set<User>().Any())
    {
        context.Set<User>().Add(new User
        {
            Id = 1, // Explicitly set ID to match the hardcoded value in TasksController
            Email = "test@test.com",
            PasswordHash = "dummyhash"
        });
        context.SaveChanges();
    }
}
// -------------------------

app.Run();
