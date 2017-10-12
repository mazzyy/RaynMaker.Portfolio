namespace RaynMaker.Portfolio.Gateways

module Handlers =
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
        
        let getPositionSummaries = PositionsInteractor.getPositions >> PositionsInteractor.summarizePositions
    
    let positions getEvents = warbler (fun _ -> 
        getEvents() 
        |> getPositionSummaries 
        |> List.map(fun p -> 
            dict [
                "name" => p.Name
                "isin" => p.Isin
                "open" => (p.Open |> formatDate)
                "close" => (p.Close |> Option.map formatDate |? "-")
                "duration" => ((p.Close |? DateTime.Today) - p.Open |> formatTimespan)
                "marketProfit" => p.MarketProfit
                "dividendProfit" => p.DividendProfit
                "totalProfit" => (p.MarketProfit + p.DividendProfit)
                "marketRoi" => p.MarketRoi
                "dividendRoi" => p.DividendRoi
                "totalRoi" => (p.MarketRoi + p.DividendRoi)
                "marketRoiAnual" => p.MarketRoiAnual
                "dividendRoiAnual" => p.DividendRoiAnual
                "totalRoiAnual" => (p.MarketRoiAnual + p.DividendRoiAnual)
                "isClosed" => (p.Close |> Option.isSome)
            ])
        |> JSON)
    
    let performance getEvents = warbler (fun _ -> 
        let events = getEvents() 
        
        events
        |> getPositionSummaries 
        |> PerformanceInteractor.getPerformance events
        |> fun p -> 
            dict [
                "totalInvestment" => p.TotalInvestment
                "totalProfit" => p.TotalProfit
            ]
        |> JSON)
            
    let benchmark getEvents (benchmark:Benchmark) getBenchmarkHistory = warbler (fun _ -> 
        let history = getBenchmarkHistory()

        let getPrice day =
            match history |> Seq.skipWhile(fun (p:Price) -> p.Day < day) |> Seq.tryHead with
            | Some p -> p.Value
            | None -> let last = history |> List.last
                      last.Value

        let b1 =
            getEvents() 
            |> BenchmarkInteractor.buyBenchmarkInstead benchmark getPrice
            |> getPositionSummaries
            |> Seq.head

        let b2 =
            getEvents() 
            |> BenchmarkInteractor.buyBenchmarkByPlan benchmark getPrice
            |> getPositionSummaries
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
            
module ExcelEventStore =
    open System
    open FSharp.ExcelProvider
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities

    type private Sheet = ExcelFile<"../../etc/Events.xlsx">

    type ParsedEvent = 
        | Event of DomainEvent
        | Unknown of string * DateTime * payload:obj list

    let load path =
        let sheet = new Sheet(path)

        sheet.Data
        |> Seq.filter(fun r -> String.IsNullOrEmpty(r.Event) |> not)
        |> Seq.map(fun r -> 
            try
                match r.Event with
                | EqualsI "StockBought" _ -> 
                    { StockBought.Date = r.Date
                      Isin = r.ID
                      Name = r.Name
                      Count = r.Count |> decimal
                      Price = (r.Value |> decimal) * 1.0M<Currency>
                      Fee = (r.Fee |> decimal) * 1.0M<Currency>} |> StockBought |> Event
                | EqualsI "StockSold" _ -> 
                    { StockSold.Date = r.Date
                      Isin = r.ID
                      Name = r.Name
                      Count = r.Count |> decimal
                      Price = (r.Value |> decimal) * 1.0M<Currency>
                      Fee = (r.Fee |> decimal) * 1.0M<Currency>} |> StockSold |> Event
                | EqualsI "DividendReceived" _ -> 
                    { DividendReceived.Date = r.Date
                      Isin = r.ID
                      Name = r.Name
                      Value = (r.Value |> decimal) * 1.0M<Currency>
                      Fee = (r.Fee |> decimal) * 1.0M<Currency>} |> DividendReceived |> Event
                | EqualsI "DepositAccounted" _ -> 
                    { DepositAccounted.Date = r.Date
                      Value = (r.Value |> decimal) * 1.0M<Currency>} |> DepositAccounted |> Event
                | EqualsI "DisbursementAccounted" _ -> 
                    { DisbursementAccounted.Date = r.Date
                      Value = (r.Value |> decimal) * 1.0M<Currency>} |> DisbursementAccounted |> Event
                | EqualsI "InterestReceived" _ -> 
                    { InterestReceived.Date = r.Date
                      Value = (r.Value |> decimal) * 1.0M<Currency>} |> InterestReceived |> Event
                | EqualsI "PositionClosed" _ -> 
                    { PositionClosed.Date = DateTime.Today
                      Isin = r.ID
                      Name = r.Name
                      Price = (r.Value |> decimal) * 1.0M<Currency>
                      Fee = (r.Fee |> decimal) * 1.0M<Currency>} |> PositionClosed |> Event
                | x -> Unknown(r.Event,r.Date,[r.ID; r.Name; r.Value; r.Fee; r.Count; r.Comment])
            with 
                | ex -> failwithf "Failed reading event store at %s with %A" r.Event ex 
        )
        |> List.ofSeq

module HistoricalPrices =
    open System
    open FSharp.Data
    open RaynMaker.Portfolio.Entities

    type private Sheet = CsvProvider<"../../etc/FR0011079466.history.csv",";">
    
    let load (path:string) =
        let sheet = Sheet.Load(path)
        sheet.Rows
        |> Seq.map(fun row -> 
            { Day = DateTime.Parse(row.Date)
              Value = row.Price * 1.0M<Currency> } )
        |> Seq.sortBy(fun x -> x.Day)
        |> List.ofSeq

