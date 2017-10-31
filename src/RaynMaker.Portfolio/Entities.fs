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

module EventStore =
    open RaynMaker.Portfolio

    type private Msg = 
        | Post of DomainEvent
        | Get of AsyncReplyChannel<DomainEvent list>
        | Stop 

    type Api = {
        Post: DomainEvent -> unit
        Get: unit -> DomainEvent list
        Stop: unit -> unit
    }

    let Instance =
        let agent = Agent<Msg>.Start(fun inbox ->
            let rec loop store =
                async {
                    let! msg = inbox.Receive()

                    match msg with
                    | Post evt -> return! loop (evt::store)
                    | Get replyChannel -> 
                        replyChannel.Reply (store |> List.rev)
                        return! loop store
                    | Stop -> return ()
                }
            loop [] ) 

        agent.Error.Add(handleLastChanceException)

        { Post = fun evt -> agent.Post(evt |> Post)
          Get = fun () -> agent.PostAndReply( fun replyChannel -> replyChannel |> Get)
          Stop = fun () -> agent.Post Stop }

