namespace RaynMaker.Portfolio.Service

module Startup =
    open System
    open RaynMaker.Portfolio.UseCases
    open RaynMaker.Portfolio.UseCases.BenchmarkInteractor
    open RaynMaker.Portfolio
    open Suave.Http
    
    [<AutoOpen>]
    module private Impl =
        open Suave
        open Suave.Successful
        open Suave.Operators
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
            
