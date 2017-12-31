namespace RaynMaker.Portfolio.UseCases

module CashflowInteractor =
    open System
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities

    type Transaction = {
        Date : DateTime
        Type : string
        Comment : string
        Value : decimal<Currency> }
    
    type Cashflow = {
        Transactions : Transaction list
        Total : decimal<Currency> }

    /// As commonly used in accounting the list is returned in reverse order
    let getTransactions limit (store:DomainEvent list) =
        let createTransaction =
            let comment name isin = sprintf "%s (Isin: %s)" name (isin |> Str.ofIsin)
            function
            | StockBought e -> 
                { Date = e.Date
                  Type = "Stock bought"
                  Comment = comment e.Name e.Isin
                  Value = -1.0M * e.Count * e.Price + e.Fee } 
                |> Some
            | StockSold e ->
                { Date = e.Date
                  Type = "Stock sold"
                  Comment = comment e.Name e.Isin
                  Value = e.Count * e.Price - e.Fee } 
                |> Some
            | DividendReceived e ->
                { Date = e.Date
                  Type = "Dividend"
                  Comment = comment e.Name e.Isin
                  Value = e.Value - e.Fee }
                |> Some
            | DepositAccounted e ->
                { Date = e.Date
                  Type = "Deposit"
                  Comment = ""
                  Value = e.Value }
                |> Some
            | DisbursementAccounted e ->
                { Date = e.Date
                  Type = "Disbursement"
                  Comment = ""
                  Value = -1.0M * e.Value }
                |> Some
            | InterestReceived e ->
                { Date = e.Date
                  Type = "Interests"
                  Comment = ""
                  Value = e.Value } 
                |> Some
            | StockPriced e -> None

        let transactions = 
            store
            |> List.choose createTransaction
        
        { Transactions = transactions |> List.rev |> List.take limit
          Total = transactions |> List.sumBy(fun t -> t.Value) }