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
        Count : int
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
            Count = 0
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
                { p with Invested = p.Invested + (decimal evt.Count) * evt.Price + evt.Fee
                         Count = p.Count + evt.Count }
            newP::(positions |> List.filter ((<>) p))

        let sellStock positions (evt:StockSold) =
            // TODO: position must exist
            let p = evt.Isin |> getPosition positions |> Option.get
            let count = p.Count - evt.Count
            // TODO: count must not be zero
            let newP =
                { p with Payouts = p.Payouts + (decimal evt.Count) * evt.Price - evt.Fee
                         Count = count
                         Close = if count = 0 then evt.Date |> Some else None }
            newP::(positions |> List.filter ((<>) p))

        let closePosition positions (evt:PositionClosed) =
            // TODO: position must exist
            let p = evt.Isin |> getPosition positions |> Option.get
            // TODO: count must not be zero
            let newP =
                { p with Payouts = p.Payouts + (decimal p.Count) * evt.Price - evt.Fee
                         Count = 0 }
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

    let summarizePositions getInvestedDays positions =
        let summarizePosition p =
            let investedYears = (p |> getInvestedDays) / 365.0 |> decimal
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

    let summarizeClosedPositions positions =
        positions
        |> Seq.filter(fun p -> p.Close |> Option.isSome)
        |> summarizePositions (fun p -> ((p.Close |> Option.get) - p.Open).TotalDays)

    let summarizeOpenPositions positions =
        positions
        |> Seq.filter(fun p -> p.Close |> Option.isNone)
        |> summarizePositions (fun p -> (DateTime.Today - p.Open).TotalDays)

