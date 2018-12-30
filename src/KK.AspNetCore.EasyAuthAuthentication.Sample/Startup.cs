namespace KK.AspNetCore.EasyAuthAuthentication.Sample
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SpaServices.AngularCli;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using KK.AspNetCore.EasyAuthAuthentication;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Authentication;
    using KK.AspNetCore.EasyAuthAuthentication.Sample.Transformers;
    using KK.AspNetCore.EasyAuthAuthentication.Sample.Repositories;

    public class Startup
    {
        public Startup(
            IConfiguration configuration,
            IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IClaimsTransformation, ClaimsTransformer>();

            services.AddAuthentication(
                options =>
                {
                    options.DefaultAuthenticateScheme = EasyAuthAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = EasyAuthAuthenticationDefaults.AuthenticationScheme;
                }
            ).AddEasyAuth(
                options =>
                {
                    // Override the AuthEndpoint while developing
                    if (this.Environment.IsDevelopment())
                    {
                        // The me.json should be placed in 'wwwroot/.auth'
                        // The Static File Handler will not work with files without extension
                        // so the endpoint should point to a file with extension. Folders that
                        // start with '.' are considered hidden folders and do not get included
                        // in publish site output which is desirable in this case.
                        options.AuthEndpoint = ".auth/me.json";
                    }
                }
            );

            services.AddSingleton<IRepository, Repository>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            // Add Authentication middleware before MVC to get it working for MVC routes
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

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
