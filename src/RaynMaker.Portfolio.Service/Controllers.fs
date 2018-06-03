namespace RaynMaker.Portfolio.Gateways

module Controllers =
    open System
    open RaynMaker.Portfolio.Entities
    open RaynMaker.Portfolio.UseCases
    open RaynMaker.Portfolio.UseCases.BenchmarkInteractor
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

        let (=>) k v = k,v |> box
        let formatDate (date:DateTime) = date.ToString("yyyy-MM-dd")
        let formatTimespan (span:TimeSpan) = 
            match span.TotalDays with
            | x when x > 365.0 -> sprintf "%.2f years" (span.TotalDays / 365.0)
            | x when x > 90.0 -> sprintf "%.2f months" (span.TotalDays / 30.0)
            | x -> sprintf "%.0f days" span.TotalDays
        let formatCount = sprintf "%.2f"
        let formatPrice = sprintf "%.2f"
        
    let listOpenPositions (depot:Depot.Api) broker lastPriceOf = 
        depot.Get() 
        |> PositionsInteractor.evaluateOpenPositions broker lastPriceOf
        |> List.map(fun p -> 
            dict [
                "name" => p.Position.Name
                "isin" => (p.Position.Isin |> Str.ofIsin)
                "shares" => (p.Position.Count |> formatCount)
                "duration" => (p.PricedAt - p.Position.OpenedAt |> formatTimespan)
                "buyingPrice" => (p.BuyingPrice |> Option.map formatPrice |> Option.defaultValue "n.a.")
                "buyingValue" => (p.BuyingValue |> Option.map formatPrice |> Option.defaultValue "n.a.")
                "pricedAt" => (p.PricedAt |> formatDate)
                "currentPrice" => (p.CurrentPrice |> formatPrice)
                "currentValue" => (p.CurrentValue |> formatPrice)
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
        |> JSON
        
    let listClosedPositions (depot:Depot.Api) broker lastPriceOf = 
        depot.Get() 
        |> PositionsInteractor.evaluateClosedPositions
        |> List.map(fun p -> 
            dict [
                "name" => p.Position.Name
                "isin" => (p.Position.Isin |> Str.ofIsin)
                "duration" => (p.Duration |> formatTimespan)
                "totalProfit" => p.TotalProfit
                "totalRoi" => p.TotalRoi
                "marketProfitAnnual" => (p.MarketProfitAnnual)
                "dividendProfitAnnual" => (p.DividendProfitAnnual)
                "totalProfitAnnual" => (p.MarketProfitAnnual + p.DividendProfitAnnual)
                "marketRoiAnnual" => (p.MarketRoiAnnual)
                "dividendRoiAnnual" => (p.DividendRoiAnnual)
                "totalRoiAnnual" => (p.MarketRoiAnnual + p.DividendRoiAnnual)
            ])
        |> JSON
    
    let getPerformanceIndicators (store:EventStore.Api) (depot:Depot.Api) broker lastPriceOf = 
        depot.Get()
        |> PerformanceInteractor.getPerformance (store.Get()) broker lastPriceOf
        |> fun p -> 
            dict [
                "totalInvestment" => p.TotalInvestment
                "totalProfit" => p.TotalProfit
            ]
        |> JSON

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
        |> JSON

    let getDiversification (depot:Depot.Api) lastPriceOf = 
        depot.Get() 
        |> StatisticsInteractor.getDiversification lastPriceOf
        |> fun r ->
            dict [
                "labels" => (r.Positions |> List.map fst)
                "data" => (r.Positions |> List.map snd)
            ]
        |> JSON
            
    let listCashflow (request:HttpRequest) (store:EventStore.Api) = 
        let limit = 
            match request.queryParam "limit" with
            | Choice1Of2 x -> x |> System.Int32.Parse
            | Choice2Of2 _ -> 25
        store.Get() 
        |> CashflowInteractor.getTransactions limit
        |> List.map(fun t ->
            dict [
                "date" => (t.Date |> formatDate)
                "type" => t.Type
                "comment" => t.Comment
                "value" => t.Value 
                "balance" => t.Balance 
            ])
        |> JSON
            
