namespace RaynMaker.Portfolio.UseCases

open RaynMaker.Portfolio.Entities


module PerformanceInteractor =
    open System
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities

    // TODO: yield per month based on actual capital then take average
    type PerformanceReport = {
        TotalInvestment : decimal<Currency>
        TotalProfit : decimal<Currency>
        TotalDividends : decimal<Currency>
        CashLimit : decimal<Currency>
        InvestingTime : TimeSpan }

    let getPerformance store broker getCashLimit getLastPrice positions =
        let sumInvestment total evt =
            match evt with
            | DepositAccounted evt -> total + evt.Value
            | DisbursementAccounted evt -> total - evt.Value
            | _ -> total

        let investment =
            store
            |> List.fold sumInvestment 0.0M<Currency>

        let totalProfit = 
            positions
            |> PositionsInteractor.evaluateTotalProfit broker getLastPrice

        let totalDividends = 
            store
            |> List.fold(fun total evt -> 
                match evt with
                | DividendReceived evt -> total + evt.Value - evt.Fee
                | _ -> total ) 0.0M<Currency>

        { 
            TotalInvestment = investment
            TotalProfit = totalProfit
            TotalDividends = totalDividends
            CashLimit = getCashLimit()
            InvestingTime = DateTime.Today - (store |> Seq.tryHead |> Option.map Events.GetDate |? DateTime.Today) 
        }
