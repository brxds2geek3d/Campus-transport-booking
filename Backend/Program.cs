using Backend.Controllers;
using Backend.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Enable CORS so your frontend HTML/JS can talk to this API
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use the CORS policy
app.UseCors("AllowAll");

app.UseAuthorization();

// This line automatically connects your Controllers to their URLs
app.MapControllers();

app.Run();