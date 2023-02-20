using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.DocFX.DocFXTasks;
using System.IO;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using Nuke.Common.ChangeLog;
using Nuke.Common.Tools.DocFX;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.SignClient.SignClientTasks;
using Nuke.Common.Tools.SignClient;
using Octokit;
using Nuke.Common.Utilities;
using Nuke.Common.CI.GitHubActions;
using Project = Nuke.Common.ProjectModel.Project;
using System.Collections.Generic;
using System.Threading.Tasks;
partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Tests);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    [GitRepository] readonly GitRepository GitRepository;

    [Parameter] string NugetPublishUrl = "https://api.nuget.org/v3/index.json";
    [Parameter][Secret] string NugetKey;

    [Parameter] int Port = 8090;
    [Parameter] string NugetPrerelease;
    AbsolutePath ToolsDir => RootDirectory / "tools";
    AbsolutePath Output => RootDirectory / "bin";
    AbsolutePath OutputNuget => Output / "nuget";
    AbsolutePath OutputTests => RootDirectory / "TestResults";
    AbsolutePath OutputPerfTests => RootDirectory / "PerfResults";
    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath DocSiteDirectory => RootDirectory / "docs" / "_site";
    public string ChangelogFile => RootDirectory / "RELEASE_NOTES.md";
    public AbsolutePath DocFxDir => RootDirectory / "docs";
    public AbsolutePath DocFxDirJson => DocFxDir / "docfx.json";

    readonly Solution Solution = ProjectModelTasks.ParseSolution(RootDirectory.GlobFiles("*.sln").FirstOrDefault());

    GitHubActions GitHubActions => GitHubActions.Instance;
    private long BuildNumber()
    {
        return GitHubActions.RunNumber;
    }
    private string PreReleaseVersionSuffix()
    {
        return "beta" + (BuildNumber() > 0 ? BuildNumber() : DateTime.UtcNow.Ticks.ToString());
    }
    public ChangeLog Changelog => ReadChangelog(ChangelogFile);

    public ReleaseNotes ReleaseNotes => Changelog.ReleaseNotes.OrderByDescending(s => s.Version).FirstOrDefault() ?? throw new ArgumentException("Bad Changelog File. Version Should Exist");

    private string VersionFromReleaseNotes => ReleaseNotes.Version.IsPrerelease ? ReleaseNotes.Version.OriginalVersion : "";
    private string VersionSuffix => NugetPrerelease == "dev" ? PreReleaseVersionSuffix() : NugetPrerelease == "" ? VersionFromReleaseNotes : NugetPrerelease;
    public string ReleaseVersion => ReleaseNotes.Version?.ToString() ?? throw new ArgumentException("Bad Changelog File. Define at least one version");
    GitHubClient GitHubClient;
    Target Clean => _ => _
         .Description("Cleans all the output directories")
         .Before(Restore)
         .Executes(() =>
         {
             RootDirectory
             .GlobDirectories("src/**/bin", "src/**/obj", Output, OutputTests, OutputPerfTests, OutputNuget, DocSiteDirectory)
             .ForEach(DeleteDirectory);
             EnsureCleanDirectory(Output);
         });

    Target Restore => _ => _
        .Description("Restores all nuget packages")
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });
    Target Compile => _ => _
        .Description("Builds all the projects in the solution")
        .DependsOn(AssemblyInfo, Restore)
        .Executes(() =>
        {
            var version = ReleaseNotes.Version.ToString();
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });
    Target Tests => _ => _
        .Description("Runs all the unit tests")
        .DependsOn(Compile)
        .Executes(() =>
        {
            IEnumerable<Project> GetProjects()
            {
                // if you need to filter tests by environment, do it here.
                if (EnvironmentInfo.IsWin)
                    return Solution.GetProjects("*.Tests");
                else
                    return Solution.GetProjects("*.Tests");
            }
            var projects = GetProjects();
            foreach (var project in projects)
            {
                Information($"Running tests from {project}");
                foreach (var fw in project.GetTargetFrameworks())
                {
                    Information($"Running for {project} ({fw}) ...");
                    DotNetTest(c => c
                           .SetProjectFile(project)
                           .SetConfiguration(Configuration.ToString())
                           .SetFramework(fw)
                           .SetResultsDirectory(OutputTests)
                           .SetProcessWorkingDirectory(Directory.GetParent(project).FullName)
                           .SetLoggers("trx")
                           .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                           .EnableNoBuild());
                }
            }
        });
    Target AssemblyInfo => _ => _
       .After(Restore)
       .Executes(() =>
       {
           XmlTasks.XmlPoke(SourceDirectory / "Directory.Build.props", "//Project/PropertyGroup/PackageReleaseNotes", GetNuGetReleaseNotes(ChangelogFile));
           XmlTasks.XmlPoke(SourceDirectory / "Directory.Build.props", "//Project/PropertyGroup/VersionPrefix", ReleaseVersion);

       });

    Target Install => _ => _
        .Description("Install `Nuke.GlobalTool` and SignClient")
        .Executes(() =>
        {
            DotNet($@"dotnet tool install SignClient --version 1.3.155 --tool-path ""{ToolsDir}"" ");
            DotNet($"tool install Nuke.GlobalTool --global");
        });

    static void Information(string info)
    {
        Serilog.Log.Information(info);
    }
}
