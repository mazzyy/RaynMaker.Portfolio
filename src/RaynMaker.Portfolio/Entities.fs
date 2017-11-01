module RaynMaker.Portfolio.Entities

open System

[<Measure>] type Currency
[<Measure>] type Percentage

type Isin = Isin of string

module Str = 
    let ofIsin (Isin x) = x

[<AutoOpen>]
module Finance =
    let percent (p:decimal<Percentage>) (v:decimal<_>) =
        v * p / 100.0M<Percentage>

type StockBought = {
    Date : DateTime
    Isin : Isin
    Name : string
    Count : decimal
    Price : decimal<Currency>
    Fee : decimal<Currency> }

type StockSold = {
    Date : DateTime
    Isin : Isin
    Name : string
    Count : decimal
    Price : decimal<Currency>
    Fee : decimal<Currency> }

type DividendReceived = {
    Date : DateTime
    Isin : Isin
    Name : string
    Value : decimal<Currency>
    Fee : decimal<Currency> }

type DepositAccounted = {
    Date : DateTime
    Value : decimal<Currency> }

type DisbursementAccounted = {
    Date : DateTime
    Value : decimal<Currency> }

type InterestReceived = {
    Date : DateTime
    Value : decimal<Currency> }

/// notifies the (final) price of a stock at a given date
type StockPriced = {
    Date : DateTime
    Isin : Isin
    Name : string
    Price : decimal<Currency>
    Fee : decimal<Currency> }

type DomainEvent = 
    | StockBought of StockBought
    | StockSold of StockSold
    | DividendReceived of DividendReceived
    | DepositAccounted of DepositAccounted
    | DisbursementAccounted of DisbursementAccounted
    | InterestReceived of InterestReceived
    | StockPriced of StockPriced

type Price = {
    Day : DateTime
    Value : decimal<Currency>
    }

module Events =
    let GetDate event =
        match event with
        | StockBought e -> e.Date
        | StockSold e -> e.Date
        | DividendReceived e -> e.Date
        | DepositAccounted e -> e.Date
        | DisbursementAccounted e -> e.Date
        | InterestReceived e -> e.Date
        | StockPriced e -> e.Date

    /// searches for the last price information available
    let LastPriceOf events isin =
        events
        |> List.rev
        |> Seq.tryPick (function 
            | StockBought e when e.Isin = isin -> { Day = e.Date; Value = e.Price } |> Some 
            | StockSold e when e.Isin = isin -> { Day = e.Date; Value = e.Price } |> Some 
            | StockPriced e when e.Isin = isin -> { Day = e.Date; Value = e.Price } |> Some 
            | _ -> None)


