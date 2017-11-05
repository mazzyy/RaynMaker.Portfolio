namespace RaynMaker.Portfolio.Specs

open System
open RaynMaker.Portfolio.Entities

[<AutoOpen>]
module internal FakeBroker = 
    let fee = 9.9M<Currency> 

    let at year month day = new DateTime(year, month, day)

    let isin company = company.GetHashCode() |> sprintf "US%i" |> Isin

    let buy company count (price:float) date =
        { StockBought.Date = date 
          Name = company
          Isin = company |> isin
          Count = count |> decimal
          Price = 1.0M<Currency> * (price |> decimal)
          Fee = fee
        } |> StockBought

    let sell company count (price:float) date =
        { StockSold.Date = date 
          Name = company
          Isin = sprintf "US%i" (company.GetHashCode()) |> Isin
          Count = count |> decimal
          Price = 1.0M<Currency> * (price |> decimal)
          Fee = fee
        } |> StockSold

    let price company (price:float) date =
        { StockPriced.Date = date 
          Name = company
          Isin = sprintf "US%i" (company.GetHashCode()) |> Isin
          Price = 1.0M<Currency> * (price |> decimal)
        } |> StockPriced


