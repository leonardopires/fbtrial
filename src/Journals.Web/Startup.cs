using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Journals.Data;
using Journals.Model;
using Journals.Repository;
using LP.Framework.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Antiforgery;
using Serilog;

using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Journals.Web
{
    public class Startup
    {

        private readonly IHostingEnvironment env;

        public Startup(IHostingEnvironment env)
        {
            this.env = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);


            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IContainer ApplicationContainer { get; set; }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));


            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()                                
                ;

            if (env.IsDevelopment())
            {
                services.Add(new ServiceDescriptor(typeof(IAntiforgery), new NullAntiForgery()));
            }

            services.AddMvc(options => options.RespectBrowserAcceptHeader = true);

            var builder = new ContainerBuilder();

            builder.Register(c => Configuration).As<IConfigurationRoot>().As<IConfiguration>();

            builder.RegisterModule<JournalsWebModule>();


            builder.Populate(services);

            this.ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(this.ApplicationContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            loggerFactory.AddSerilog();
            loggerFactory.AddDebug();
            loggerFactory.AddConsole();

            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            appLifetime.ApplicationStopped.Register(() => this.ApplicationContainer.Dispose());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                //app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715


            app.UseMvc(routes =>
                       {
                           routes.MapRoute(
                               name: "default",
                               template: "{controller=Home}/{action=Index}/{id?}");

                       });

            if (env.IsDevelopment())
            {
                var logger = loggerFactory.CreateLogger("INITDB");

                InitializeDB(logger);
            }
        }

        /// <summary>
        /// Initializes the database.
        /// </summary>
        /// <param name="logger">The logger.</param>
        private void InitializeDB(ILogger logger)
        {
            List<Task> tasks = new List<Task>();

            try
            {
                var seeders =
                    ApplicationContainer.ResolveOptional<IEnumerable<IDbSeeder>>()?.ToList();

                if (seeders != null && seeders.Any())
                {
                    foreach (var seeder in seeders)
                    {
                        logger.LogInformation($"Initializing database. Initializer:", seeder);
                        tasks.Add(seeder.Seed());
                    }
                    Task.WaitAll(tasks.ToArray(), TimeSpan.FromMinutes(1));
                    logger.LogInformation("Finished DB Initialization.");
                }
                else
                {
                    logger.LogWarning("No database seeders found.");
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical($"Error setting up the database.\n\n{ex}\n");
            }
        }

    }

}
