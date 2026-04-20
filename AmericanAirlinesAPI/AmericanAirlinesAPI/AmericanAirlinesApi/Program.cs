using Microsoft.EntityFrameworkCore;
using AmericanAirlinesApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = 
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Banco de dados SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=AmericanAirlines.db"));

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "AmericanAirlinesSkyApi",
        Version = "v1",
        Description = "API de micro-gerenciamento de voos, tripulação, aeronaves e passageiros."
    });
});

var app = builder.Build();

// Garante que o banco SQLite é criado ao iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Swagger sempre ativo (inclusive em produção para fins acadêmicos)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AmericanAirlinesSkyApi v1");
    c.RoutePrefix = string.Empty; // Swagger na raiz: http://localhost:5000
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
