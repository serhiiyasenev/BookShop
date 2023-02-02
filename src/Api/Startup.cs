using BusinessLayer.Interfaces;
using BusinessLayer.Models.Outbound;
using BusinessLayer.Profiles;
using BusinessLayer.Services;
using DataAccessLayer;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.IO;
using System;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Api
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
            services.AddScoped<IProductRepository, ProductDbRepository>();
            services.AddScoped<IBookingRepository, BookingDbRepository>();

            services.AddScoped<IProductService<ProductInbound, ProductOutbound>, ProductService<ProductInbound, ProductOutbound>>();
            services.AddScoped<IBookingService<BookingInbound, BookingOutbound>, BookingService<BookingInbound, BookingOutbound>>();

            services.AddDbContext<EfCoreContext>(options => 
                options.UseSqlServer(Configuration.GetConnectionString("Default")));

            var assemblies = new[] { Assembly.GetAssembly(typeof(BookingProfile)) };
            services.AddAutoMapper(assemblies);

            services.AddControllers().AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "BookShop", Version = "v1",
                    Description = "Use ISO date format 'yyyy-MM-dd'"
                });
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
