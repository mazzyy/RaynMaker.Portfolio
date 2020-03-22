module RaynMaker.Portfolio.TestAPI

open System
open Entities
open RaynMaker.Portfolio.UseCases

[<AutoOpen>]
module private Impl =

    let getPosition company events = 
        events
        |> Positions.create 
        |> Seq.find(fun x -> x.Name = company)
  
// TODO: think about grouping into "positions", "cash", "performance evaluation", "benchmark"  
// maybe "managing positions" ("portfolio management") and "cash" can be "Accounting"

let getActiveInvestment company = getPosition company >> Positions.activeInvestment

let getOwningStockCount company = getPosition company >> fun x -> x.Count

let getBalance = CashflowInteractor.getTransactions 1 >> Seq.head >> fun x -> x.Balance

let getNettoDividend company = getPosition company >> fun x -> x.Dividends

let getMostRecentPrice events isin = 
    isin
    |> Events.LastPriceOf events
    |> Option.map(fun x -> x.Value)

let getFee broker = Broker.getFee broker

let fixedFeeBroker fee = { Name = "FixedFee"; Fee = 0.0M<Percentage>; MinFee = fee; MaxFee = fee }

let evaluate company events = 
    let getPrice = Events.LastPriceOf events
    let ignoreBroker = fixedFeeBroker 0.0M<Currency>
    
    events
    |> (getPosition company >> List.singleton >> PositionsInteractor.evaluateOpenPositions ignoreBroker getPrice)
    |> Seq.head

