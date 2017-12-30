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
        MarketProfit : decimal<Currency>
        DividendProfit : decimal<Currency>
        MarketRoi : decimal<Percentage>
        DividendRoi : decimal<Percentage>
        MarketRoiAnual : decimal<Percentage>
        DividendRoiAnual : decimal<Percentage> }
        
    let evaluatePositions broker getLastPrice positions =
        let evaluate (p:Position) =
            let p,pricedAt = 
                match p.ClosedAt with
                | Some c -> p,c
                | None -> let price = p.Isin |> getLastPrice |> Option.get // there has to be a price otherwise there would be no position
                          { p with Payouts = p.Payouts + p.Count * price.Value - (Broker.getFee broker price.Value)
                                   Count = 0.0M }, price.Day

            let investedYears = (pricedAt - p.OpenedAt).TotalDays / 365.0 |> decimal
            let marketRoi = (p.Payouts - p.Invested) / p.Invested * 100.0M<Percentage>
            let dividendRoi = p.Dividends / p.Invested * 100.0M<Percentage>
            
            { 
                Position = p
                PricedAt = pricedAt
                MarketProfit = p.Payouts - p.Invested
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

    let getDiversification (positions:Position list) =
        let investmentPerPositions =
            positions
            |> Seq.filter(fun p -> p.ClosedAt |> Option.isNone)        
            |> Seq.map(fun p -> p.Name,p.Invested)  
            |> List.ofSeq
        
        { Positions = investmentPerPositions } 
