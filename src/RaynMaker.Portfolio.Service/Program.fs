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

type Project = JsonProvider<"../../etc/Portfolio.json">

let private getHome () =
    let location = Assembly.GetExecutingAssembly().Location
    location |> Path.GetDirectoryName |> Path.GetFullPath

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

    let loadEvents() =
        printfn "Loading events ..."

        let store = "Events.xlsx" |> fromStore |> ExcelEventStore.load

        store
        |> Seq.choose (function |ExcelEventStore.Unknown(a,b,c) -> Some(a,b,c) | _ -> None)
        |> Seq.iter(fun (e,d,p) -> printfn "Unknown event skipped: %s|%A|%A" e d p)

        store
        |> List.choose (function |ExcelEventStore.Event e -> Some e | _ -> None)

    let loadBenchmarkHistory() =
        printfn "Loading benchmark history ..."

        project.Benchmark.Isin 
        |> sprintf "%s.history.csv" 
        |> fromStore 
        |> HistoricalPrices.load
    
    printfn "Starting ..."

    let getEvents = remember loadEvents
    let getBenchmarkHistory = remember loadBenchmarkHistory

    let benchmark = { 
        Isin = project.Benchmark.Isin
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
                    path "/api/positions" >=> Handlers.positions getEvents
                    path "/api/performance" >=> Handlers.performance getEvents
                    path "/api/benchmark" >=> Handlers.benchmark getEvents benchmark getBenchmarkHistory 
                    path "/api/diversification" >=> Handlers.diversification getEvents 
                    NOT_FOUND "Resource not found."
                ]
        ]

    Httpd.start app

let stop (cts:CancellationTokenSource) =
    cts.Cancel()

let getProjectFileFromCommandLine argv =
    let home = getHome()

    match argv with
    | [|path|] -> path 
    | x -> Path.Combine(home, "..", "..", "etc", "Portfolio.json")
    |> Path.GetFullPath

[<EntryPoint>]
let main argv =
    let port,cts = 
        argv
        |> getProjectFileFromCommandLine
        |> start 

    Browser.start port

    Console.ReadKey true |> ignore
    
    stop cts

    0
