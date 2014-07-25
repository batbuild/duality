// include Fake lib
#r @"packages\FAKE\tools\FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile

RestorePackages()

// Directories
let buildDir  = @".\build\"
let testDir   = @".\test\"
let deployDir = @".\deploy\"
let packagesDir = @".\packages"

// Game directory this is where the assemblies will be copied to
let gameDir = @"D:\BatCat\honourbound\trunk\Game"

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir; deployDir]
)

Target "CompileApp" (fun _ ->
     !! "Duality.sln"      
      |> MSBuildDebug buildDir "Build"
      |> Log "AppBuild-Output: "
)

Target "CompileTest" (fun _ ->
    !! @"*Test.csproj"
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
)

Target "NUnitTest" (fun _ ->
    !! (testDir + @"\NUnit.Test.*.dll")
      |> NUnit (fun p ->
                 {p with
                   DisableShadowCopy = true;
                   OutputFile = testDir + @"TestResults.xml"})
)

let pluginsDir = buildDir+ "\Plugins"
open System.IO
Target "CopyToGame" (fun _ ->    
    CopyDir gameDir @".\DualityEditor\bin\Debug" allFiles
)

Target "Zip" (fun _ ->
    !+ (buildDir + "\**\*.*")
        -- "*.zip"
        |> Scan
        |> Zip buildDir (deployDir + "Duality." + ".zip")
)



// Dependencies
"Clean"
  ==> "CompileApp"
(*  ==> "CompileTest"
  ==> "FxCop"
  ==> "NUnitTest"*)
  ==> "CopyToGame"
//  ==> "Zip"


// start build
RunTargetOrDefault "CopyToGame"