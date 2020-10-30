using EdmObjectsGenerator;
using Hackathon2020.Poc01.Data;
using Hackathon2020.Poc01.Lib;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using System.Reflection;

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
            var contextType = generator.GenerateDbContext(DbContextConstants.CsdlFile, DbContextConstants.Name);


            var connectionString = File.ReadAllText(DbContextConstants.ConnectionStringFile).Trim();

            DbContext dbContext = (DbContext)Activator.CreateInstance(contextType, new string[] { connectionString });
         
            dbContext.Database.CreateIfNotExists();

            services.AddSingleton(this.Configuration);
            services.AddSingleton(typeof(DbContext), dbContext);

            services.AddMvc(options => {
                    options.EnableEndpointRouting = false;
                    options.Conventions.Insert(0, new DynamicControllerModelConvention(Model.GetModel()));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .ConfigureApplicationPartManager(d =>  d.FeatureProviders.Add(new DynamicControllerFeatureProvider(Model.GetModel())));
            services.AddControllers();
            services.AddOData();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            string routeName = "odata";
            DefaultODataPathHandler pathHandler = new DefaultODataPathHandler();

            app.UseRouting();

            app.UseMvc(routeBuilder =>
            {
                IList<IODataRoutingConvention> routingConventions = ODataRoutingConventions.CreateDefault();
                routingConventions.Insert(0, new DynamicControllerRoutingConvention());
                // Add attribute routing (RE:#1622)
                routingConventions.Insert(1, new AttributeRoutingConvention(routeName, routeBuilder.ServiceProvider, pathHandler));

                routeBuilder.EnableDependencyInjection();
                routeBuilder.Filter().Expand().Select().OrderBy().Count().MaxTop(null).SkipToken();
                routeBuilder.MapODataServiceRoute("odata", "odata", Model.GetModel(), pathHandler, routingConventions);
            });
        }
    }

    public static class Model
    {
        private static IEdmModel instance = null;
        public static IEdmModel GetModel()
        {
            if (instance == null)
            {
                var xmlReader = XmlReader.Create(DbContextConstants.CsdlFile);
                instance = CsdlReader.Parse(xmlReader);
            }

            return instance;
        }
    }
}
