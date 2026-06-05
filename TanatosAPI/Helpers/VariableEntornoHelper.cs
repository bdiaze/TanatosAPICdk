using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
    [ExcludeFromCodeCoverage]
    public class VariableEntornoHelper(IHostEnvironment env, IConfiguration config): IVariableEntornoHelper {
        public string Obtener(string nombre) {
            if (!env.IsProduction()) {
                return config[$"VariableEntorno:{nombre}"] ?? throw new InvalidOperationException($"Debes agregar el atributo VariableEntorno > {nombre} en el archivo appsettings.Development.json para ejecutar localmente.");
            }
            return Environment.GetEnvironmentVariable(nombre) ?? throw new InvalidOperationException($"No se ha configurado la variable de entorno {nombre}.");
        }
    }
}
