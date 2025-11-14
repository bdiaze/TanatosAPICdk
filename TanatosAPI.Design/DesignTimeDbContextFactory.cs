using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TanatosAPI.Entities.Contexts;

namespace TanatosAPI.Design {
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TanatosDbContext> {

        // Proyecto .Design para la creación de migrations y compiled models
        // Migrations: 
        //     dotnet ef migrations add [MigrationName] --project TanatosAPI --startup-project TanatosAPI.Design --context TanatosDbContext --output-dir Migrations
        //     dotnet ef migrations remove --project TanatosAPI --startup-project TanatosAPI.Design --context TanatosDbContext
        //     dotnet ef migrations script --idempotent --project TanatosAPI --startup-project TanatosAPI.Design --context TanatosDbContext --output MigrationScripts.sql
        public TanatosDbContext CreateDbContext(string[] args) {
            string basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "TanatosAPI");

            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            string appName = config["VariableEntorno:APP_NAME"]!;

            string connectionString =
                $"Server={config["ConnectionStrings:Host"]!};" +
                $"Port={config["ConnectionStrings:Port"]!};" +
                $"Database={config["ConnectionStrings:Database"]!};" +
                $"User Id={config["ConnectionStrings:User Id"]!};" +
                $"Password='{config["ConnectionStrings:Password"]!}';";

            DbContextOptionsBuilder<TanatosDbContext> optionsBuilder = new();
            optionsBuilder.UseNpgsql(connectionString);

            return new TanatosDbContext(optionsBuilder.Options);
        }
    }
}
