using MassTransit;
using TroLiKOC.API;
using TroLiKOC.API.Extensions;
using TroLiKOC.JobOrchestrator.Configuration;
using TroLiKOC.Modules.Jobs.Contracts.Messages;
using TroLiKOC.SharedKernel;

var builder = WebApplication.CreateBuilder(args);

// SHARED KERNEL
builder.Services.AddSharedKernel(builder.Configuration);

// REGISTER MODULES (Modular Monolith)
builder.Services.AddModularMonolith(builder.Configuration);

// JOB ORCHESTRATOR (Quartz.NET)
builder.Services.AddJobOrchestrator(builder.Configuration);

// MASSTRANSIT + RABBITMQ
builder.Services.AddMassTransit(x =>
{
    // Register Consumers
    x.AddConsumer<TroLiKOC.Modules.Jobs.Infrastructure.Messaging.Consumers.JobCompletedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");
        
        cfg.Host(rabbitConfig["Host"], "/", h =>
        {
            h.Username(rabbitConfig["Username"]);
            h.Password(rabbitConfig["Password"]);
        });

        // Configure Configure Publish Topologies for Job Requests
        cfg.Message<TalkingHeadRequest>(m => m.SetEntityName("job-requests"));
        cfg.Publish<TalkingHeadRequest>(p => p.ExchangeType = "topic");
        
        cfg.Message<VirtualTryOnRequest>(m => m.SetEntityName("job-requests"));
        cfg.Publish<VirtualTryOnRequest>(p => p.ExchangeType = "topic");
        
        cfg.Message<ImageToVideoRequest>(m => m.SetEntityName("job-requests"));
        cfg.Publish<ImageToVideoRequest>(p => p.ExchangeType = "topic");
        
        cfg.Message<MotionTransferRequest>(m => m.SetEntityName("job-requests"));
        cfg.Publish<MotionTransferRequest>(p => p.ExchangeType = "topic");
        
        cfg.Message<FaceSwapRequest>(m => m.SetEntityName("job-requests"));
        cfg.Publish<FaceSwapRequest>(p => p.ExchangeType = "topic");

        cfg.ConfigureEndpoints(context);
    });
});

// API CONFIGURATION
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "TroLiKOC_SuperSecretKey_2026_MinLength32Chars!";
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "TroLiKOC",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "TroLiKOC",
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSecretKey))
        };
        
        // SignalR Token Support
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hubs")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Apply Migrations
app.ApplyMigrations();

app.UseCors(b => b
        .WithOrigins("http://localhost:3000", "https://tro-ly-koc.nhatcuong.io.vn")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());

// MIDDLEWARE PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TroLiKOC.API.Hubs.JobHub>("/hubs/jobs");

app.Run();
