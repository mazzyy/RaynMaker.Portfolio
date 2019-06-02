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

    type OpenPositionEvaluation = {
        Position : Position
        PricedAt : DateTime
        // TODO: why is there an option? if count is zero then the position is closed
        BuyingPrice : decimal<Currency> option
        BuyingValue : decimal<Currency> option
        CurrentPrice : decimal<Currency>
        CurrentValue : decimal<Currency>
        MarketProfit : decimal<Currency>
        DividendProfit : decimal<Currency>
        MarketRoi : decimal<Percentage>
        DividendRoi : decimal<Percentage>
        MarketProfitAnnual : decimal<Currency>
        DividendProfitAnnual : decimal<Currency>
        MarketRoiAnnual : decimal<Percentage>
        DividendRoiAnnual : decimal<Percentage> }
        
    let evaluateOpenPositions broker getLastPrice positions =
        let evaluate (p:Position) =
            let value,pricedAt = 
                let price = p.Isin |> getLastPrice |> Option.get // there has to be a price otherwise there would be no position
                p.Payouts + p.Count * price.Value - (Broker.getFee broker price.Value), price.Day

            let investedYears = (pricedAt - p.OpenedAt).TotalDays / 365.0 |> decimal
            let marketRoi = (value - p.Invested) / p.Invested * 100.0M<Percentage>
            // TODO: this is not 100% correct: we would have to compare the dividends to the 
            // invested capital in that point in time when we got the dividend
            let dividendRoi = p.Dividends / p.Invested * 100.0M<Percentage>
            
            { 
                Position = p
                PricedAt = pricedAt
                BuyingPrice = if p.Count <> 0.0M then (p.Invested - p.Payouts) / p.Count |> Some else None
                BuyingValue = p |> Positions.buyingValue
                CurrentPrice = (p.Isin |> getLastPrice |> Option.get).Value
                CurrentValue = (p.Isin |> getLastPrice |> Option.get).Value * p.Count
                MarketProfit = value - p.Invested
                DividendProfit = p.Dividends
                MarketRoi = marketRoi
                DividendRoi = dividendRoi
                MarketProfitAnnual = if investedYears = 0.0M then 0.0M<Currency> else (value - p.Invested) / investedYears
                DividendProfitAnnual = if investedYears = 0.0M then 0.0M<Currency> else p.Dividends / investedYears
                MarketRoiAnnual = if investedYears = 0.0M then 0.0M<Percentage> else marketRoi / investedYears 
                DividendRoiAnnual = if investedYears = 0.0M then 0.0M<Percentage> else dividendRoi / investedYears
            }

        positions
        |> Seq.filter(fun p -> p.ClosedAt |> Option.isNone)
        |> Seq.map evaluate
        |> List.ofSeq

    type ClosedPositionEvaluation = {
        Position : Position
        Duration : TimeSpan
        TotalProfit : decimal<Currency>
        TotalRoi : decimal<Percentage>
        MarketProfitAnnual : decimal<Currency>
        DividendProfitAnnual : decimal<Currency>
        MarketRoiAnnual : decimal<Percentage>
        DividendRoiAnnual : decimal<Percentage> }

    let evaluateClosedPositions positions =
        let evaluate (p:Position) =
            let value,pricedAt = p.Payouts,(p.ClosedAt |> Option.get)

            let investedYears = (pricedAt - p.OpenedAt).TotalDays / 365.0 |> decimal
            let marketRoi = (value - p.Invested) / p.Invested * 100.0M<Percentage>
            // TODO: this is not 100% correct: we would have to compare the dividends to the 
            // invested capital in that point in time when we got the dividend
            let dividendRoi = p.Dividends / p.Invested * 100.0M<Percentage>
            
            { 
                ClosedPositionEvaluation.Position = p
                Duration = pricedAt - p.OpenedAt
                TotalProfit = value - p.Invested + p.Dividends
                TotalRoi = marketRoi + dividendRoi
                MarketProfitAnnual = (value - p.Invested) / investedYears
                DividendProfitAnnual = p.Dividends / investedYears
                MarketRoiAnnual = marketRoi / investedYears 
                DividendRoiAnnual = dividendRoi / investedYears
            }

        positions
        |> Seq.filter(fun p -> p.ClosedAt |> Option.isSome)
        |> Seq.map evaluate
        |> List.ofSeq

    let evaluateTotalProfit broker getLastPrice positions =
        let evaluate (p:Position) =
            let value = 
                match p.ClosedAt with
                | Some c -> p.Payouts
                | None -> let price = p.Isin |> getLastPrice |> Option.get // there has to be a price otherwise there would be no position
                          p.Payouts + p.Count * price.Value - (Broker.getFee broker price.Value)
            
            value - p.Invested + p.Dividends

        positions
        |> Seq.map evaluate
        |> Seq.sum

    type ProfitEvaluation = {
        Profit : decimal<Currency>
        Roi : decimal<Percentage>
        RoiAnnual : decimal<Percentage> }

    let evaluateProfit broker getLastPrice positions =
        let evaluate (p:Position) =
            let value,pricedAt = 
                match p.ClosedAt with
                | Some c -> p.Payouts,c
                | None -> let price = p.Isin |> getLastPrice |> Option.get // there has to be a price otherwise there would be no position
                          p.Payouts + p.Count * price.Value - (Broker.getFee broker price.Value),price.Day

            let investedYears = (pricedAt - p.OpenedAt).TotalDays / 365.0 |> decimal
            let marketRoi = (value - p.Invested) / p.Invested * 100.0M<Percentage>
            // TODO: this is not 100% correct: we would have to compare the dividends to the 
            // invested capital in that point in time when we got the dividend
            let dividendRoi = p.Dividends / p.Invested * 100.0M<Percentage>
            
            { 
                Profit = value - p.Invested + p.Dividends
                Roi = marketRoi + dividendRoi
                RoiAnnual = (marketRoi / investedYears + dividendRoi / investedYears)
            }

        positions
        |> Seq.map evaluate
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
            |> PositionsInteractor.evaluateTotalProfit broker getLastPrice

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
