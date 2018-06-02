namespace RaynMaker.Portfolio.UseCases

module Depot =
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities

    type private Msg = 
        | Init of (unit -> DomainEvent list)
        | Get of AsyncReplyChannel<Position list>
        | Stop 

    type Api = {
        Get: unit -> Position list
        Stop: unit -> unit
    }

    let create init =
        let agent = Agent<Msg>.Start(fun inbox ->
            let rec loop store =
                async {
                    let! msg = inbox.Receive()

                    match msg with
                    | Init f -> return! loop (f() |> Positions.create)
                    | Get replyChannel -> 
                        replyChannel.Reply store
                        return! loop store
                    | Stop -> return ()
                }
            loop [] ) 

        agent.Error.Add(handleLastChanceException)
        
        agent.Post(init |> Init)

        { Get = fun () -> agent.PostAndReply( fun replyChannel -> replyChannel |> Get)
          Stop = fun () -> agent.Post Stop }

module PositionsInteractor =
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities
    open System

    type PositionEvaluation = {
        Position : Position
        PricedAt : DateTime
        BuyingPrice : decimal<Currency> option
        BuyingValue : decimal<Currency> option
        CurrentPrice : decimal<Currency>
        CurrentValue : decimal<Currency>
        MarketProfit : decimal<Currency>
        DividendProfit : decimal<Currency>
        MarketRoi : decimal<Percentage>
        DividendRoi : decimal<Percentage>
        MarketRoiAnual : decimal<Percentage>
        DividendRoiAnual : decimal<Percentage> }
        
    let evaluatePositions broker getLastPrice positions =
        let evaluate (p:Position) =
            let value,pricedAt = 
                match p.ClosedAt with
                | Some c -> p.Payouts,c
                | None -> let price = p.Isin |> getLastPrice |> Option.get // there has to be a price otherwise there would be no position
                          (p.Payouts + p.Count * price.Value - (Broker.getFee broker price.Value), price.Day)

            let investedYears = (pricedAt - p.OpenedAt).TotalDays / 365.0 |> decimal
            let marketRoi = (value - p.Invested) / p.Invested * 100.0M<Percentage>
            let dividendRoi = p.Dividends / p.Invested * 100.0M<Percentage>
            
            { 
                Position = p
                PricedAt = pricedAt
                BuyingPrice = if p.Count <> 0.0M then (p.Invested - p.Payouts) / p.Count |> Some else None
                BuyingValue = if p.Count <> 0.0M then p.Invested - p.Payouts |> Some else None
                CurrentPrice = (p.Isin |> getLastPrice |> Option.get).Value
                CurrentValue = (p.Isin |> getLastPrice |> Option.get).Value * p.Count
                MarketProfit = value - p.Invested
                DividendProfit = p.Dividends
                MarketRoi = marketRoi
                DividendRoi = dividendRoi
                MarketRoiAnual = marketRoi / investedYears 
                DividendRoiAnual = dividendRoi / investedYears
            }

        positions
        |> Seq.map evaluate
        |> Seq.sortByDescending(fun p -> p.MarketRoiAnual + p.DividendRoiAnual)
        |> List.ofSeq

module PerformanceInteractor =
    open PositionsInteractor
    open RaynMaker.Portfolio.Entities

    type PerformanceReport = {
        TotalInvestment : decimal<Currency>
        TotalProfit : decimal<Currency>
        }

    let getPerformance store broker getLastPrice positions =
        let sumInvestment total evt =
            match evt with
            | DepositAccounted evt -> printfn "%A - %A = %A" evt.Date evt.Value total; total + evt.Value
            | DisbursementAccounted evt -> printfn "%A - %A = %A" evt.Date evt.Value total; total - evt.Value
            | _ -> total

        let investment =
            store
            |> List.fold sumInvestment 0.0M<Currency>

        let totalProfit = 
            positions
            |> PositionsInteractor.evaluatePositions broker getLastPrice
            |> Seq.sumBy(fun p -> p.MarketProfit + p.DividendProfit)

        { TotalInvestment = investment
          TotalProfit = totalProfit }

module StatisticsInteractor =
    open PositionsInteractor
    open RaynMaker.Portfolio.Entities

    type DiversificationReport = {
        Positions : (string*decimal<Currency>) list
        }

    /// gets diversification report according to current value of the position 
    /// based on last price and share count
    let getDiversification getLastPrice (positions:Position list) =
        let investmentPerPositions =
            positions
            |> Seq.filter(fun p -> p.ClosedAt |> Option.isNone)        
            |> Seq.map(fun p -> 
                let price = p.Isin |> getLastPrice |> Option.get // there has to be a price otherwise there would be no position
                p.Name, p.Count * price.Value)  
            |> List.ofSeq
        
        { Positions = investmentPerPositions } 
