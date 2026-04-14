using System;
using System.IO;
using System.Linq;
using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Clean;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Core;
using Cake.Frosting;
using Cake.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;

namespace CakeBuild;

public record ModProject(string ProjectName, string ProjectDir, string ModDirName);

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
    }
}

public class BuildContext : FrostingContext
{
    public static readonly ModProject[] Mods =
    [
        new("FireArrows", "FireArrows", "base"),
        new("FireArrows.CombatOverhaul", "FireArrows.CombatOverhaul", "cocompat"),
    ];

    public string BuildConfiguration { get; }
    public bool SkipJsonValidation { get; }

    public BuildContext(ICakeContext context)
        : base(context)
    {
        BuildConfiguration = context.Argument("configuration", "Release");
        SkipJsonValidation = context.Argument("skipJsonValidation", false);
    }
}

[TaskName("ValidateJson")]
public sealed class ValidateJsonTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        if (context.SkipJsonValidation)
        {
            return;
        }
        var jsonFiles = context.GetFiles($"../assets/**/*.json");
        foreach (var file in jsonFiles)
        {
            try
            {
                var json = File.ReadAllText(file.FullPath);
                JToken.Parse(json);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Validation failed for JSON file: {file.FullPath}{Environment.NewLine}{ex.Message}", ex);
            }
        }
    }
}

[TaskName("Build")]
[IsDependentOn(typeof(ValidateJsonTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        string[] excludedProjects = context.Arguments.GetArgument("exclude").Split(',');
        foreach (var mod in BuildContext.Mods)
        {
            if (excludedProjects.Contains(mod.ProjectName)) continue;
            var csproj = $"../{mod.ProjectDir}/{mod.ProjectName}.csproj";

            context.DotNetClean(csproj,
                new DotNetCleanSettings
                {
                    Configuration = context.BuildConfiguration
                });

            context.DotNetPublish(csproj,
                new DotNetPublishSettings
                {
                    Configuration = context.BuildConfiguration
                });
        }
    }
}

[TaskName("Package")]
[IsDependentOn(typeof(BuildTask))]
public sealed class PackageTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.EnsureDirectoryExists("../Releases");
        context.CleanDirectory("../Releases");

        string[] excludedProjects = context.Arguments.GetArgument("exclude").Split(',');
        foreach (var mod in BuildContext.Mods)
        {
            if (excludedProjects.Contains(mod.ProjectName)) continue;
            var modInfo = context.DeserializeJsonFromFile<ModInfo>($"../{mod.ProjectDir}/modinfo.json");
            var modName = modInfo.ModID;
            var version = modInfo.Version;
            var publishDir = $"../bin/{context.BuildConfiguration}/Mods/{mod.ModDirName}/publish";

            context.EnsureDirectoryExists($"../Releases/{modName}");
            context.CopyFiles($"{publishDir}/*", $"../Releases/{modName}");

            if (context.DirectoryExists($"../assets/{modName}"))
            {
                context.CopyDirectory($"../assets/{modName}", $"../Releases/{modName}/assets/{modName}");
            }

            context.CopyFile($"../{mod.ProjectDir}/modinfo.json", $"../Releases/{modName}/modinfo.json");

            if (context.FileExists($"../{mod.ProjectDir}/modicon.png"))
            {
                context.CopyFile($"../{mod.ProjectDir}/modicon.png", $"../Releases/{modName}/modicon.png");
            }

            context.Zip($"../Releases/{modName}", $"../Releases/{modName}_{version}.zip");
        }
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(PackageTask))]
public class DefaultTask : FrostingTask
{
}
