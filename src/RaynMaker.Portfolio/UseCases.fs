namespace RaynMaker.Portfolio.UseCases

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
        MarketRoi : float<Percentage>
        DividendRoi : float<Percentage>
        MarketRoiAnual : float<Percentage>
        DividendRoiAnual : float<Percentage>
        }

    type Position = {
        Open : DateTime 
        Close : DateTime option
        Isin : string
        Name : string
        Count : int
        Invested : decimal<Currency> /// total invested profit
        MarketProfit : decimal<Currency>  /// so far realized profit 
        DividendProfit : decimal<Currency>  /// so far realized profit 
        }

    let createPosition date isin name =
        { 
            Open = date
            Close = None
            Isin = isin
            Name = name
            Count = 0
            Invested = 0.0M<Currency>
            MarketProfit = 0.0M<Currency>
            DividendProfit = 0.0M<Currency>
        }

    let listClosed store =
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
            let p = evt.Isin |> getPosition positions |? createPosition evt.Date evt.Isin evt.Name
            let count = p.Count - evt.Count
            // TODO: count must not be zero
            let newP =
                { p with MarketProfit = p.MarketProfit + (decimal evt.Count) * evt.Price - evt.Fee
                         Count = count
                         Close = if count = 0 then evt.Date |> Some else None }
            newP::(positions |> List.filter ((<>) p))

        let receiveDividend positions (evt:DividendReceived) =
            // TODO: position not found actually is an error
            let p = evt.Isin |> getPosition positions |? createPosition evt.Date evt.Isin evt.Name
            let newP =
                { p with DividendProfit = p.DividendProfit + evt.Value - evt.Fee }
            newP::(positions |> List.filter ((<>) p))

        let processEvent positions evt =
            match evt with
            | StockBought evt -> evt |> buyStock positions
            | StockSold evt  -> evt |> sellStock positions
            | DividendReceived evt -> evt |> receiveDividend positions
            | _ -> positions
        
        let summarizePosition position =
            let close = Option.get position.Close
            let investedDays = 365.0 / (close -  position.Open).TotalDays
            let marketRoi = Convert.ToDouble(position.MarketProfit) / Convert.ToDouble(position.Invested) * 100.0<Percentage>
            let dividendRoi = Convert.ToDouble(position.DividendProfit) / Convert.ToDouble(position.Invested) * 100.0<Percentage>
            { 
                Open = position.Open
                Close = position.Close
                Isin = position.Isin
                Name = position.Name
                MarketProfit = position.MarketProfit
                DividendProfit = position.DividendProfit
                MarketRoi = marketRoi
                DividendRoi = dividendRoi
                MarketRoiAnual = investedDays * marketRoi
                DividendRoiAnual = investedDays * dividendRoi
            }

        store
        |> Seq.fold processEvent []
        |> Seq.filter(fun p -> p.Close |> Option.isSome)
        |> Seq.map summarizePosition
        |> List.ofSeq

