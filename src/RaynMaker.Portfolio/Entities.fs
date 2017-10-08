namespace RaynMaker.Portfolio.Entities

open System

[<Measure>] type Currency
[<Measure>] type Percentage

[<AutoOpen>]
module Finance =
    let percent (p:decimal<Percentage>) (v:decimal<_>) =
        v * p / 100.0M<Percentage>

type StockBought = {
    Date : DateTime
    Isin : string
    Name : string
    Count : decimal
    Price : decimal<Currency>
    Fee : decimal<Currency>
    }

type StockSold = {
    Date : DateTime
    Isin : string
    Name : string
    Count : decimal
    Price : decimal<Currency>
    Fee : decimal<Currency>
    }

type DividendReceived = {
    Date : DateTime
    Isin : string
    Name : string
    Value : decimal<Currency>
    Fee : decimal<Currency>
    }

type DepositAccounted = {
    Date : DateTime
    Value : decimal<Currency>
    }

type DisbursementAccounted = {
    Date : DateTime
    Value : decimal<Currency>
    }

type InterestReceived = {
    Date : DateTime
    Value : decimal<Currency>
    }

/// Simulates that position is closed to get current performance
type PositionClosed = {
    Date : DateTime
    Isin : string
    Name : string
    Price : decimal<Currency>
    Fee : decimal<Currency>
    }

type DomainEvent = 
    | StockBought of StockBought
    | StockSold of StockSold
    | DividendReceived of DividendReceived
    | DepositAccounted of DepositAccounted
    | DisbursementAccounted of DisbursementAccounted
    | InterestReceived of InterestReceived
    | PositionClosed of PositionClosed

module Events =
    let GetDate event =
        match event with
        | StockBought e -> e.Date
        | StockSold e -> e.Date
        | DividendReceived e -> e.Date
        | DepositAccounted e -> e.Date
        | DisbursementAccounted e -> e.Date
        | InterestReceived e -> e.Date
        | PositionClosed e -> e.Date

type Price = {
    Day : DateTime
    Value : decimal<Currency>
    }
