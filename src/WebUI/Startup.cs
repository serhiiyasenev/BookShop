using BusinessLayer.Interfaces;
using BusinessLayer.Mappings;
using BusinessLayer.Models.Files;
using BusinessLayer.Services;
using DataAccessLayer;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;

namespace WebUI
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
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IFileUploadService, FileUploadLocalService>();

            services.Configure<AllowedExtensions>(Configuration.GetSection(nameof(AllowedExtensions)));
            services.Configure<ImageStorageSettings>(Configuration.GetSection(nameof(ImageStorageSettings)));

            services.AddDbContext<EfCoreContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("Default")));

            services.AddControllersWithViews();

            services.AddAutoMapper(new[] { Assembly.GetAssembly(typeof(BookingProfile)) });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts(); ??
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            //TODO: get path from Configuration
            var pathString = "/ImageStorage";
            var path = Path.Combine(Directory.GetCurrentDirectory(), $"..{pathString}");

            app.UseStaticFiles(new StaticFileOptions 
            {
                FileProvider = new PhysicalFileProvider(path),
                RequestPath = new PathString(pathString) 
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(path),
                RequestPath = new PathString(pathString)
            });

            app.UseRouting();
			
			//app.AddCors

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}");
            });
        }
    }
}
