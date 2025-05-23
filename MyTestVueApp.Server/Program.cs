using Microsoft.Extensions.Configuration;
using MyTestVueApp.Server.Configuration;
using MyTestVueApp.Server.Interfaces;
using MyTestVueApp.Server.ServiceImplementations;
using MyTestVueApp.Server.Hubs;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});

builder.Services.AddSignalR(sig => {
  sig.MaximumReceiveMessageSize = 524288;
});

builder.Services.Configure<ApplicationConfiguration>(builder.Configuration.GetSection("ApplicationConfiguration"));

//Custom Services
builder.Services.AddTransient<IArtAccessService, ArtAccessService>();
builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<ILikeService, LikeService>();
builder.Services.AddTransient<ICommentAccessService, CommentAccessService>();
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();

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
