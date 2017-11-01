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
    Price : decimal<Currency> }

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

type Broker = {
    Name : string
    Fee : decimal<Percentage>
    MinFee : decimal<Currency>
    MaxFee : decimal<Currency> }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Broker =
    let getFee broker value =
        match value |> percent broker.Fee with
        | x when x < broker.MinFee -> broker.MinFee
        | x when x > broker.MaxFee -> broker.MaxFee
        | x -> x

type Position = {
    OpenedAt : DateTime 
    ClosedAt : DateTime option
    Isin : Isin
    Name : string
    Count : decimal
    Invested : decimal<Currency> 
    Payouts : decimal<Currency>  
    Dividends : decimal<Currency> }

module Positions =
    let openNew date isin name =
        { 
            OpenedAt = date
            ClosedAt = None
            Isin = isin
            Name = name
            Count = 0.0M
            Invested = 0.0M<Currency>
            Payouts = 0.0M<Currency>
            Dividends = 0.0M<Currency>
        }

    let buy p (evt:StockBought) =
        Contract.requires (fun () -> p.Isin = evt.Isin) "evt.Isin = p.Isin"
        Contract.requires (fun () -> evt.Count > 0.0M) "evt.Count > 0"

        let investment = evt.Count * evt.Price + evt.Fee
        let newInvestment,payouts = 
            if investment > p.Payouts then 
                investment - p.Payouts,0.0M<Currency>
            else
                0.0M<Currency>,p.Payouts - investment

        { p with Invested = p.Invested + newInvestment
                 Payouts = payouts
                 Count = p.Count + evt.Count }
    
    let sell p (evt:StockSold) =
        Contract.requires (fun () -> p.Isin = evt.Isin) "evt.Isin = p.Isin"
        Contract.requires (fun () -> evt.Count > 0.0M) "evt.Count > 0"
        Contract.requires (fun () -> p.ClosedAt |> Option.isNone) "position is closed"

        let count = p.Count - evt.Count
        { p with Payouts = p.Payouts + evt.Count * evt.Price - evt.Fee
                 Count = count
                 ClosedAt = if count = 0.0M then evt.Date |> Some else None }

    let addDividends p (evt:DividendReceived) =
        Contract.requires (fun () -> p.Isin = evt.Isin) "evt.Isin = p.Isin"
        Contract.requires (fun () -> p.ClosedAt |> Option.isNone) "position is closed"

        { p with Dividends = p.Dividends + evt.Value - evt.Fee }
    
    let create store =
        let getPosition positions isin = 
            positions |> List.tryFind(fun p -> p.Isin = isin)

        let buyStock positions (evt:StockBought) =
            let p = evt.Isin |> getPosition positions |? openNew evt.Date evt.Isin evt.Name
            let newP = buy p evt
            newP::(positions |> List.filter ((<>) p))

        let sellStock positions (evt:StockSold) =
            let p = 
                match evt.Isin |> getPosition positions with
                | Some p -> p
                | None -> failwithf "Cannot sell stock %s (Isin: %s) because no position exists" evt.Name (Str.ofIsin evt.Isin)
            
            let newP = sell p evt
            newP::(positions |> List.filter ((<>) p))

        let receiveDividend positions (evt:DividendReceived) =
            // TODO: position not found actually is an error
            let p = evt.Isin |> getPosition positions |? openNew evt.Date evt.Isin evt.Name
            let newP = addDividends p evt
            newP::(positions |> List.filter ((<>) p))

        let processEvent positions evt =
            match evt with
            | StockBought evt -> evt |> buyStock positions
            | StockSold evt  -> evt |> sellStock positions
            | DividendReceived evt -> evt |> receiveDividend positions
            | _ -> positions

        store
        |> List.fold processEvent []
