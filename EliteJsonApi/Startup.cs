using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EliteJsonApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;

namespace EliteJsonApi
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // Added during migration to .NET Core 2.1; we're not using cookies yet.
                options.CheckConsentNeeded = ctx => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });

            services.AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            var connectionString = Configuration.GetConnectionString("EliteDbContext");
            services.AddEntityFrameworkNpgsql().AddDbContext<EliteDbContext>(options => options.UseNpgsql(connectionString));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();
            //app.UseHttpsRedirection(); // Added in 2.1, we don't need it, but should implement at some point regardless
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                //app.UseHsts(); // Added in 2.1, we don't need it, but should implement at some point regardless
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "/{controller=MaterialFinder}/{action=Default}/{id?}");
            });

            // No need to rebuild the database from the CLI every time a change is made
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<EliteDbContext>();
                context.Database.Migrate();
            }
        }
    }
}
