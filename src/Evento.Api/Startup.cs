using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evento.Core.Repositories;
using Evento.Infrastructure.Mappers;
using Evento.Infrastructure.Repositories;
using Evento.Infrastructure.Services;
using Evento.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Evento.Api
{
    public class Startup
    {
        /* 
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
*/
        public Startup(IHostingEnvironment env)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration {get;}
        
        //public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
            .AddJsonOptions(x => x.SerializerSettings.Formatting = Formatting.Indented);
            //services.AddTransient -- za kazdym razem utworzy mam nowy obiekt
            services.AddAuthentication();
            services.AddScoped<IEventRepository, EventRepository>();   // pojedyncze rzadznie dla calego http,  per request http
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IEventService, EventService>();
            services.AddSingleton(AutoMapperConfig.Initialize());
            services.AddSingleton<IJwtHandler, JwtHandler>();
            services.AddScoped<IUserService, UserService>();
            services.Configure<JwtSettings>(Configuration.GetSection("JwtSettings"));

            var serviceProvider = services.BuildServiceProvider();
            var jwt = serviceProvider.GetService<IOptions<JwtSettings>>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JwtBearer";
                options.DefaultChallengeScheme = "JwtBearer";
            })
            .AddJwtBearer("JwtBearer", jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    //ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Value.Key)),

                    ValidateIssuer = false,
                    ValidIssuer = jwt.Value.Issuer

                    //ValidateAudience = true,
                    //ValidAudience = "The name of the audience",

                    //ValidateLifetime = true //validate the expiration and not before values in the token

                    //ClockSkew = TimeSpan.FromMinutes(5) //5 minute tolerance for the expiration date
                };
            });
        }

        JwtSettings jwtSettings;
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            jwtSettings = app.ApplicationServices.GetService<JwtSettings>();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
