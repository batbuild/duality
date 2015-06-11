// include Fake lib
#r @"packages\FAKE\tools\FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile

// Directories
let buildDir  = @".\build\"
let packagesDir = @".\deploy\"

let authors = ["Andrea Magnorsky"; "Andrew O'Connor"; "Dean Ellis";]

// project name and description
let projectName = "Duality.Android"
let projectDescription = "Duality with Android runtime"
let projectSummary = projectDescription // TODO: write a summary

let version = "0.1."+ buildVersion  


// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; packagesDir] 
)

Target "SetVersions" (fun _ ->
    CreateCSharpAssemblyInfo "./Properties/AssemblyInfo.cs"
        [Attribute.Title projectName
         Attribute.Description projectDescription
         Attribute.Product projectSummary
         Attribute.Version version
         Attribute.FileVersion version]
)

Target "RestorePackages" (fun _ ->
    !! "./**/packages.config"
        |> Seq.iter (RestorePackage (fun p ->
            { p with Sources = ["https://www.myget.org/F/6416d9912a7c4d46bc983870fb440d25/"]}))
)

Target "BuildUnsafe" (fun _ ->          
    let buildMode = getBuildParamOrDefault "buildMode" "Release"
    let setParams defaults =
        { defaults with
            Verbosity = Some(Normal)            
            Properties =
                [            
                    "Configuration", buildMode                    
                    "AllowUnsafeBlocks", "True"
                ]
        }
    build setParams "./Duality.android.sln"    
    |> DoNothing  
)

Target "AndroidPack" (fun _ ->         
    NuGet (fun p -> 
        {p with 
            Authors = authors
            Project = projectName
            Description = projectDescription      
            Version = if isLocalBuild then "0.1-loc" else "0.1."+ buildVersion
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Publish = hasBuildParam "nugetkey"
            PublishUrl = getBuildParamOrDefault "nugetUrl" ""            
            WorkingDir = @".\"
            OutputPath = packagesDir
            Dependencies = [                            
                            "Farseer.Duality.Android","0.1.7"
                            ]
        }) "NuGetPackageSpecs/duality.android.nuspec"
)

// Dependencies
"Clean"    
  ==> "SetVersions"
  ==> "RestorePackages"
  ==> "BuildUnsafe"    
  ==> "AndroidPack"
  


RunTargetOrDefault "AndroidPack"