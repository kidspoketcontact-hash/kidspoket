using System.Text;
using KidsPocket.Api.Auth;
using KidsPocket.Application.Features.Allowance.ReceiveAllowance;
using KidsPocket.Application.Features.Chore.ApproveChore;
using KidsPocket.Application.Features.Chore.CompleteChore;
using KidsPocket.Application.Features.Chore.CreateChore;
using KidsPocket.Application.Features.Chore.GetChildChores;
using KidsPocket.Application.Features.Decision.AllocateMoney;
using KidsPocket.Application.Features.Decision.GetChildPendingDecisions;
using KidsPocket.Application.Features.Decision.GetPendingDecision;
using KidsPocket.Application.Features.Goal.ContributeToGoal;
using KidsPocket.Application.Features.Goal.CreateGoal;
using KidsPocket.Application.Features.Goal.GetChildGoals;
using KidsPocket.Application.Features.Household.GetHouseholdChildren;
using KidsPocket.Application.Features.Ledger.GetChildBalance;
using KidsPocket.Application.Features.Reflection.SubmitReflection;
using KidsPocket.Application.Features.Reward.CreateReward;
using KidsPocket.Domain.Exceptions;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<KidsPocketDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("KidsPocketDb")));

// כל Handler נרשם ישירות (בלי MediatR) - בהתאם ל-Vertical Slice Architecture
builder.Services.AddScoped<ReceiveAllowanceHandler>();
builder.Services.AddScoped<GetPendingDecisionHandler>();
builder.Services.AddScoped<GetChildPendingDecisionsHandler>();
builder.Services.AddScoped<AllocateMoneyHandler>();
builder.Services.AddScoped<GetChildBalanceHandler>();
builder.Services.AddScoped<CreateGoalHandler>();
builder.Services.AddScoped<ContributeToGoalHandler>();
builder.Services.AddScoped<GetChildGoalsHandler>();
builder.Services.AddScoped<CreateChoreHandler>();
builder.Services.AddScoped<CompleteChoreHandler>();
builder.Services.AddScoped<ApproveChoreHandler>();
builder.Services.AddScoped<GetChildChoresHandler>();
builder.Services.AddScoped<CreateRewardHandler>();
builder.Services.AddScoped<SubmitReflectionHandler>();
builder.Services.AddScoped<GetHouseholdChildrenHandler>();

builder.Services.AddSingleton<TokenService>();
builder.Services.AddHttpClient<ExternalAuthService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ממיר DomainException ל-400 עקבי בכל ה-API, במקום try/catch בכל Controller
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (DomainException ex)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// חשיפת Program כטיפוס public כדי ש-WebApplicationFactory<Program> ב-Api.Tests יוכל לגשת אליו
public partial class Program;
