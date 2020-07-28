using EdmObjectsGenerator;
//using Hackathon2020.Poc01.Data;
using Hackathon2020.Poc01.Lib;
//using Hackathon2020.Poc01.Models;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Xml;

namespace Hackathon2020.Poc01
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            DbContextGenerator generator = new DbContextGenerator();
            var contextType = generator.GenerateDbContext(@"SampleModel.xml");

            services.AddSingleton(this.Configuration);
            //services.AddDbContext<DbContext, InMemoryDbContext>(options => options.UseInMemoryDatabase(databaseName: "Hackathon2020.Poc01Db"));
            services.AddScoped<DbContext>(sp =>
            {
                DbContext dbContext = Activator.CreateInstance(contextType) as DbContext;

                return dbContext;
            });

            services.AddMvc(options => {
                    options.EnableEndpointRouting = false;
                    options.Conventions.Insert(0, new DynamicControllerModelConvention(Model.GetModel()));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .ConfigureApplicationPartManager(d => d.FeatureProviders.Add(new DynamicControllerFeatureProvider(Model.GetModel())));
            services.AddControllers();
            services.AddOData();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            //var modelBuilder = new ODataConventionModelBuilder();
            //modelBuilder.EntitySet<Product>("Products");
            //modelBuilder.EntitySet<Supplier>("Suppliers");
            //modelBuilder.EntitySet<Customer>("Customers");


            app.UseMvc(routeBuilder =>
            {
                IList<IODataRoutingConvention> routingConventions = ODataRoutingConventions.CreateDefault();
                routingConventions.Insert(0, new DynamicControllerRoutingConvention());

                routeBuilder.EnableDependencyInjection();
                routeBuilder.Filter().Expand().Select().OrderBy().MaxTop(null).SkipToken();
                routeBuilder.MapODataServiceRoute("odata", "odata", Model.GetModel(), new DefaultODataPathHandler(), routingConventions);
            });

            using (IServiceScope serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                DbContext db = serviceScope.ServiceProvider.GetRequiredService<DbContext>();

                //if (!db.Set<Product>().Any())
                //{
                //    db.Set<Product>().AddRange(
                //        new[]
                //        {
                //            new Product { Id = 1, Name = "Microsoft Xbox One X - 1TB Console - Black", UnitPrice = 45000M },
                //            new Product { Id = 2, Name = "Microsoft Microsoft Surface Pro 3 36W Power Supply", UnitPrice = 5700M },
                //        });

                //    db.Set<Supplier>().AddRange(
                //        new[]
                //        {
                //            new Supplier { Id = 1, Name = "ABC Limited", Website = "www.abc.ltd" },
                //            new Supplier { Id = 2, Name = "XYZ Limited", Website = "www.xyz.ltd" }
                //        });

                //    db.Set<Customer>().AddRange(
                //        new[] {
                //            new Customer { Id = 1, Name = "General Merchants " }
                //        });

                //    db.SaveChanges();
                //}
            }
        }
    }

    public static class Model
    {
        private static IEdmModel instance = null;
        public static IEdmModel GetModel()
        {
            if (instance == null)
            {
                var xmlReader = XmlReader.Create("SampleModel.xml");
                instance = CsdlReader.Parse(xmlReader);
            }

            return instance;
        }
    }
}
