namespace RaynMaker.Portfolio.Specs

open System
open NUnit.Framework
open FsUnit
open RaynMaker.Portfolio.Entities

[<TestFixture>]
module ``Given a sequence of events`` =
    [<Test>]
    let ``<When> empty <Then> not last price can be found``() =
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

