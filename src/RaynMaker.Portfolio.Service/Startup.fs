module RaynMaker.Portfolio.Service.Startup

open System.IO
open FSharp.Data
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Redirection
open Suave.RequestErrors
open Suave.Writers
open Suave.Successful
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
                    
type Project = JsonProvider<"../../docs/Samples/Portfolio.json">

let build errorHandler projectFile =
    let home = getHome()
    printfn "Home: %s" home
    
    printfn "Loading project '%s' ..." projectFile

    let project = Project.Load(projectFile)

    let storeHome =
        if project.Store |> Path.IsPathRooted then
            project.Store
        else
            let projectDir = projectFile |> Path.GetDirectoryName
            Path.Combine(projectDir,project.Store)
        |> Path.GetFullPath

    printfn "Store: %s" storeHome

    let fromStore path = Path.Combine(storeHome, path)

    let eventStore = EventStore.create errorHandler (fun () -> 
        printfn "Loading events ..."

        let events, errors = "Events.xlsx" |> fromStore |> EventsReader.readExcel
        errors |> Seq.iter (printf "  %s")
        events)

    let getPricesFile = sprintf "%s.prices.csv" >> fromStore 
    let collectPrices() = PricesCollector.execute getPricesFile 
    let pricesRepository = PricesRepository.create errorHandler collectPrices (fun isin ->
        printfn "Loading prices for %s" (Str.ofIsin isin)

        isin
        |> Str.ofIsin
        |> getPricesFile
        |> CsvPricesReader.readCsv)
    
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

    let depot = Depot.create errorHandler (eventStore.Get)

    let getCashLimit() = (project.CashLimit |> decimal) * 1.0M<Currency>

    let lastPriceOf = Events.LastPriceOf (eventStore.Get())
    
    let log = request (fun r -> printfn "%s" r.path; succeed)

    let listOpenPositions _ = Controllers.listOpenPositions depot broker lastPriceOf 
    let listClosedPositions _ = Controllers.listClosedPositions depot 
    let getPerformanceIndicators _ = Controllers.getPerformanceIndicators eventStore depot broker getCashLimit lastPriceOf 
    let getBenchmarkPerformance _ = Controllers.getBenchmarkPerformance eventStore broker savingsPlan pricesRepository benchmark 
    let getDiversification _ = Controllers.getDiversification depot lastPriceOf 
    let listCashflow ctx = 
        match ctx.request.queryParam "limit" with
        | Choice1Of2 x -> x |> System.Int32.Parse
        | Choice2Of2 _ -> 25
        |> Controllers.listCashflow eventStore
    
    let fileApi root path = Files.file (sprintf "%s/%s/%s" home root path)
    let jsonApi f = warbler (f >> JSON)
    
    let setCORSHeaders =
        addHeader  "Access-Control-Allow-Origin" "*" 
            >=> addHeader "Access-Control-Allow-Headers" "content-type" 
            >=> addHeader "Access-Control-Allow-Methods" "GET,POST,PUT" 

    let app = 
        choose [ 
            GET >=> log >=> setCORSHeaders >=> choose
                [
                    path "/" >=> redirect "/Client/index.html"
                    pathScan "/Client/%s" (fileApi "Client")
                    pathScan "/js/%s" (fileApi "Client/js")
                    pathScan "/css/%s" (fileApi "Client/css")
                    path "/api/positions" >=> jsonApi listOpenPositions
                    path "/api/performance" >=> jsonApi getPerformanceIndicators
                    path "/api/benchmark" >=> jsonApi getBenchmarkPerformance
                    path "/api/diversification" >=> jsonApi getDiversification
                    path "/api/cashflow" >=> jsonApi listCashflow
                    path "/api/closedPositions" >=> jsonApi listClosedPositions
                    NOT_FOUND "Resource not found."
                ]
        ]

    let shutdown() =
        eventStore.Stop()
        pricesRepository.Stop()

    app, shutdown
