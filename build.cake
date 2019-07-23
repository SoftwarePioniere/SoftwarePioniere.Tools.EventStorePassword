// dotnet cake build.cake --target=Pack --verbosity=Verbose
// dotnet cake build.cake --target=Pack --verbosity=Verbose


#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0"

var nugetApiKey         = Argument("nugetapikey", EnvironmentVariable("NUGET_API_KEY") );
var mygetApiKey         = Argument("mygetapikey", EnvironmentVariable("MYGET_API_KEY") );
var tfsBuild            = HasEnvironmentVariable("TF_BUILD");
var artifactsDirectory  = Directory("./artifacts");
var target              = Argument<string>("target", "Default");

GitVersion gitVersion;

Setup(context =>
{

    if (tfsBuild) {
        GitVersion(new GitVersionSettings {
            UpdateAssemblyInfo = false,
            OutputType = GitVersionOutput.BuildServer
        });
    }   

    gitVersion = GitVersion(new GitVersionSettings {
        UpdateAssemblyInfo = false
    });
 
});

///////////////////////////////////////////////////////////////////////////////

Task("Pack")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] { artifactsDirectory });

    var vv = gitVersion.NuGetVersionV2;
    var asv = gitVersion.AssemblySemVer;

    Information("Version: {0}" , vv );
    Information("AssemblyVersion: {0}" , asv );
   
    var settings = new DotNetCorePackSettings
    {
        Configuration = "Release",
        EnvironmentVariables = new Dictionary<string, string> {
            { "Version", vv },
            { "AssemblyVersion", asv },
            { "FileVersion", asv }
        },        
        OutputDirectory = artifactsDirectory
    };

    DotNetCorePack("SoftwarePioniere.Tools.EventStorePassword.sln", settings);
    
});


///////////////////////////////////////////////////////////////////////////////

Task("Publish")   
    .Does( () => {
  
    var settings = new DotNetCoreNuGetPushSettings {
        Source = "https://api.nuget.org/v3/index.json",
        ApiKey = nugetApiKey
    };

    if (gitVersion.BranchName == "dev") {
        Verbose("dev branch, publish to myget");

        settings.Source = "https://www.myget.org/F/softwarepioniere/api/v3/index.json";
        settings.ApiKey = mygetApiKey;
    }
    
    var pkgs = GetFiles($"{artifactsDirectory.Path.FullPath}/**/*.nupkg");

    foreach(var pk in pkgs)
    {
        Information("Starting DotNetCoreNuGetPush on Package: {0}", pk.FullPath);      

        if (!string.IsNullOrEmpty(settings.ApiKey)) {
            DotNetCoreNuGetPush(pk.FullPath, settings);  
        }   else {
            Warning("Skipping push because of empty api key");
        }  
    }

});

Task("PackAndPublish")  
  .IsDependentOn("Pack") 
  .IsDependentOn("Publish") 
  ;


RunTarget(target);