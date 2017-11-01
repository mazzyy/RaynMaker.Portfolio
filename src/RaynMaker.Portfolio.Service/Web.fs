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
        
    let positions (store:EventStore.Api) = warbler (fun _ -> 
        store.Get() 
        |> PositionsInteractor.getPositions
        |> PositionsInteractor.evaluatePositions 
        |> List.map(fun p -> 
            dict [
                "name" => p.Name
                "isin" => p.Isin
                "open" => (p.Open |> formatDate)
                "pricedAt" => (p.PricedAt|> Option.map formatDate |? "-")
                "duration" => ((p.PricedAt |? DateTime.Today) - p.Open |> formatTimespan)
                "marketProfit" => p.MarketProfit
                "dividendProfit" => p.DividendProfit
                "totalProfit" => (p.MarketProfit + p.DividendProfit)
                "marketRoi" => p.MarketRoi
                "dividendRoi" => p.DividendRoi
                "totalRoi" => (p.MarketRoi + p.DividendRoi)
                "marketRoiAnual" => p.MarketRoiAnual
                "dividendRoiAnual" => p.DividendRoiAnual
                "totalRoiAnual" => (p.MarketRoiAnual + p.DividendRoiAnual)
                "isClosed" => p.IsClosed 
            ])
        |> JSON)
    
    let performance (store:EventStore.Api) = warbler (fun _ -> 
        store.Get()
        |> PositionsInteractor.getPositions
        |> PositionsInteractor.evaluatePositions 
        |> PerformanceInteractor.getPerformance (store.Get())
        |> fun p -> 
            dict [
                "totalInvestment" => p.TotalInvestment
                "totalProfit" => p.TotalProfit
            ]
        |> JSON)
            
    let benchmark (store:EventStore.Api) (benchmark:Benchmark) getBenchmarkHistory = warbler (fun _ -> 
        let history = getBenchmarkHistory()

        let getPrice day =
            match history |> Seq.skipWhile(fun (p:Price) -> p.Day < day) |> Seq.tryHead with
            | Some p -> p.Value
            | None -> let last = history |> List.last
                      last.Value

        let b1 =
            store.Get()
            |> BenchmarkInteractor.buyBenchmarkInstead benchmark getPrice
            |> PositionsInteractor.getPositions
            |> PositionsInteractor.evaluatePositions 
            |> Seq.head

        let b2 =
            store.Get()
            |> BenchmarkInteractor.buyBenchmarkByPlan benchmark getPrice
            |> PositionsInteractor.getPositions
            |> PositionsInteractor.evaluatePositions 
            |> Seq.head

        dict [
            "name" => benchmark.Name
            "isin" => benchmark.Isin
            "buyInstead" => dict [
                "totalProfit" => (b1.MarketProfit + b1.DividendProfit)
                "totalRoi" => (b1.MarketRoi + b1.DividendRoi)
                "totalRoiAnual" => (b1.MarketRoiAnual + b1.DividendRoiAnual) 
            ]
            "buyPlan" => dict [
                "totalProfit" => (b2.MarketProfit + b2.DividendProfit)
                "totalRoi" => (b2.MarketRoi + b2.DividendRoi)
                "totalRoiAnual" => (b2.MarketRoiAnual + b2.DividendRoiAnual) 
                "rate" => benchmark.SavingsPlan.Rate
            ]
        ]
        |> JSON)

    let diversification (store:EventStore.Api) = warbler (fun _ -> 
        let report =
            store.Get()
            |> PositionsInteractor.getPositions 
            |> StatisticsInteractor.getDiversification

        dict [
            "labels" => (report.Positions |> List.map fst)
            "data" => (report.Positions |> List.map snd)
        ]
        |> JSON)
            
