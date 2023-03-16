using AutoMapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SocialMedia.core.CutomEntities;
using SocialMedia.core.Interfaces;
using SocialMedia.core.Services;
using SocialMedia.infrastructure.Data;
using SocialMedia.infrastructure.Filters;
using SocialMedia.infrastructure.Interfaces;
using SocialMedia.infrastructure.Repositories;
using SocialMedia.infrastructure.Servicies;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace SocialMedia.api
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
            //services.AddScoped<IRepository, MemoryRepository>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); //esto buscara los mappeaos que tengamos creados o definidos en la clase AutoMapperProfile para poder ejecutarlos

            services.AddControllers(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            })
            .ConfigureApiBehaviorOptions(options => 
            options.SuppressModelStateInvalidFilter = true); // con esto indicaremos que el postcontroller, o cualquier controlador que creemos siga utilizacon el apicontroller, pero esta funcionalidad que nos ofrece tambien el apicontroller no

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Social Media API", Version = "v1" }); //con esto creamos el acceso a la documentacion de la api utilizando la url swagger/v1/swagger.json
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"; //obtenemos el archivo xml
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile) ; //obtenemos la ruta que vamos a necesitar
                c.IncludeXmlComments(xmlPath); //incluye los comentarios del xml de la ruta dada en ladocumentacion creada de swagger, es decir, cuando comentemos los distintos metodos CRUD como si fuese javadoc, se mostrara en la documentacion dicha
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Authentication:Issuer"],
                    ValidAudience = Configuration["Authentication:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication:SecretKey"]))

                };
            });

            services.AddMvc(options =>
            {
                options.Filters.Add<ValidationFilter>();
            }).AddFluentValidation(options =>
            {
                options.RegisterValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            });

            services.Configure<PaginationOptions>(Configuration.GetSection("Pagination"));
            services.Configure< SocialMedia.infrastructure.Options.PasswordOptions >(Configuration.GetSection("PasswordOptions"));

            services.AddDbContext<SocialMediaContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("SocialMedia"))
            );

            
            
            services.AddTransient<IPostRepository, PostRepository>();
            //services.AddTransient<IUserRepository, UserRepository>();
            
            services.AddTransient<IPostService, PostService>();
            services.AddTransient<ISecurityService, SecurityService>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
            services.AddSingleton<IPasswordService, PasswordService>();
            services.AddSingleton<IUriService>(provider =>
            {
                var accesor = provider.GetRequiredService<IHttpContextAccessor>();
                var request = accesor.HttpContext.Request;
                var absoluteUri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent());
                return new UriService(absoluteUri);
            });
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Documentation"); //con esto generamos la interfaz de swagger para nuestra api y asi poder probarla desde esta interfaz
                options.RoutePrefix = string.Empty; //esto hara que poniendo la url raiz acceda directamente a la interfaz swagger de nuestra api
            });

            

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
