namespace RaynMaker.Portfolio.UseCases

module CashflowInteractor =
    open System
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities

    type Transaction = {
        Date : DateTime
        Type : string
        Comment : string
        Value : decimal<Currency> 
        Balance : decimal<Currency> 
        }
    
    /// As commonly used in accounting the list is returned in reverse order
    let getTransactions limit (store:DomainEvent list) =
        let createTransaction =
            let comment name isin = sprintf "%s (Isin: %s)" name (isin |> Str.ofIsin)
            function
            | StockBought e -> 
                { Date = e.Date
                  Type = "Stock bought"
                  Comment = comment e.Name e.Isin
                  Value = -1.0M * e.Count * e.Price + e.Fee 
                  Balance = 0.0M<Currency> } 
                |> Some
            | StockSold e ->
                { Date = e.Date
                  Type = "Stock sold"
                  Comment = comment e.Name e.Isin
                  Value = e.Count * e.Price - e.Fee
                  Balance = 0.0M<Currency> } 
                |> Some
            | DividendReceived e ->
                { Date = e.Date
                  Type = "Dividend"
                  Comment = comment e.Name e.Isin
                  Value = e.Value - e.Fee
                  Balance = 0.0M<Currency> } 
                |> Some
            | DepositAccounted e ->
                { Date = e.Date
                  Type = "Deposit"
                  Comment = ""
                  Value = e.Value
                  Balance = 0.0M<Currency> } 
                |> Some
            | DisbursementAccounted e ->
                { Date = e.Date
                  Type = "Disbursement"
                  Comment = ""
                  Value = -1.0M * e.Value
                  Balance = 0.0M<Currency> } 
                |> Some
            | InterestReceived e ->
                { Date = e.Date
                  Type = "Interests"
                  Comment = ""
                  Value = e.Value 
                  Balance = 0.0M<Currency> } 
                |> Some
            | StockPriced e -> None

        let transactions,total = 
            store
            |> List.choose createTransaction
            |> List.mapFold (fun balance t -> 
                let newBalance = balance + t.Value
                {t with Balance = newBalance}, newBalance) 0.0M<Currency>
        
        transactions 
        |> List.rev 
        |> List.take ( Math.Min(transactions.Length, limit)) 