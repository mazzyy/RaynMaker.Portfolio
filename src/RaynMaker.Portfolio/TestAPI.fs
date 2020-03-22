module RaynMaker.Portfolio.TestAPI

open System
open Entities
open RaynMaker.Portfolio.UseCases

let at year month day = new DateTime(year, month, day)
let isin company = company.GetHashCode() |> sprintf "US%i" |> Isin
let count (v:int) = v |> decimal
let price (v:float) = (v |> decimal) * 1.0M<Currency>
let fee = price

let buy company count price fee date =
    {   StockBought.Date = date 
        Name = company
        Isin = company |> isin
        Count = count
        Price = price
        Fee = fee } 
    |> StockBought

let sell company count price fee date =
    {   StockSold.Date = date 
        Name = company
        Isin = company |> isin
        Count = count
        Price = price
        Fee = fee } 
    |> StockSold

let dividend company value fee date =
    {   DividendReceived.Date = date 
        Name = company
        Isin = company |> isin
        Value = value
        Fee = fee } 
    |> DividendReceived

let deposit value date =
    {   DepositAccounted.Date = date
        Value = value }
    |> DepositAccounted 

let disbursement value date =
    {   DisbursementAccounted.Date = date
        Value = value }
    |> DisbursementAccounted 

let priced company price date =
    {   StockPriced.Date = date 
        Name = company
        Isin = company  |> isin
        Price = price}
    |> StockPriced

let private getPosition company events = 
    events
    |> Positions.create 
    |> Seq.find(fun x -> x.Name = company)

// TODO: is this too detailed?
// i think we can return "entities" here - those are tha UL in DDD - we can use those in BDD

let getActiveInvestment company = getPosition company >> Positions.activeInvestment

let getOwningStockCount company = getPosition company >> fun x -> x.Count

let getBalance = CashflowInteractor.getTransactions 1 >> Seq.head >> fun x -> x.Balance

let getNettoDividend company = getPosition company >> fun x -> x.Dividends

let getMostRecentPrice company events = 
    events 
    |> Events.LastPriceOf <| isin company
    |> Option.map(fun x -> x.Value)

let getFee broker = Broker.getFee broker

let fixedFeeBroker fee = { Name = "FixedFee"; Fee = 0.0M<Percentage>; MinFee = fee; MaxFee = fee }

let private ignoreBroker = fixedFeeBroker 0.0M<Currency>

let evaluate company events = 
    let getPrice = Events.LastPriceOf events
    events
    |> (getPosition company >> List.singleton >> PositionsInteractor.evaluateOpenPositions ignoreBroker getPrice)
    |> Seq.head

