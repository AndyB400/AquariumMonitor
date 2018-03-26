using System;
using AquariumMonitor.DAL;
using AquariumMonitor.DAL.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AquariumMonitor.BusinessLogic.Interfaces;
using AquariumMonitor.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using Pwned;
using Serilog;

namespace AquariumAPI
{
    public class Startup
    {
        private IHostingEnvironment _env;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var tokenSecurityKey = Environment.GetEnvironmentVariable(Constants.TokenSecurityKey);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration[Constants.ValidIssuer],
                        ValidAudience = Configuration[Constants.ValidAudience],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecurityKey))
                    };
                });

            services.AddAuthorization(cfg =>
            {
                cfg.AddPolicy("Admin", p => p.RequireClaim("Admin", "True"));
            });

            services.AddCors(cfg =>
            {
                cfg.AddPolicy(Configuration[Constants.CorsPolicyName], bldr =>
                {
                    bldr.AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins(Configuration[Constants.CorsOrigins]);
                });
            });

            // Add authenticated policy so it can be globally added to all controllers
            var authenticatedPolicy = new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .Build();

            services.AddMvc(opt =>
                {
                    if (! _env.IsProduction())
                    {
                        opt.SslPort = 44308;
                    }
                    opt.Filters.Add(new RequireHttpsAttribute());
                    opt.Filters.Add(new AuthorizeFilter(authenticatedPolicy));
                })
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            services.AddApiVersioning(cfg =>
            {
                cfg.DefaultApiVersion = new ApiVersion(1, 0);
                cfg.AssumeDefaultVersionWhenUnspecified = true;
                cfg.ReportApiVersions = true;
                cfg.ApiVersionReader = new HeaderApiVersionReader(Constants.APIVersionHeaderName);
            });

            // Add application services.
            services.AddScoped<IConnectionFactory>(s => new ConnectionFactory(Configuration.GetConnectionString(Constants.DBConnectionName)));

            services.AddAutoMapper();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IValidationManager, ValidationManager>();
            services.AddScoped<IHaveIBeenPwnedRestClient, HaveIBeenPwnedRestClient>();

            // Repositories
            services.AddScoped<IAquariumRepository, AquariumRepository>();
            services.AddScoped<IMeasurementRepository, MeasurementRepository>();
            services.AddScoped<IWaterChangeRepository, WaterChangeRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAquariumTypeRepository, AquariumTypeRepository>();
            services.AddScoped<IUnitRepository, UnitRepository>();
            services.AddScoped<IPasswordRepository, PasswordRepository>();

            // Managers
            services.AddScoped<IUnitManager, UnitManager>();
            services.AddScoped<IAquariumTypeManager, AquariumTypeManager>();
            services.AddScoped<IMeasurementManager, MeasurementManager>();
            services.AddScoped<IPasswordManager, PasswordManager>();
            services.AddScoped<IAuthManager, AuthManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                loggerFactory.AddSerilog(new LoggerConfiguration()
                           .MinimumLevel.Information()
                           .Enrich.WithEnvironmentUserName()
                           .Enrich.WithMachineName()
                           .WriteTo.MSSqlServer(connectionString: Configuration.GetConnectionString(Constants.DBConnectionName), tableName: Constants.SerilogTableName)
                           .CreateLogger());
            }
            if (env.IsStaging())
            {
                app.UseDeveloperExceptionPage();

                loggerFactory.AddSerilog(new LoggerConfiguration()
                           .MinimumLevel.Information()
                           .Enrich.WithEnvironmentUserName()
                           .Enrich.WithMachineName()
                           .WriteTo.MSSqlServer(connectionString: Configuration.GetConnectionString(Constants.DBConnectionName), tableName: Constants.SerilogTableName)
                           .CreateLogger());
            }
            else if (env.IsProduction())
            {
                loggerFactory.AddSerilog(new LoggerConfiguration()
                            .MinimumLevel.Warning()
                            .Enrich.WithEnvironmentUserName()
                            .Enrich.WithMachineName()
                            .WriteTo.MSSqlServer(connectionString: Configuration.GetConnectionString(Constants.DBConnectionName), tableName: Constants.SerilogTableName)
                            .CreateLogger());
            }

            app.UseCors(Configuration[Constants.CorsPolicyName]);
            app.UseMvc();
        }
    }
}
