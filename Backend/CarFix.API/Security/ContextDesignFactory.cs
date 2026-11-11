using Microsoft.EntityFrameworkCore.Design;
using Modelo;

namespace CarFix.API.Security;

public sealed class ContextDesignFactory : IDesignTimeDbContextFactory<Context>
{
    public Context CreateDbContext(string[] args)
    {
        // EF puede ejecutarse desde la raíz; resolver la configuración de la API,
        // no la del ejecutable dotnet-ef, y reutilizar su UserSecretsId.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(typeof(ContextDesignFactory).Assembly.Location)!)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddUserSecrets<ContextDesignFactory>(optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();
        Context.Configure(configuration.GetConnectionString("CarFix")
            ?? throw new InvalidOperationException("Falta configurar ConnectionStrings:CarFix para las migraciones."));
        return new Context();
    }
}
