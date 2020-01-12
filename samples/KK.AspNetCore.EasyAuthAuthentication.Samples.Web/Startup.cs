namespace KK.AspNetCore.EasyAuthAuthentication.Samples.Web
{
    using KK.AspNetCore.EasyAuthAuthentication.Samples.Web.Repositories;
    using KK.AspNetCore.EasyAuthAuthentication.Samples.Web.Transformers;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SpaServices.AngularCli;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        public Startup(IConfiguration configuration) => this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _ = services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            _ = services.AddScoped<IClaimsTransformation, ClaimsTransformer>();

            _ = services.AddAuthentication(
                options =>
                {
                    options.DefaultAuthenticateScheme = EasyAuthAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = EasyAuthAuthenticationDefaults.AuthenticationScheme;
                }
            ).AddEasyAuth(this.Configuration);

            _ = services.AddSingleton<IRepository, Repository>();

            _ = services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(
                configuration => configuration.RootPath = "ClientApp/dist"
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                _ = app.UseDeveloperExceptionPage();
            }
            else
            {
                _ = app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                _ = app.UseHsts();
            }

            _ = app.UseHttpsRedirection();
            _ = app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            // Add Authentication middleware before MVC to get it working for MVC routes
            _ = app.UseAuthentication();

            _ = app.UseRouting();

            _ = app.UseEndpoints(
                endpoints => endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}")
            );

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
