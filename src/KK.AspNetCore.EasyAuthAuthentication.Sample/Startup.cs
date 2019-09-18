namespace KK.AspNetCore.EasyAuthAuthentication.Sample
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SpaServices.AngularCli;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using KK.AspNetCore.EasyAuthAuthentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Authentication;
    using KK.AspNetCore.EasyAuthAuthentication.Sample.Transformers;
    using KK.AspNetCore.EasyAuthAuthentication.Sample.Repositories;
    using System.Security.Claims;

    public class Startup
    {
        public Startup(
            IConfiguration configuration,
            IHostingEnvironment environment)
        {
            this.Configuration = configuration;
            this.Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

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

            _ = services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration => configuration.RootPath = "ClientApp/dist");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                _ = app.UseDeveloperExceptionPage();
            }
            else
            {
                _ = app.UseExceptionHandler("/Error");
                _ = app.UseHsts();
                _ = app.UseHttpsRedirection();
            }

            _ = app.UseStaticFiles();
            app.UseSpaStaticFiles();

            // Add Authentication middleware before MVC to get it working for MVC routes
            _ = app.UseAuthentication();

            _ = app.UseMvc(routes => _ = routes.MapRoute(
                      name: "default",
                      template: "{controller}/{action=Index}/{id?}")
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
