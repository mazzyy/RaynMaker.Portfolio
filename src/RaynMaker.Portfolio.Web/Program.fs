module RaynMaker.Portfolio.Main

open System.Threading
open System
open System.Reflection
open System.IO
open FSharp.Data
open RaynMaker.Portfolio.Frameworks
open RaynMaker.Portfolio.Gateways

type Project = JsonProvider<"../../etc/Portfolio.json">

[<EntryPoint>]
let main argv = 
    let home = Assembly.GetExecutingAssembly().Location |> Path.GetDirectoryName |> Path.GetFullPath
    printfn "Home: %s" home

    printfn "Loading project ..."

    let projectFile = 
        match argv with
        | [|path|] -> path 
        | x -> Path.Combine(home, "..", "..", "etc", "Portfolio.json")
        |> Path.GetFullPath

    let resolveFromProject path =
        if Path.IsPathRooted(path) then
            path
        else
            let projectDir = projectFile |> Path.GetDirectoryName
            Path.Combine(projectDir,path)
        |> Path.GetFullPath

    let project = Project.Load(projectFile)

    printfn "Loading events ..."

    let store =  project.Events |> resolveFromProject |> ExcelEventStore.load

    store
    |> Seq.choose (function |ExcelEventStore.Unknown(a,b,c) -> Some(a,b,c) | _ -> None)
    |> Seq.iter(fun (e,d,p) -> printfn "Unknown event skipped: %s|%A|%A" e d p)
    
    printfn "Starting ..."

    let app = 
        store
        |> List.choose (function |ExcelEventStore.Event e -> Some e | _ -> None)
        |> WebApp.createApp home
    let port,cts = Httpd.start app

    Browser.start port

    Console.ReadKey true |> ignore
    
    cts.Cancel()

    0