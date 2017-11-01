module RaynMaker.Portfolio.Main

open System.Threading
open System
open System.Reflection
open System.IO
open FSharp.Data
open Suave
open Suave.Operators
open Suave.Filters
open Suave.Redirection
open RaynMaker.Portfolio.Frameworks
open RaynMaker.Portfolio.Gateways
open RaynMaker.Portfolio.Interactors.BenchmarkInteractor
open RaynMaker.Portfolio.Entities
open System.Diagnostics
open Suave.RequestErrors
open RaynMaker.Portfolio.Storage

type Project = JsonProvider<"../../etc/Portfolio.json">

let private getHome () =
    let location = Assembly.GetExecutingAssembly().Location
    location |> Path.GetDirectoryName |> Path.GetFullPath

type internal Services = {
    suaveCts : CancellationTokenSource
    eventStore : EventStore.Api
}

type Instance = {
    port : int
    services : obj 
}

let start projectFile =
    let location = Assembly.GetExecutingAssembly().Location
    let home = getHome()
    printfn "Home: %s" home

    // development support
    Process.GetProcesses()
    |> Seq.filter(fun x -> x.ProcessName = Path.GetFileNameWithoutExtension(location))
    |> Seq.filter(fun x -> x.Id <> Process.GetCurrentProcess().Id)
    |> Seq.iter(fun x -> x.Kill())

    printfn "Loading project ..."

    printfn "Project: %s" projectFile

    let project = Project.Load(projectFile)

    let storeHome =
        if project.Store |> Path.IsPathRooted then
            project.Store
        else
            let projectDir = projectFile |> Path.GetDirectoryName
            Path.Combine(projectDir,project.Store)
        |> Path.GetFullPath

    printfn "Store: %s" storeHome

    let fromStore path = Path.Combine(storeHome,path)

    let store = EventStore.create (fun () -> 
        printfn "Loading events ..."

        let events, errors = "Events.xlsx" |> fromStore |> EventsReader.readExcel
        errors |> Seq.iter (printf "  %s")
        events)

    let loadBenchmarkHistory() =
        printfn "Loading benchmark history ..."

        project.Benchmark.Isin 
        |> sprintf "%s.history.csv" 
        |> fromStore 
        |> HistoricalPricesReader.readCsv
    
    printfn "Starting ..."

    let getBenchmarkHistory = remember loadBenchmarkHistory

    let benchmark = { 
        Isin = project.Benchmark.Isin |> Isin
        Name = project.Benchmark.Name
        SavingsPlan = { SavingsPlan.Fee = project.Benchmark.SavingsPlan.Fee * 1.0M<Percentage>
                        AnualFee = project.Benchmark.SavingsPlan.AnualFee * 1.0M<Percentage>
                        Rate = (decimal project.Benchmark.SavingsPlan.Rate) * 1.0M<Currency> }
        Manual = { ManualOrder.Fee = project.Benchmark.Manual.Fee * 1.0M<Percentage>
                   MinFee = project.Benchmark.Manual.MinFee * 1.0M<Currency>
                   MaxFee = project.Benchmark.Manual.MaxFee * 1.0M<Currency> }
        }

    let app = 
        let log = request (fun r -> printfn "%s" r.path; succeed)

        choose [ 
            GET >=> log >=> choose
                [
                    path "/" >=> redirect "/Client/index.html"
                    pathScan "/Client/%s" (fun f -> Files.file (sprintf "%s/Client/%s" home f))
                    pathScan "/static/%s" (fun f -> Files.file (sprintf "%s/Client/static/%s" home f))
                    path "/api/positions" >=> Controllers.positions store
                    path "/api/performance" >=> Controllers.performance store
                    path "/api/benchmark" >=> Controllers.benchmark store benchmark getBenchmarkHistory 
                    path "/api/diversification" >=> Controllers.diversification store 
                    NOT_FOUND "Resource not found."
                ]
        ]

    let port,cts = Httpd.start app

    { port = port
      services = { suaveCts = cts 
                   eventStore = store } }

let stop instance =
    let services = instance.services :?> Services

    services.suaveCts.Cancel()
    services.eventStore.Stop()

let getProjectFileFromCommandLine argv =
    let home = getHome()

    match argv with
    | [|path|] -> path 
    | x -> Path.Combine(home, "..", "..", "etc", "Portfolio.json")
    |> Path.GetFullPath

[<EntryPoint>]
let main argv =
    let instance = 
        argv
        |> getProjectFileFromCommandLine
        |> start 

    Browser.start instance.port

    Console.ReadKey true |> ignore
    
    stop instance

    0
