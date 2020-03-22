[<AutoOpen>]
module RaynMaker.Portfolio.Specs.TDK

open System
open NUnit.Framework.Constraints
open RaynMaker.Portfolio.Entities
open RaynMaker.Portfolio

let equalList x =
    new CollectionEquivalentConstraint(x)

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

let getMostRecentPrice company events = 
    TestAPI.getMostRecentPrice events (isin company)

