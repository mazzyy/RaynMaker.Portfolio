module RaynMaker.Portfolio.Service.Main

open System
open System.Diagnostics
open System.IO
open System.Reflection
open System.Threading
open Plainion

type internal Services = {
    suaveCts : CancellationTokenSource
    shutdown : unit -> unit }

type Instance = {
    port : int
    services : obj }

let start errorHandler projectFile =
    let location = Assembly.GetExecutingAssembly().Location

    // development support
    Process.GetProcesses()
    |> Seq.filter(fun x -> x.ProcessName = Path.GetFileNameWithoutExtension(location))
    |> Seq.filter(fun x -> x.Id <> Process.GetCurrentProcess().Id)
    |> Seq.iter(fun x -> x.Kill())

    let app, shutdown = Startup.build errorHandler projectFile

    printfn "Starting ..."
    
    let port,cts = Httpd.start errorHandler app

    { port = port
      services = { suaveCts = cts 
                   shutdown = shutdown } }

let startCSharp (errorHandler:System.Action<string,Exception>) projectFile =
    start (fun msg ex -> errorHandler.Invoke(msg,ex)) projectFile

let stop instance =
    let services = instance.services :?> Services

    services.suaveCts.Cancel()
    services.shutdown()

let getProjectFileFromCommandLine argv =
    let home = getHome()

    argv |> printf "Cmd line: %A"

    match argv with
    | [| path |] -> path 
    | _ -> Path.Combine(home, "..", "..", "docs", "Samples", "Portfolio.json")
    |> Path.GetFullPath

let handleLastChanceException msg (ex:Exception) = 
    Console.Error.WriteLine( sprintf "FATAL ERROR: %s%s%s" msg Environment.NewLine (ExceptionExtensions.Dump(ex)) )
    Environment.Exit(1)

[<EntryPoint>]
let main argv =
    let instance = 
        argv
        |> getProjectFileFromCommandLine
        |> start handleLastChanceException

    Process.Start(sprintf "http://localhost:%i/" instance.port) |> ignore

    Console.ReadKey true |> ignore
    
    stop instance

    0
