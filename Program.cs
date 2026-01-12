using GoogleDriveIntegration.src.Common;
using Microsoft.AspNetCore.Http.Features;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.Configure<FormOptions>(
    builder.Configuration.GetSection("FormOptions"));

WebApplication app = builder.Build();
app.UseCors("AngularPolicy"); 

app.UseHttpsRedirection();

app.MapEndpoints();

app.Run();