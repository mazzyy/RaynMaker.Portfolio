module RaynMaker.Portfolio.Service.Startup

open System.IO
open System.Reflection
open FSharp.Data
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Redirection
open Suave.RequestErrors
open RaynMaker.Portfolio
open RaynMaker.Portfolio.Entities
open RaynMaker.Portfolio.UseCases
open RaynMaker.Portfolio.UseCases.BenchmarkInteractor

[<AutoOpen>]
module private Impl =
    open Suave.Successful
    open Newtonsoft.Json
    open Newtonsoft.Json.Serialization

    let JSON v =
        let jsonSerializerSettings = new JsonSerializerSettings()
        jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

        JsonConvert.SerializeObject(v, jsonSerializerSettings)
        |> OK
        >=> Writers.setMimeType "application/json; charset=utf-8"
        
    let listOpenPositions (depot:Depot.Api) broker lastPriceOf = 
        Controllers.listOpenPositions depot broker lastPriceOf
        |> JSON
        
    let listClosedPositions (depot:Depot.Api) broker lastPriceOf = 
        Controllers.listClosedPositions depot
        |> JSON
    
    let getPerformanceIndicators (store:EventStore.Api) (depot:Depot.Api) broker getCashLimit lastPriceOf = 
        Controllers.getPerformanceIndicators store depot broker getCashLimit lastPriceOf
        |> JSON

    let getBenchmarkPerformance (store:EventStore.Api) broker savingsPlan (historicalPrices:HistoricalPrices.Api) (benchmark:Benchmark) = 
        Controllers.getBenchmarkPerformance store broker savingsPlan historicalPrices benchmark
        |> JSON

    let getDiversification (depot:Depot.Api) lastPriceOf = 
        Controllers.getDiversification depot lastPriceOf
        |> JSON
            
    let listCashflow (request:HttpRequest) (store:EventStore.Api) = 
        match request.queryParam "limit" with
        | Choice1Of2 x -> x |> System.Int32.Parse
        | Choice2Of2 _ -> 25
        |> Controllers.listCashflow store
        |> JSON
            
type Project = JsonProvider<"../../docs/Samples/Portfolio.json">

let build errorHandler projectFile =
    let home = getHome()
    printfn "Home: %s" home
    
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

    let store = EventStore.create errorHandler (fun () -> 
        printfn "Loading events ..."

        let events, errors = "Events.xlsx" |> fromStore |> EventsReader.readExcel
        errors |> Seq.iter (printf "  %s")
        events)

    let historicalPrices = HistoricalPrices.create errorHandler (fun isin ->
        printfn "Loading historical prices for %s" (Str.ofIsin isin)

        isin
        |> Str.ofIsin
        |> sprintf "%s.history.csv" 
        |> fromStore 
        |> HistoricalPricesReader.readCsv)
    
    printfn "Starting ..."

    let benchmark = { 
        Isin = project.Benchmark.Isin |> Isin
        Name = project.Benchmark.Name }

    let savingsPlan = { SavingsPlan.Fee = project.Benchmark.SavingsPlan.Fee * 1.0M<Percentage>
                        AnnualFee = project.Benchmark.SavingsPlan.AnnualFee * 1.0M<Percentage>
                        Rate = (decimal project.Benchmark.SavingsPlan.Rate) * 1.0M<Currency> }

    let broker = { Broker.Name = project.Broker.Name
                   Fee = project.Broker.Fee * 1.0M<Percentage>
                   MinFee = project.Broker.MinFee * 1.0M<Currency>
                   MaxFee = project.Broker.MaxFee * 1.0M<Currency> }

    let depot = Depot.create errorHandler (store.Get)

    let getCashLimit() = (project.CashLimit |> decimal) * 1.0M<Currency>

    let lastPriceOf isin = 
        let events = store.Get()
        Events.LastPriceOf events isin
    
    let log = request (fun r -> printfn "%s" r.path; succeed)

    let app = 
        choose [ 
            GET >=> log >=> choose
                [
                    path "/" >=> redirect "/Client/index.html"
                    pathScan "/Client/%s" (fun f -> Files.file (sprintf "%s/Client/%s" home f))
                    pathScan "/static/%s" (fun f -> Files.file (sprintf "%s/Client/static/%s" home f))
                    path "/api/positions" >=> warbler (fun _ -> listOpenPositions depot broker lastPriceOf)
                    path "/api/performance" >=> warbler (fun _ -> getPerformanceIndicators store depot broker getCashLimit lastPriceOf)
                    path "/api/benchmark" >=> warbler (fun _ -> getBenchmarkPerformance store broker savingsPlan historicalPrices benchmark)
                    path "/api/diversification" >=> warbler (fun _ -> getDiversification depot lastPriceOf)
                    path "/api/cashflow" >=> warbler (fun ctx -> listCashflow ctx.request store)
                    path "/api/closedPositions" >=> warbler (fun _ -> listClosedPositions depot broker lastPriceOf)
                    NOT_FOUND "Resource not found."
                ]
        ]

    let shutdown() =
        store.Stop()
        historicalPrices.Stop()

    app, shutdown
