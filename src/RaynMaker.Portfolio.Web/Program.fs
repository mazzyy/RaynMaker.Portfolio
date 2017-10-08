module RaynMaker.Portfolio.Main

open System.Threading
open System
open System.Reflection
open System.IO
open FSharp.Data
open Suave
open Suave.Successful
open Suave.Operators
open Suave.Filters
open Suave.Redirection
open RaynMaker.Portfolio.Frameworks
open RaynMaker.Portfolio.Gateways
open RaynMaker.Portfolio.Interactors
open RaynMaker.Portfolio.Interactors.BenchmarkInteractor
open RaynMaker.Portfolio.Entities

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
                    path "/" >=> redirect "/Content/index.html"
                    pathScan "/Content/%s" (fun f -> Files.file (sprintf "%s/Content/%s" home f))
                    pathScan "/Scripts/%s" (fun f -> Files.file (sprintf "%s/Scripts/%s" home f))
                    path "/api/positions" >=> Handlers.positions getEvents
                    path "/api/performance" >=> Handlers.performance getEvents
                    path "/api/benchmark" >=> Handlers.benchmark getEvents benchmark getBenchmarkHistory 
                ]
        ]

    let port,cts = Httpd.start app

    Browser.start port

    Console.ReadKey true |> ignore
    
    cts.Cancel()

    0