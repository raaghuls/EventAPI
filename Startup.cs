using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventAPI.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Swagger;
using EventAPI.CustomFormatters;
using EventAPI.CustomFilters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<EventDbContext>(options =>
            {
                // options.UseInMemoryDatabase(databaseName: "EventDb");
                options.UseSqlServer(Configuration.GetConnectionString("EventSqlConnection"));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Event API",
                    Version = "v1",
                    Contact = new Contact {Name = "Dwarak K",
                                           Email = "dwarakk@abc.com"
                                            }
                });
            });

            services.AddCors( c => {
                c.AddPolicy("MSPolicy", builder =>
                {
                    builder.WithOrigins("*.microsoft.com")
                            .AllowAnyMethod()
                            .AllowAnyHeader();

              });
                c.AddPolicy("SynPolicy", builder =>
                {
                    builder.WithOrigins("*.synergetics-india.com")
                            .WithMethods("GET")
                            .WithHeaders("Authorization", "Content-Type", "Accept");

                });
                c.AddPolicy("Others", builder =>
                {
                    builder.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();

                });
                c.DefaultPolicyName = "Others";
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options => {
                        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = Configuration.GetValue<string>("Jwt:Issuer"),
                            ValidAudience = Configuration.GetValue<string>("Jwt:Audience"),
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("Jwt:Secret")))
                        };
                    });

            services.AddMvc(c => {
                            c.Filters.Add(typeof(CustomExceptionHandler));
                            c.OutputFormatters.Add(new CsvCustomFormatter());
                           })
                       .AddXmlDataContractSerializerFormatters() //Xml is to be configured manually not default as in Web API 2(MVC) version
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            if(env.IsDevelopment())
            {
                app.UseSwaggerUI(c =>
               {
                   c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event API");
               });
            }

            //app.UseCors( c =>
            //{
            //    c.WithOrigins("*.microsoft.com")
            //    .AllowAnyMethod()
            //    .AllowAnyHeader();
            //    c.WithOrigins("*.synergetics-india.com")
            //   .WithMethods("GET")
            //   .WithHeaders("Authorization","Content-Type","Accept");
            //});

            app.UseCors();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
