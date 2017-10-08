namespace RaynMaker.Portfolio.Interactors

module PositionsInteractor =
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities
    open System

    type PositionSummary = {
        Open : DateTime 
        Close : DateTime option
        Isin : string
        Name : string
        MarketProfit : decimal<Currency>
        DividendProfit : decimal<Currency>
        MarketRoi : decimal<Percentage>
        DividendRoi : decimal<Percentage>
        MarketRoiAnual : decimal<Percentage>
        DividendRoiAnual : decimal<Percentage>
        }

    type Position = {
        Open : DateTime 
        Close : DateTime option
        Isin : string
        Name : string
        Count : decimal
        Invested : decimal<Currency> 
        Payouts : decimal<Currency>  
        Dividends : decimal<Currency>  
        }

    let createPosition date isin name =
        { 
            Open = date
            Close = None
            Isin = isin
            Name = name
            Count = 0.0M
            Invested = 0.0M<Currency>
            Payouts = 0.0M<Currency>
            Dividends = 0.0M<Currency>
        }

    let getPositions store =
        let getPosition positions isin = 
            positions |> List.tryFind(fun p -> p.Isin = isin)

        let buyStock positions (evt:StockBought) =
            let p = evt.Isin |> getPosition positions |? createPosition evt.Date evt.Isin evt.Name
            let newP =
                { p with Invested = p.Invested + evt.Count * evt.Price + evt.Fee
                         Count = p.Count + evt.Count }
            newP::(positions |> List.filter ((<>) p))

        let sellStock positions (evt:StockSold) =
            let p = 
                match evt.Isin |> getPosition positions with
                | Some p -> p
                | None -> failwithf "Cannot sell stock %s (Isin: %s) because no position exists" evt.Name evt.Isin
            
            let count = p.Count - evt.Count
            // TODO: count must not be zero
            let newP =
                { p with Payouts = p.Payouts + evt.Count * evt.Price - evt.Fee
                         Count = count
                         Close = if count = 0.0M then evt.Date |> Some else None }
            newP::(positions |> List.filter ((<>) p))

        let closePosition positions (evt:PositionClosed) =
            let p = 
                match evt.Isin |> getPosition positions with
                | Some p -> p
                | None -> failwithf "Cannot sell stock %s (Isin: %s) because no position exists" evt.Name evt.Isin

            // TODO: count must not be zero
            let newP =
                { p with Payouts = p.Payouts + p.Count * evt.Price - evt.Fee
                         Count = 0.0M }
            newP::(positions |> List.filter ((<>) p))

        let receiveDividend positions (evt:DividendReceived) =
            // TODO: position not found actually is an error
            let p = evt.Isin |> getPosition positions |? createPosition evt.Date evt.Isin evt.Name
            let newP =
                { p with Dividends = p.Dividends + evt.Value - evt.Fee }
            newP::(positions |> List.filter ((<>) p))

        let processEvent positions evt =
            match evt with
            | StockBought evt -> evt |> buyStock positions
            | StockSold evt  -> evt |> sellStock positions
            | PositionClosed evt  -> evt |> closePosition positions
            | DividendReceived evt -> evt |> receiveDividend positions
            | _ -> positions

        store
        |> List.fold processEvent []

    let summarizePositions positions =
        let summarizePosition p =
            let investedYears = 
                let span = p.Close |> Option.map(fun c -> c - p.Open) |? (DateTime.Today - p.Open)
                span.TotalDays / 365.0 |> decimal
            let marketRoi = (p.Payouts - p.Invested) / p.Invested * 100.0M<Percentage>
            let dividendRoi = p.Dividends / p.Invested * 100.0M<Percentage>
            { 
                Open = p.Open
                Close = p.Close
                Isin = p.Isin
                Name = p.Name
                MarketProfit = p.Payouts - p.Invested
                DividendProfit = p.Dividends
                MarketRoi = marketRoi
                DividendRoi = dividendRoi
                MarketRoiAnual = marketRoi / investedYears 
                DividendRoiAnual = dividendRoi / investedYears
            }

        positions
        |> Seq.map summarizePosition
        |> Seq.sortByDescending(fun p -> p.MarketRoiAnual + p.DividendRoiAnual)
        |> List.ofSeq

module PerformanceInteractor =
    open PositionsInteractor
    open RaynMaker.Portfolio.Entities

    type PerformanceReport = {
        TotalProfit : decimal<Currency>
        }

    let getPerformance (positions:PositionSummary list) =

        let totalProfit = 
            positions
            |> Seq.sumBy(fun p -> p.MarketProfit + p.DividendProfit)

        { TotalProfit = totalProfit }

module BenchmarkInteractor =
    open System
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities

    type SavingsPlan = {
        Fee : decimal<Percentage>
        AnualFee : decimal<Percentage>
        }

    type ManualOrder = {
        Fee : decimal<Percentage>
        MinFee : decimal<Currency>
        MaxFee : decimal<Currency>
        }
    
    let getFee order value =
        match value |> percent order.Fee with
        | x when x < order.MinFee -> order.MinFee
        | x when x > order.MaxFee -> order.MaxFee
        | x -> x

    type Benchmark = {
        Isin : string
        Name : string
        SavingsPlan : SavingsPlan
        Manual : ManualOrder
        }

    /// Based on the original buy events new benchmarking events are generated which simulate 
    /// the performance one could have achived by buying the benchmark asset (e.g. an ETF) instead
    let buyBenchmarkInstead (benchmark:Benchmark) getPrice (store:DomainEvent list) =
        let getFee = getFee benchmark.Manual

        let buy day (value:decimal<Currency>) =
            let price = day |> getPrice
            let fee = value |> getFee
            let count = (value - fee) / price
            { StockBought.Isin = benchmark.Isin
              Name = benchmark.Name
              Date = day
              Fee = fee
              Price = price
              Count = count }

        let closePosition() =
            { PositionClosed.Date = DateTime.Today
              Name = benchmark.Name
              Isin = benchmark.Isin
              Price = DateTime.Today |> getPrice
              // TODO: fix
              Fee = 10.0M<Currency> } 

        let sell day (value:decimal<Currency>) =
            let price = day |> getPrice
            let count = value / price
            { StockSold.Isin = benchmark.Isin
              Name = benchmark.Name
              Date = day
              // ignore fee as we wouldnt sell ETF we would also save the fee
              Fee = 0.0M<Currency>
              Price = price
              Count = count }
        
        seq {
            yield! store
                    |> Seq.choose (function
                        | StockBought e -> buy e.Date (e.Price * e.Count + e.Fee) |> StockBought |> Some
                        // if we would invest in ETF we would not sell but we have to do it here in this simulation 
                        // otherwise we would buy more shares than we have money
                        // - ignore fee as we wouldnt sell ETF we would also save the fee
                        | StockSold e -> sell e.Date (e.Price * e.Count) |> StockSold |> Some
                        | _ -> None)
            yield closePosition() |> PositionClosed
        }
        |> List.ofSeq

    /// Simulate buying a benchmark every month with the money available to calculate
    /// possible performance
    let buyBenchmarkByPlan (benchmark:Benchmark) getPrice cashLimit (store:DomainEvent list) =

        // TODO: anual fee
    
        let start = store.Head |> Events.GetDate
        let stop = DateTime.Today
        let numMonths = (stop.Year - start.Year) * 12 + (stop.Month - start.Month + 1)
        
        //[0 .. numMonths]
        //|> Seq.map(fun m -> (new DateTime(start.Year, start.Month, 1)).AddMonths(m))
        //|> Seq.map workingDay

        store
