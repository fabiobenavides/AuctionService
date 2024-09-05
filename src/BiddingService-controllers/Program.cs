using BiddingService_controllers.Consumers;
using BiddingService_controllers.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Driver;
using MongoDB.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddMassTransit( x => 
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("bids", false));

    x.UsingRabbitMq((context, cfg) => 
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h => {
            h.Username(builder.Configuration.GetValue("RabbitMQ:Username", "guest"));
            h.Password(builder.Configuration.GetValue("RabbitMQ:Password", "guest"));
        });
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.RequireHttpsMetadata = false; //we are not using https for now
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
//Add the backgroup task
builder.Services.AddHostedService<CheckAuctionFinished>();

var app = builder.Build();

app.MapControllers();

await DB.InitAsync("BidDb", MongoClientSettings
    .FromConnectionString(builder.Configuration.GetConnectionString("BidDbConnection")));

app.Run();

