using MANASI;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// SERVICES
// =========================================================

builder.Services.AddSingleton<Services>();

builder.Services.AddControllers();

// =========================================================
// CORS
// =========================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
    );
});

// =========================================================
// BUILD APP
// =========================================================

var app = builder.Build();

// =========================================================
// MIDDLEWARE
// =========================================================

app.UseCors("AllowAll");

app.UseDefaultFiles();

app.UseStaticFiles();

app.MapControllers();

// =========================================================
// RUN
// =========================================================

app.Run();

