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
let gameDir = @"D:\honourbound\trunk\Game"

// version info
let version = "0.2"  // or retrieve from CI server

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
(*    if Directory.Exists @"\DualityEditorPlugins\CamView\bin\Debug\" then CopyDir pluginsDir @".\DualityEditorPlugins\CamView\bin\Debug\CamView.editor.dll"  allFiles

    CopyDir pluginsDir  @".\DualityEditorPlugins\EditorBase\bin\Debug\EditorBase.editor.dll"  allFiles
    CopyDir pluginsDir @".\DualityEditorPlugins\HelpAdvisor\bin\Debug\HelpAdvisor.editor.dll"  allFiles
    CopyDir pluginsDir @".\DualityEditorPlugins\LogView\bin\Debug\LogView.editor.dll" allFiles
    CopyDir pluginsDir @".\DualityEditorPlugins\ObjectInspector\bin\Debug\ObjectInspector.editor.dll"  allFiles
    CopyDir pluginsDir @".\DualityEditorPlugins\ProjectView\bin\Debug\ProjectView.editor.dll"  allFiles
    CopyDir pluginsDir @".\DualityEditorPlugins\SceneView\bin\Debug\SceneView.editor.dll"  allFiles
*)
    CopyDir gameDir @".\DualityEditor\bin\Debug" allFiles
)

Target "Zip" (fun _ ->
    !+ (buildDir + "\**\*.*")
        -- "*.zip"
        |> Scan
        |> Zip buildDir (deployDir + "Calculator." + version + ".zip")
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