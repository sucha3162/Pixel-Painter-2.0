using Microsoft.Extensions.Configuration;
using MyTestVueApp.Server.Configuration;
using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.ServiceImplementations;
using System.Runtime;
using MyTestVueApp.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();


builder.Services.Configure<ApplicationConfiguration>(builder.Configuration.GetSection("ApplicationConfiguration"));

//Custom Services
builder.Services.AddTransient<IArtAccessService, ArtAccessService>();
builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<ILikeService, LikeService>();
builder.Services.AddTransient<ICommentAccessService, CommentAccessService>();


var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.MapHub<SignalHub>("/signalHub");

app.Run();
