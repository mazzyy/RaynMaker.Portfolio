namespace RaynMaker.Portfolio.Gateways

module Controllers =
    open System
    open Suave
    open Suave.Successful
    open Suave.Operators
    open Newtonsoft.Json
    open Newtonsoft.Json.Serialization
    open RaynMaker.Portfolio.Interactors
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Interactors.BenchmarkInteractor
    open RaynMaker.Portfolio.Entities
    open RaynMaker.Portfolio.Interactors.PositionsInteractor
    open RaynMaker.Portfolio.Storage
    
    [<AutoOpen>]
    module private Impl =
        open RaynMaker.Portfolio.Interactors.PositionsInteractor
        open RaynMaker.Portfolio.Interactors.PerformanceInteractor

        let JSON v =
            let jsonSerializerSettings = new JsonSerializerSettings()
            jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

            JsonConvert.SerializeObject(v, jsonSerializerSettings)
            |> OK
            >=> Writers.setMimeType "application/json; charset=utf-8"

        let (=>) k v = k,v |> box
        let formatDate (date:DateTime) = date.ToString("yyyy-MM-dd")
        let formatTimespan (span:TimeSpan) = 
            match span.TotalDays with
            | x when x > 365.0 -> sprintf "%.2f years" (span.TotalDays / 365.0)
            | x when x > 90.0 -> sprintf "%.2f months" (span.TotalDays / 30.0)
            | x -> sprintf "%.0f days" span.TotalDays
        
    let positions (depot:Depot.Api) broker lastPriceOf = warbler (fun _ -> 
        depot.Get() 
        |> PositionsInteractor.evaluatePositions broker lastPriceOf
        |> List.map(fun p -> 
            dict [
                "name" => p.Position.Name
                "isin" => (p.Position.Isin |> Str.ofIsin)
                "open" => (p.Position.OpenedAt |> formatDate)
                "pricedAt" => (p.PricedAt |> formatDate)
                "duration" => (p.PricedAt - p.Position.OpenedAt |> formatTimespan)
                "marketProfit" => p.MarketProfit
                "dividendProfit" => p.DividendProfit
                "totalProfit" => (p.MarketProfit + p.DividendProfit)
                "marketRoi" => p.MarketRoi
                "dividendRoi" => p.DividendRoi
                "totalRoi" => (p.MarketRoi + p.DividendRoi)
                "marketRoiAnual" => p.MarketRoiAnual
                "dividendRoiAnual" => p.DividendRoiAnual
                "totalRoiAnual" => (p.MarketRoiAnual + p.DividendRoiAnual)
                "isClosed" => (p.Position.ClosedAt |> Option.isSome) 
            ])
        |> JSON)
    
    let performance (store:EventStore.Api) (depot:Depot.Api) broker lastPriceOf = warbler (fun _ -> 
        depot.Get()
        |> PositionsInteractor.evaluatePositions broker lastPriceOf
        |> PerformanceInteractor.getPerformance (store.Get())
        |> fun p -> 
            dict [
                "totalInvestment" => p.TotalInvestment
                "totalProfit" => p.TotalProfit
            ]
        |> JSON)
            
    let benchmark (store:EventStore.Api) broker savingsPlan (historicalPrices:HistoricalPrices.Api) (benchmark:Benchmark) = warbler (fun _ -> 
        let getPrice = 
            let history = benchmark.Isin |> historicalPrices.Get
            Prices.getPrice history

        let eval = BenchmarkInteractor.evaluate benchmark getPrice broker (store.Get())

        let b1 = eval (BenchmarkInteractor.buyBenchmarkInstead broker)
        let b2 = eval (BenchmarkInteractor.buyBenchmarkByPlan savingsPlan)

        dict [
            "name" => benchmark.Name
            "isin" => (benchmark.Isin |> Str.ofIsin)
            "buyInstead" => dict [
                "totalProfit" => (b1.MarketProfit + b1.DividendProfit)
                "totalRoi" => (b1.MarketRoi + b1.DividendRoi)
                "totalRoiAnual" => (b1.MarketRoiAnual + b1.DividendRoiAnual) 
            ]
            "buyPlan" => dict [
                "totalProfit" => (b2.MarketProfit + b2.DividendProfit)
                "totalRoi" => (b2.MarketRoi + b2.DividendRoi)
                "totalRoiAnual" => (b2.MarketRoiAnual + b2.DividendRoiAnual) 
                "rate" => savingsPlan.Rate
            ]
        ]
        |> JSON)

    let diversification (depot:Depot.Api) = warbler (fun _ -> 
        let report = depot.Get() |> StatisticsInteractor.getDiversification

        dict [
            "labels" => (report.Positions |> List.map fst)
            "data" => (report.Positions |> List.map snd)
        ]
        |> JSON)
            
