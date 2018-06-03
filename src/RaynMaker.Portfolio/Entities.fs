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

module Prices = 
    let getPrice tolerance prices day =
        match prices with
        | [] -> None
        | prices ->
            let p = 
                match prices |> Seq.skipWhile(fun (p:Price) -> p.Day < day) |> Seq.tryHead with
                | Some p -> p 
                | None -> prices |> List.last

            if (day - p.Day).TotalDays <= tolerance then
                p.Value |> Some
            else
                None

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
    /// All the money ever invested into this position
    Invested : decimal<Currency>
    /// All the money already taken out of this position
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

    let accountBuy p (evt:StockBought) =
        Contract.requires (fun () -> p.Isin = evt.Isin) "evt.Isin = p.Isin"
        Contract.requires (fun () -> evt.Count > 0.0M) "evt.Count > 0"
        Contract.requires (fun () -> evt.Price > 0.0M<Currency>) "evt.Price > 0"
        Contract.requires (fun () -> evt.Fee >= 0.0M<Currency>) "evt.Fee >= 0"
        
        let newP =
            { p with Invested = p.Invested + (evt.Count * evt.Price + evt.Fee)
                     Count = p.Count + evt.Count }

        Contract.ensures (fun () -> newP.Count > 0.0M) "new count > 0"
        Contract.ensures (fun () -> newP.Invested > 0.0M<Currency>) "new invested > 0"
        Contract.ensures (fun () -> newP.Payouts >= 0.0M<Currency>) "new payouts >= 0"

        newP    
    
    let accountSell p (evt:StockSold) =
        Contract.requires (fun () -> p.Isin = evt.Isin) "evt.Isin = p.Isin"
        Contract.requires (fun () -> evt.Count > 0.0M) "evt.Count > 0"
        Contract.requires (fun () -> evt.Price > 0.0M<Currency>) "evt.Price > 0"
        Contract.requires (fun () -> evt.Fee >= 0.0M<Currency>) "evt.Fee >= 0"
        Contract.requires (fun () -> p.ClosedAt |> Option.isNone) "position is closed"

        let count = p.Count - evt.Count
        let newP =
            { p with Payouts = p.Payouts + (evt.Count * evt.Price - evt.Fee)
                     Count = count
                     ClosedAt = if count = 0.0M then evt.Date |> Some else None }

        Contract.ensures (fun () -> newP.Count >= 0.0M) "new count >= 0"
        Contract.ensures (fun () -> newP.Invested >= 0.0M<Currency>) "new invested >= 0"
        
        // unrealistic but it could be that we sell that less that it is equal to the fee
        //Contract.ensures (fun () -> newP.Payouts > 0.0M<Currency>) "new payouts > 0"
         
        newP

    let accountDividends p (evt:DividendReceived) =
        Contract.requires (fun () -> p.Isin = evt.Isin) "evt.Isin = p.Isin"
        Contract.requires (fun () -> p.ClosedAt |> Option.isNone) "position is closed"
        Contract.requires (fun () -> evt.Value > 0.0M<Currency>) "evt.Value > 0"
        Contract.requires (fun () -> evt.Fee >= 0.0M<Currency>) "evt.Fee >= 0"

        let newP =
            { p with Dividends = p.Dividends + evt.Value - evt.Fee }
    
        Contract.ensures (fun () -> newP.Dividends > 0.0M<Currency>) "new Dividends > 0"

        newP

    let create store =
        let update f positions =
            let p = positions |> f
            positions
            |> Map.remove p.Isin
            |> Map.add p.Isin p

        let buyStock (evt:StockBought) positions =
            let p = positions |> Map.tryFind evt.Isin |? openNew evt.Date evt.Isin evt.Name
            accountBuy p evt
            
        let sellStock (evt:StockSold) positions =
            let p = positions |> Map.tryFind evt.Isin |! (sprintf "Cannot sell stock %s (Isin: %s). No position exists" evt.Name (Str.ofIsin evt.Isin))
            accountSell p evt

        let receiveDividend (evt:DividendReceived) positions =
            let p = positions |> Map.tryFind evt.Isin |!(sprintf "Cannot get dividends for stock %s (Isin: %s). No position exists" evt.Name (Str.ofIsin evt.Isin))
            accountDividends p evt

        let processEvent positions evt =
            match evt with
            | StockBought evt -> positions |> update (buyStock evt)
            | StockSold evt  -> positions |> update (sellStock evt)
            | DividendReceived evt -> positions |> update (receiveDividend evt)
            | _ -> positions

        store
        |> List.fold processEvent Map.empty
        |> Map.toSeq
        |> Seq.map snd
        |> List.ofSeq
