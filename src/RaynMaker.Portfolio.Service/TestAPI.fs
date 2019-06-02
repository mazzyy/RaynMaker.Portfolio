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

let receiveDividend company value fee date =
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

let private getPosition company events = 
    events
    |> Positions.create 
    |> Seq.find(fun x -> x.Name = company)

let getActiveInvestment company = getPosition company >> Positions.activeInvestment

let getOwningStockCount company = getPosition company >> fun x -> x.Count

let getBalance = CashflowInteractor.getTransactions 1 >> Seq.head >> fun x -> x.Balance

let getNettoDividend company = getPosition company >> fun x -> x.Dividends
