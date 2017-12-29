namespace RaynMaker.Portfolio.Gateways

module Controllers =
    open System
    open RaynMaker.Portfolio.UseCases
    open RaynMaker.Portfolio.Entities
    open RaynMaker.Portfolio.Storage
    open RaynMaker.Portfolio.UseCases.BenchmarkInteractor
    
    [<AutoOpen>]
    module private Impl =
        let (=>) k v = k,v |> box
        let formatDate (date:DateTime) = date.ToString("yyyy-MM-dd")
        let formatTimespan (span:TimeSpan) = 
            match span.TotalDays with
            | x when x > 365.0 -> sprintf "%.2f years" (span.TotalDays / 365.0)
            | x when x > 90.0 -> sprintf "%.2f months" (span.TotalDays / 30.0)
            | x -> sprintf "%.0f days" span.TotalDays
        
    let positions (depot:Depot.Api) broker lastPriceOf = 
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
    
    let performance (store:EventStore.Api) (depot:Depot.Api) broker lastPriceOf = 
        depot.Get()
        |> PositionsInteractor.evaluatePositions broker lastPriceOf
        |> PerformanceInteractor.getPerformance (store.Get())
        |> fun p -> 
            dict [
                "totalInvestment" => p.TotalInvestment
                "totalProfit" => p.TotalProfit
            ]
            
    let benchmark (store:EventStore.Api) broker savingsPlan (historicalPrices:HistoricalPrices.Api) (benchmark:Benchmark) = 
        let getPrice day = 
            let history = benchmark.Isin |> historicalPrices.Get
            match Prices.getPrice 300.0 history day with
            | Some p -> p
            | None -> failwithf "Could not get a price for: %A" day

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

    let diversification (depot:Depot.Api) = 
        let report = depot.Get() |> StatisticsInteractor.getDiversification

        dict [
            "labels" => (report.Positions |> List.map fst)
            "data" => (report.Positions |> List.map snd)
        ]
            
