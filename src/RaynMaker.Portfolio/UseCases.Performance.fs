namespace RaynMaker.Portfolio.UseCases

open RaynMaker.Portfolio.Entities


module PerformanceInteractor =
    open System
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities

    // TODO: yield per month based on actual capital then take average
    type PerformanceReport = {
        TotalDeposit : decimal<Currency>
        TotalDisbursement : decimal<Currency>
        TotalInvestment : decimal<Currency>
        TotalCash : decimal<Currency>
        TotalDividends : decimal<Currency>
        CurrentPortfolioValue : decimal<Currency>
        TotalValue : decimal<Currency>
        TotalProfit : decimal<Currency>
        CashLimit : decimal<Currency>
        InvestingTime : TimeSpan
    }

    let getPerformance store getCashLimit getLastPrice positions =
        let totalDeposit = store |> List.sumBy(function | DepositAccounted evt -> evt.Value | _ -> 0.0M<Currency>)
        let totalDisbursement = store |> List.sumBy(function | DisbursementAccounted evt -> evt.Value | _ -> 0.0M<Currency>)
        let totalCash = store |> CashflowInteractor.getTotalCash
        let totalDividends =  store |> List.sumBy(function | DividendReceived evt -> evt.Value - evt.Fee | _ -> 0.0M<Currency>)
        // we intentionally ignore the broker fee here
        let currentPortfolioValue = 
            positions
            |> Seq.filter(fun p -> p.ClosedAt |> Option.isNone)
            |> Seq.sumBy(fun p -> (p.Isin |> getLastPrice |> Option.get).Value * p.Count)
        let totalValue = totalCash + currentPortfolioValue
        let totalInvestment = totalDeposit - totalDisbursement
        
        { 
            TotalDeposit = totalDeposit
            TotalDisbursement = totalDisbursement
            TotalInvestment = totalInvestment
            TotalCash = totalCash
            TotalDividends = totalDividends
            CurrentPortfolioValue = currentPortfolioValue
            TotalValue = totalValue
            TotalProfit = totalValue - totalInvestment
            CashLimit = getCashLimit()
            InvestingTime = DateTime.Today - (store |> Seq.tryHead |> Option.map DomainEvent.Date |? DateTime.Today) 
        }
