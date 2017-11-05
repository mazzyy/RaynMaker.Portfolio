namespace RaynMaker.Portfolio.Specs

open System
open NUnit.Framework
open FsUnit
open RaynMaker.Portfolio.Entities

[<TestFixture>]
module ``Given a sequence of events`` =
    [<Test>]
    let ``<When> empty <Then> no price found``() =
        [] 
        |> Events.LastPriceOf <| isin "Any Inc"
        |> should equal None

    [<Test>]
    let ``<When> last event is a 'buy' <Then> price is taken from that``() =
        [
            at 2016 10 10 |> buy "Joe Inc" 10 10.0
            at 2016 11 10 |> sell "Joe Inc" 5 11.0
            at 2016 12 10 |> buy "Joe Inc" 5 12.0
            at 2016 12 12 |> buy "Jacky Limited" 15 200.0
        ] 
        |> Events.LastPriceOf <| isin "Joe Inc"
        |> should equal ({ Day = at 2016 12 10; Value = 12.0M<Currency> } |> Some)

    [<Test>]
    let ``<When> last event is a 'sell' <Then> price is taken from that``() =
        [
            at 2016 10 10 |> buy "Joe Inc" 10 10.0
            at 2016 11 10 |> sell "Joe Inc" 5 11.0
            at 2016 12 12 |> buy "Jacky Limited" 15 200.0
        ] 
        |> Events.LastPriceOf <| isin "Joe Inc"
        |> should equal ({ Day = at 2016 11 10; Value = 11.0M<Currency> } |> Some)


    [<Test>]
    let ``<When> last event is a 'priced' <Then> price is taken from that``() =
        [
            at 2016 10 10 |> buy "Joe Inc" 10 10.0
            at 2016 11 10 |> sell "Joe Inc" 5 11.0
            at 2016 12 11 |> price "Joe Inc" 14.0
            at 2016 12 12 |> buy "Jacky Limited" 15 200.0
        ] 
        |> Events.LastPriceOf <| isin "Joe Inc"
        |> should equal ({ Day = at 2016 12 11; Value = 14.0M<Currency> } |> Some)

[<TestFixture>]
module ``Given a list of prices`` =
    let priceOf value day =
        { Day = day; Value = value * 1.0M<Currency> }

    [<Test>]
    let ``<When> empty <Then> no price found``() =
        [] 
        |> Prices.getPrice 2.0 <| at 2016 10 10
        |> should equal None

    [<Test>]
    let ``<When> date exactly exists <Then> price of that date is returned``() =
        [
            at 2015 01 01 |> priceOf 10.0M
            at 2015 01 02 |> priceOf 11.0M
            at 2015 01 03 |> priceOf 12.0M
        ] 
        |> Prices.getPrice 2.0 <| at 2015 01 02
        |> should equal (11.0M<Currency> |> Some)

    [<Test>]
    let ``<When> lastest date is within tolerance <Then> price of that date is returned``() =
        [
            at 2015 01 01 |> priceOf 10.0M
            at 2015 01 02 |> priceOf 11.0M
            at 2015 01 03 |> priceOf 12.0M
        ] 
        |> Prices.getPrice 2.0 <| at 2015 01 05
        |> should equal (12.0M<Currency> |> Some)

    [<Test>]
    let ``<When> lastest date is outside tolerance <Then> no price found``() =
        [
            at 2015 01 01 |> priceOf 10.0M
            at 2015 01 02 |> priceOf 11.0M
            at 2015 01 03 |> priceOf 12.0M
        ] 
        |> Prices.getPrice 2.0 <| at 2015 01 06
        |> should equal None

[<TestFixture>]
module ``Given a broker`` =
    let broker = { Name = "dummy"; Fee = 0.25M<Percentage>; MinFee = 10.0M<Currency>; MaxFee = 50.0M<Currency> }
    
    [<Test>]
    let ``<When> traded value causes fee smaller than minimal fee <Then> minimal fee is returned``() =
        1000.0M<Currency>
        |> Broker.getFee broker
        |> should equal broker.MinFee
    
    [<Test>]
    let ``<When> traded value causes fee bigger than maximal fee <Then> maximal fee is returned``() =
        100000.0M<Currency>
        |> Broker.getFee broker
        |> should equal broker.MaxFee
    
    [<Test>]
    let ``<When> traded value causes fee within range of min and max fee <Then> correct fee is returned``() =
        5000.0M<Currency>
        |> Broker.getFee broker
        |> should equal 12.5M<Currency>

//[<TestFixture>]
//module ``Given a position`` =
   
//    [<Test>]
//    let ``<When> buying new shares <Then> share count and cashflow is correctly updated``() =
//        let position = Positions.openNew (at 2015 01 01) (isin "Joe Inc") "Joe Inc"

//        let newPosition =
//            { StockBought.Date = date 
//              Name = company
//              Isin = company |> isin
//              Count = count |> decimal
//              Price = 1.0M<Currency> * (price |> decimal)
//              Fee = fee
//            } 
//            |> Positions.buy position

//        newPosition.Count |> should equal 17

