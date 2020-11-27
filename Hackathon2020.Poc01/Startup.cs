using EdmObjectsGenerator;
using Hackathon2020.Poc01.Data;
using Hackathon2020.Poc01.Lib;
using Microsoft.AspNet.OData.Builder;
using Microsoft.EntityFrameworkCore;
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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Azure.Storage.Files.Shares;
using Hackathon2020.Poc01.Lib.Seeder;
using DataLib;

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
            if (ProjectEnv.IsRemoveEnv())
            {
                var azureStorageConnString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
                var azureFileShareName = Environment.GetEnvironmentVariable("AZURE_FILE_SHARE_NAME");
                var shareClient = new ShareClient(azureStorageConnString, azureFileShareName);
                var dirClient = shareClient.GetDirectoryClient("schema");
                var fileClient = dirClient.GetFileClient("Project.csdl");
                var downloadResponse = fileClient.Download();
                var stream = downloadResponse.Value.Content;
                var reader = new StreamReader(stream);
                File.WriteAllText(DbContextConstants.CsdlFile, reader.ReadToEnd());
            }

            DbContextGenerator generator = new DbContextGenerator();
            var contextType = generator.GenerateDbContext(DbContextConstants.CsdlFile, DbContextConstants.Name);


            var connectionString = File.ReadAllText(DbContextConstants.ConnectionStringFile).Trim();

            var optionsBuilder = new DbContextOptionsBuilder();
            var dbContextOptions = optionsBuilder.UseInMemoryDatabase("RapidApiDB").Options;

            //DbContext dbContext = (DbContext)Activator.CreateInstance(contextType, new object[] { dbContextOptions });
            ListDataStore dbContext = (ListDataStore)Activator.CreateInstance(contextType);

            var model = Model.GetModel();
            Assembly targetAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name.Contains(DbContextConstants.Name));
            var targetTypes = targetAssembly.DefinedTypes;
            //dbContext.Database.CreateIfNotExists();

            if (ProjectEnv.ShouldSeedData())
            {
                var dataSeeder = new DataSeeder(model, dbContext, targetTypes);
                dataSeeder.SeedData().Wait();
            }

            services.AddSingleton(this.Configuration);
            services.AddSingleton(typeof(IDataStore), dbContext);

            services.AddMvc(options => {
                    options.EnableEndpointRouting = false;
                    options.Conventions.Insert(0, new DynamicControllerModelConvention(model));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .ConfigureApplicationPartManager(d =>  d.FeatureProviders.Add(new DynamicControllerFeatureProvider(model)));
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
