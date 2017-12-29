namespace RaynMaker.Portfolio.Specs

open System
open RaynMaker.Portfolio.Entities

[<AutoOpen>]
module internal DSL =
    let at year month day = new DateTime(year, month, day)

    let isin company = company.GetHashCode() |> sprintf "US%i" |> Isin

module internal FakeBroker = 
    let fee = 9.9M<Currency> 
    let dividendFee = 0.15M

    let buy company count (price:float) date =
        { StockBought.Date = date 
          Name = company
          Isin = company |> isin
          Count = count |> decimal
          Price = 1.0M<Currency> * (price |> decimal)
          Fee = fee } 

    let sell company count (price:float) date =
        { StockSold.Date = date 
          Name = company
          Isin = sprintf "US%i" (company.GetHashCode()) |> Isin
          Count = count |> decimal
          Price = 1.0M<Currency> * (price |> decimal)
          Fee = fee } 

    let price company (price:float) date =
        { StockPriced.Date = date 
          Name = company
          Isin = sprintf "US%i" (company.GetHashCode()) |> Isin
          Price = 1.0M<Currency> * (price |> decimal) }

    let dividends company (value:float) date =
        let value = 1.0M<Currency> * (value |> decimal)
        { DividendReceived.Date = date 
          Name = company
          Isin = sprintf "US%i" (company.GetHashCode()) |> Isin
          Value = value
          Fee =  value * dividendFee}


