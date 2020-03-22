module RaynMaker.Portfolio.Controllers

open System
open RaynMaker.Portfolio.Entities
open RaynMaker.Portfolio.UseCases
open RaynMaker.Portfolio.UseCases.BenchmarkInteractor

module private Format =
    let date (date:DateTime) = date.ToString("yyyy-MM-dd")
    let timespan (span:TimeSpan) = 
        match span.TotalDays with
        | x when x > 365.0 -> sprintf "%.2f years" (span.TotalDays / 365.0)
        | x when x > 90.0 -> sprintf "%.2f months" (span.TotalDays / 30.0)
        | x -> sprintf "%.0f days" span.TotalDays
    let count = sprintf "%.2f"
    let price = sprintf "%.2f"

let (=>) k v = k,v |> box

let listOpenPositions (depot:Depot.Api) broker lastPriceOf = 
    depot.Get() 
    |> PositionsInteractor.evaluateOpenPositions broker lastPriceOf
    |> List.map(fun p -> 
        dict [
            "name" => p.Position.Name
            "isin" => (p.Position.Isin |> Str.ofIsin)
            "shares" => (p.Position.Count |> Format.count)
            "duration" => (p.PricedAt - p.Position.OpenedAt |> Format.timespan)
            "buyingPrice" => (p.BuyingPrice |> Option.map Format.price |> Option.defaultValue "n.a.")
            "buyingValue" => (p.BuyingValue |> Option.map Format.price |> Option.defaultValue "n.a.")
            "pricedAt" => (p.PricedAt |> Format.date)
            "currentPrice" => (p.CurrentPrice |> Format.price)
            "currentValue" => (p.CurrentValue |> Format.price)
            "marketProfit" => (p.MarketProfit)
            "dividendProfit" => (p.DividendProfit)
            "totalProfit" => (p.MarketProfit + p.DividendProfit)
            "marketRoi" => (p.MarketRoi)
            "dividendRoi" => (p.DividendRoi)
            "totalRoi" => (p.MarketRoi + p.DividendRoi)
            "marketProfitAnnual" => (p.MarketProfitAnnual)
            "dividendProfitAnnual" => (p.DividendProfitAnnual)
            "totalProfitAnnual" => (p.MarketProfitAnnual + p.DividendProfitAnnual)
            "marketRoiAnnual" => (p.MarketRoiAnnual)
            "dividendRoiAnnual" => (p.DividendRoiAnnual)
            "totalRoiAnnual" => (p.MarketRoiAnnual + p.DividendRoiAnnual)
        ])

let listClosedPositions (depot:Depot.Api) = 
    depot.Get() 
    |> PositionsInteractor.evaluateClosedPositions
    |> List.map(fun p -> 
        dict [
            "name" => p.Position.Name
            "isin" => (p.Position.Isin |> Str.ofIsin)
            "duration" => (p.Duration |> Format.timespan)
            "totalProfit" => p.TotalProfit
            "totalRoi" => p.TotalRoi
            "marketProfitAnnual" => (p.MarketProfitAnnual)
            "dividendProfitAnnual" => (p.DividendProfitAnnual)
            "totalProfitAnnual" => (p.MarketProfitAnnual + p.DividendProfitAnnual)
            "marketRoiAnnual" => (p.MarketRoiAnnual)
            "dividendRoiAnnual" => (p.DividendRoiAnnual)
            "totalRoiAnnual" => (p.MarketRoiAnnual + p.DividendRoiAnnual)
        ])

let getPerformanceIndicators (store:EventStore.Api) (depot:Depot.Api) broker getCashLimit lastPriceOf = 
    depot.Get()
    |> PerformanceInteractor.getPerformance (store.Get()) broker getCashLimit lastPriceOf
    |> fun p -> 
        dict [
            "totalInvestment" => p.TotalInvestment
            "totalProfit" => p.TotalProfit
            "totalDividends" => p.TotalDividends
            "cashLimit" => p.CashLimit
            "investingTime" => p.InvestingTime.TotalDays / 365.0
        ]

let getBenchmarkPerformance (store:EventStore.Api) broker savingsPlan (historicalPrices:HistoricalPrices.Api) (benchmark:Benchmark) = 
    benchmark
    |> BenchmarkInteractor.getBenchmarkPerformance store broker savingsPlan historicalPrices
    |> fun r -> 
        dict [
            "name" => benchmark.Name
            "isin" => (benchmark.Isin |> Str.ofIsin)
            "buyInstead" => dict [
                "totalProfit" => (r.BuyInstead.Profit)
                "totalRoi" => (r.BuyInstead.Roi)
                "totalRoiAnnual" => (r.BuyInstead.RoiAnnual) 
            ]
            "buyPlan" => dict [
                "totalProfit" => (r.BuyPlan.Profit)
                "totalRoi" => (r.BuyPlan.Roi)
                "totalRoiAnnual" => (r.BuyPlan.RoiAnnual) 
                "rate" => savingsPlan.Rate
            ]
        ]

let getDiversification (depot:Depot.Api) lastPriceOf = 
    depot.Get() 
    |> StatisticsInteractor.getDiversification lastPriceOf
    |> fun r ->
        dict [
            "labels" => (r.Positions |> List.map fst)
            "data" => (r.Positions |> List.map snd)
        ]

let listCashflow (store:EventStore.Api) limit = 
    store.Get() 
    |> CashflowInteractor.getTransactions limit
    |> List.map(fun t ->
        dict [
            "date" => (t.Date |> Format.date)
            "type" => t.Type
            "comment" => t.Comment
            "value" => t.Value 
            "balance" => t.Balance 
        ])
        
