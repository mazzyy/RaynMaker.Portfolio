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
    open RaynMaker.Portfolio.Entities
    open System

    type Benchmark = {
        Isin : string
        Name : string
        TransactionFee : decimal<Currency>
        AnualFee : decimal<Currency>
        }

    // TODO: consider disposal instead of buy -  second kind of benchmark

    /// Based on the original events (dates and values) new benchmarking events are generated
    /// which simulate which performance one could have achived by buying the benchmark asset (e.g. an ETF) instead
    let sellBenchmarkInstead (benchmark:Benchmark) getPrice (store:DomainEvent list) =
        // TODO: anual fee
        let buy day (value:decimal<Currency>) =
            let price = day |> getPrice
            let count = (value - benchmark.TransactionFee) / price
            { StockBought.Isin = benchmark.Isin
              Name = benchmark.Name
              Date = day
              Fee = benchmark.TransactionFee
              Price = price
              Count = count }

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
        
        let price = DateTime.Today |> getPrice

        seq {
            yield! store
                    |> Seq.choose (function
                        | StockBought e -> buy e.Date (e.Price * e.Count + e.Fee) |> StockBought |> Some
                        // if we would invest in ETF we would not sell but we have to do it here in this simulation 
                        // otherwise we would buy more shares than we have money
                        // - ignore fee as we wouldnt sell ETF we would also save the fee
                        | StockSold e -> sell e.Date (e.Price * e.Count) |> StockSold |> Some
                        | _ -> None)
            yield { PositionClosed.Name = benchmark.Name
                    Isin = benchmark.Isin
                    Price = price
                    Fee = benchmark.TransactionFee } |> PositionClosed
        }
        |> List.ofSeq
