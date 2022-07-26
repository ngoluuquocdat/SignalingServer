using SignalingServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddSingleton<List<string>>(opts => new List<string>());

// services cors
builder.Services.AddCors(p => p.AddPolicy("MyCorsPolicy", builder =>
{
    builder.WithOrigins("http://localhost:3000", "https://tourmaline-pastelito-ff7c52.netlify.app").AllowAnyMethod().AllowAnyHeader().AllowCredentials();  
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("MyCorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.MapHub<SignalRtcHub>("/signalrtc");

app.Run();
