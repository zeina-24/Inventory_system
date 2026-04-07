using ImsPosSystem.Api.Middleware;
using ImsPosSystem.Infrastructure.Extensions;
using ImsPosSystem.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── Infrastructure (DbContext + IUnitOfWork) ─────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Controllers + JSON options ───────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ── Swagger / OpenAPI Configuration ──────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ImsPosSystem API",
        Version = "v1",
        Description = "Inventory Management and POS System API",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Admin",
            Email = "admin@example.com"
        }
    });
});

// ── CORS (permit all in dev) ─────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ── Data Seeding ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.Initialize(context);
}

// ── Middleware pipeline ──────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
