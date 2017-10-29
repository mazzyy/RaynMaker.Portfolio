namespace RaynMaker.Portfolio.Interactors

module PositionsInteractor =
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities
    open System

    type PositionSummary = {
        Open : DateTime 
        PricedAt : DateTime option
        IsClosed : bool
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
        PricedAt : DateTime option
        IsClosed : bool
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
            PricedAt = None
            IsClosed = false
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
            let investment = evt.Count * evt.Price + evt.Fee
            let newInvestment,payouts = 
                if investment > p.Payouts then 
                    investment - p.Payouts,0.0M<Currency>
                else
                    0.0M<Currency>,p.Payouts - investment

            let newP =
                { p with Invested = p.Invested + newInvestment
                         Payouts = payouts
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
                         PricedAt = evt.Date |> Some
                         IsClosed = count = 0.0M }
            newP::(positions |> List.filter ((<>) p))

        let closePosition positions (evt:PositionClosed) =
            let p = 
                match evt.Isin |> getPosition positions with
                | Some p -> p
                | None -> failwithf "Cannot sell stock %s (Isin: %s) because no position exists" evt.Name evt.Isin

            // TODO: count must not be zero
            let newP =
                { p with Payouts = p.Payouts + p.Count * evt.Price - evt.Fee
                         Count = 0.0M 
                         PricedAt = evt.Date |> Some }
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
                let span = p.PricedAt |> Option.map(fun c -> c - p.Open) |? (DateTime.Today - p.Open)
                span.TotalDays / 365.0 |> decimal
            let marketRoi = (p.Payouts - p.Invested) / p.Invested * 100.0M<Percentage>
            let dividendRoi = p.Dividends / p.Invested * 100.0M<Percentage>
            { 
                Open = p.Open
                PricedAt = p.PricedAt
                IsClosed = p.IsClosed
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
        TotalInvestment : decimal<Currency>
        TotalProfit : decimal<Currency>
        }

    let getPerformance store (positions:PositionSummary list) =
        let processEvent total evt =
            match evt with
            | DepositAccounted evt -> printfn "%A - %A = %A" evt.Date evt.Value total; total + evt.Value
            | DisbursementAccounted evt -> printfn "%A - %A = %A" evt.Date evt.Value total; total - evt.Value
            | _ -> total

        let investment =
            store
            |> List.fold processEvent 0.0M<Currency>

        let totalProfit = 
            positions
            |> Seq.sumBy(fun p -> p.MarketProfit + p.DividendProfit)

        { TotalInvestment = investment
          TotalProfit = totalProfit }

module BenchmarkInteractor =
    open System
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities

    type SavingsPlan = {
        Fee : decimal<Percentage>
        AnualFee : decimal<Percentage>
        Rate : decimal<Currency>
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

    let private closePosition benchmark getPrice =
        { PositionClosed.Date = DateTime.Today
          Name = benchmark.Name
          Isin = benchmark.Isin
          Price = DateTime.Today |> getPrice
          Fee = 10.0M<Currency> } 

    /// Based on the original buy & sell events new benchmarking events are generated which simulate 
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

        let sell day (value:decimal<Currency>) =
            let price = day |> getPrice
            let fee = value |> getFee
            let count = value / price
            { StockSold.Isin = benchmark.Isin
              Name = benchmark.Name
              Date = day
              Fee = fee
              Price = price
              Count = count }
        
        seq {
            yield! store
                    |> Seq.choose (function
                        | StockBought e -> buy e.Date (e.Price * e.Count + e.Fee) |> StockBought |> Some
                        | StockSold e -> sell e.Date (e.Price * e.Count) |> StockSold |> Some
                        | _ -> None)
            yield closePosition benchmark getPrice |> PositionClosed
        }
        |> List.ofSeq

    /// Simulate buying a benchmark whenever a deposit was made considering the cash limit
    let buyBenchmarkByPlan (benchmark:Benchmark) getPrice (store:DomainEvent list) =

        let buy day  =
            let price = day |> getPrice
            let value = benchmark.SavingsPlan.Rate
            let fee = value |> percent benchmark.SavingsPlan.Fee
            let count = (value - fee) / price
            { StockBought.Isin = benchmark.Isin
              Name = benchmark.Name
              Date = day
              Fee = fee
              Price = price
              Count = count }

        // TODO: anual fee
        // -> what about just inventing an event because it could then be calculated when walking the positions
    
        let start = store.Head |> Events.GetDate
        let stop = DateTime.Today
        let numMonths = (stop.Year - start.Year) * 12 + (stop.Month - start.Month + 1)
        
        seq {
            yield!  [0 .. numMonths]
                    |> Seq.map(fun m -> (new DateTime(start.Year, start.Month, 1)).AddMonths(m))
                    |> Seq.map workingDay
                    |> Seq.map(fun day -> day |> buy |> StockBought)
            yield closePosition benchmark getPrice |> PositionClosed
        }
        |> List.ofSeq

module StatisticsInteractor =
    open PositionsInteractor
    open RaynMaker.Portfolio.Entities

    type DiversificationReport = {
        Positions : (string*decimal<Currency>) list
        }

    let getDiversification (positions:Position list) =
        let investmentPerPositions =
            positions
            |> Seq.filter(fun p -> p.IsClosed |> not)        
            |> Seq.map(fun p -> p.Name,p.Invested)  
            |> List.ofSeq
        
        { Positions = investmentPerPositions } 
