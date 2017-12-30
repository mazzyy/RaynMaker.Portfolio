namespace RaynMaker.Portfolio.Specs

open System
open RaynMaker.Portfolio.Entities

[<AutoOpen>]
module internal DSL =
    let at year month day = new DateTime(year, month, day)

    let isin company = company.GetHashCode() |> sprintf "US%i" |> Isin

module internal FakeBroker = 
    let instance = { 
        Name = "Dummy"
        Fee = 0.25M<Percentage>
        MinFee = 10.0M<Currency>
        MaxFee = 25.0M<Currency> }

    let getFee = Broker.getFee instance
    let withFee price = price + (getFee price)
    let withoutFee price = price - (getFee price)

    let dividendFee = 0.15M

    let buyd company count (price:float) date =
        let price' = 1.0M<Currency> * (price |> decimal)
        { StockBought.Date = date 
          Name = company
          Isin = company |> isin
          Count = count
          Price = price'
          Fee = price' |> getFee } 
    let buy company count (price:float) date = buyd company (count |> decimal) price date

    let selld company count (price:float) date =
        let price' = 1.0M<Currency> * (price |> decimal)
        { StockSold.Date = date 
          Name = company
          Isin = company  |> isin
          Count = count 
          Price = price'
          Fee = price' |> getFee } 
    let sell company count (price:float) date = selld company (count |> decimal) price date

    let price company (price:float) date =
        { StockPriced.Date = date 
          Name = company
          Isin = company  |> isin
          Price = 1.0M<Currency> * (price |> decimal) }

    let dividends company (value:float) date =
        let value = 1.0M<Currency> * (value |> decimal)
        { DividendReceived.Date = date 
          Name = company
          Isin = company  |> isin
          Value = value
          Fee =  value * dividendFee}

    let toDomainEvent (evt:obj) =
        match evt with
        | :? StockBought as x -> x |> StockBought
        | :? StockSold as x -> x |> StockSold
        | :? DividendReceived as x -> x |> DividendReceived
        | :? DepositAccounted as x  -> x |> DepositAccounted
        | :? DisbursementAccounted as x  -> x |> DisbursementAccounted
        | :? InterestReceived as x  -> x |> InterestReceived
        | :? StockPriced as x  -> x |> StockPriced
        | _ -> failwith "Not a domain event"
    