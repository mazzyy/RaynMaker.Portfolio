[<AutoOpen>]
module RaynMaker.Portfolio.Specs.TDK

open System
open NUnit.Framework.Constraints
open RaynMaker.Portfolio.Entities
open RaynMaker.Portfolio

let equalList x =
    new CollectionEquivalentConstraint(x)

let at year month day = new DateTime(year, month, day)
let isin company = company.GetHashCode() |> sprintf "US%i" |> AssetId.Isin
let count (v:int) = v |> decimal
let price (v:float) = (v |> decimal) * 1.0M<Currency>
let fee = price

let buy company count price fee date =
    {   Date = date 
        Name = company
        AssetId = company |> isin
        Count = count
        Price = price
        Fee = fee } 
    |> AssetBought

let sell company count price fee date =
    {   Date = date 
        Name = company
        AssetId = company |> isin
        Count = count
        Price = price
        Fee = fee } 
    |> AssetSold

let dividend company value fee date =
    {   DividendReceived.Date = date 
        Name = company
        Isin = company |> Isin.Isin
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
    {   Date = date 
        Name = company
        AssetId = company |> AssetId.Isin
        Price = price}
    |> AssetPriced

let getMostRecentPrice company events = 
    TestAPI.getMostRecentPrice events (isin company)

