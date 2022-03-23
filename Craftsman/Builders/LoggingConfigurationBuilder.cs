namespace Craftsman.Builders
{
    using System.IO.Abstractions;
    using Helpers;

    public class LoggingConfigurationBuilder
    {
        public static void CreateConfigFile(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiHostExtensionsClassPath(projectDirectory, "LoggingConfiguration.cs", authServerProjectName);
            var fileText = GetConfigText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        private static string GetConfigText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Json;

public static class LoggingConfiguration
{{
    public static void AddLoggingConfiguration(this IHost host)
    {{
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var env = services.GetService<IWebHostEnvironment>();

        var loggingLevelSwitch = new LoggingLevelSwitch();
        if (env.IsDevelopment())
            loggingLevelSwitch.MinimumLevel = LogEventLevel.Warning;
        if (env.IsProduction())
            loggingLevelSwitch.MinimumLevel = LogEventLevel.Information;
        
        var logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(loggingLevelSwitch)
            .MinimumLevel.Override(""Microsoft.Hosting.Lifetime"", LogEventLevel.Information)
            .MinimumLevel.Override(""Microsoft.AspNetCore.Authentication"", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironment(env.EnvironmentName)
            .Enrich.WithProperty(""ApplicationName"", env.ApplicationName)
            .Enrich.WithExceptionDetails()
            .WriteTo.Console();

        Log.Logger = logger.CreateLogger();
    }}
}}";
        }
    }
}