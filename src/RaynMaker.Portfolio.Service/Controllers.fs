namespace RaynMaker.Portfolio.Gateways

module Controllers =
    open System
    open RaynMaker.Portfolio.Entities
    open RaynMaker.Portfolio.UseCases
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
        
    let listPositions (depot:Depot.Api) broker lastPriceOf = 
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
    
    let getPerformanceIndicators (store:EventStore.Api) (depot:Depot.Api) broker lastPriceOf = 
        depot.Get()
        |> PositionsInteractor.evaluatePositions broker lastPriceOf
        |> PerformanceInteractor.getPerformance (store.Get())
        |> fun p -> 
            dict [
                "totalInvestment" => p.TotalInvestment
                "totalProfit" => p.TotalProfit
            ]
            
    let getBenchmarkPerformance (store:EventStore.Api) broker savingsPlan (historicalPrices:HistoricalPrices.Api) (benchmark:Benchmark) = 
        benchmark
        |> BenchmarkInteractor.getBenchmarkPerformance store broker savingsPlan historicalPrices
        |> fun r -> 
            dict [
                "name" => benchmark.Name
                "isin" => (benchmark.Isin |> Str.ofIsin)
                "buyInstead" => dict [
                    "totalProfit" => (r.BuyInstead.MarketProfit + r.BuyInstead.DividendProfit)
                    "totalRoi" => (r.BuyInstead.MarketRoi + r.BuyInstead.DividendRoi)
                    "totalRoiAnual" => (r.BuyInstead.MarketRoiAnual + r.BuyInstead.DividendRoiAnual) 
                ]
                "buyPlan" => dict [
                    "totalProfit" => (r.BuyPlan.MarketProfit + r.BuyPlan.DividendProfit)
                    "totalRoi" => (r.BuyPlan.MarketRoi + r.BuyPlan.DividendRoi)
                    "totalRoiAnual" => (r.BuyPlan.MarketRoiAnual + r.BuyPlan.DividendRoiAnual) 
                    "rate" => savingsPlan.Rate
                ]
            ]

    let getDiversification (depot:Depot.Api) = 
        depot.Get() 
        |> StatisticsInteractor.getDiversification
        |> fun r ->
            dict [
                "labels" => (r.Positions |> List.map fst)
                "data" => (r.Positions |> List.map snd)
            ]
            
