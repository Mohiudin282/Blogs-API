using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//Get the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ConduitContext>(options => 
    options.UseSqlite(connectionString));

builder.Services.AddControllers();
var app = builder.Build();

app.MapControllers();

app.Run();