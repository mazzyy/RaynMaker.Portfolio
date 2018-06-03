namespace RaynMaker.Portfolio.Specs

open System
open NUnit.Framework
open FsUnit
open RaynMaker.Portfolio.Entities
open RaynMaker.Portfolio.UseCases

[<TestFixture>]
module ``Given HistoricalPrices`` =
    open FakeBroker

    [<Test>]
    let ``<When> queried with isin <Then> available prices for that isin are returned``() =
        let read isin =
            match isin with
            | Isin x when x = "I1" ->
                [
                    { Day = at 2014 01 01; Value = 10.0M<Currency> }
                    { Day = at 2014 01 01; Value = 10.0M<Currency> }
                ]
            | _ -> []

        let history = HistoricalPrices.create read

        "I1" |> Isin 
        |> history.Get 
        |> should equalList ("I1" |> Isin |> read)

    [<Test>]
    let ``<When> queried with unknown isin <Then> empty list is returned``() =
        let read isin = []

        let history = HistoricalPrices.create read

        "I1" |> Isin 
        |> history.Get 
        |> should be Empty


[<TestFixture>]
module ``Given a benchmark and a list of domain events`` =
    open FakeBroker
    open RaynMaker.Portfolio.UseCases.BenchmarkInteractor

    let broker = FakeBroker.instance
    let benchmark = { BenchmarkInteractor.Benchmark.Name = "ETF 123"; BenchmarkInteractor.Benchmark.Isin = "ETF 123" |> isin }
    let getPrice day = 
        let prices = 
            [
                { Day = at 2015 01 01; Value =  9.0M<Currency>}
                { Day = at 2015 02 01; Value = 10.0M<Currency>}
                { Day = at 2015 03 01; Value = 11.0M<Currency>}
                { Day = at 2015 04 01; Value = 12.0M<Currency>}
                { Day = at 2015 05 01; Value = 13.0M<Currency>}
                { Day = at 2015 06 01; Value = 15.0M<Currency>}
            ]
        match day |> Prices.getPrice 1.0 prices with
        | Some p -> p
        | None -> failwithf "No price found for %A" day

    [<Test>]
    let ``<When> investing in benchmark asset instead <Then> timing and capital is taken from events, prices from benchmark asset``() =
        let events =
            [
                at 2015 01 01 |> buy  "Joe Inc"   100 10.0 |> toDomainEvent
                at 2015 03 01 |> sell "Joe Inc"    25 15.0 |> toDomainEvent
                at 2015 06 01 |> buy  "Jack Corp" 100 20.0 |> toDomainEvent
            ]
        
        let count oldCount oldValue newValue =
            let value = (decimal oldCount) * (decimal oldValue) * 1.0M<Currency> |> withFee
            (value - (value |> getFee)) / (newValue |> decimal) / 1.0M<Currency>

        events
        |> BenchmarkInteractor.buyBenchmarkInstead broker benchmark getPrice
        |> should equalList <|
            [
                at 2015 01 01 |> buyd  "ETF 123" (count 100 10.0  9.0)  9.0 |> toDomainEvent
                at 2015 03 01 |> selld "ETF 123" (count  25 15.0 11.0) 11.0 |> toDomainEvent
                at 2015 06 01 |> buyd  "ETF 123" (count 100 20.0 15.0) 15.0 |> toDomainEvent
                at 2015 06 01 |> price "ETF 123"                       15.0 |> toDomainEvent
            ]

    [<Test>]
    let ``<When> buying benchmark asset with savings plan <Then> start date is taken from events, prices from benchmark asset, timing and capital from savings plan``() =
        let start = at 2015 01 01 
        let stop = at 2015 06 01 

        let plan = { SavingsPlan.Fee = 0.45M<Percentage>; AnnualFee = 0.45M<Percentage>; Rate = 500.0M<Currency> }

        let buy company day =
            let price = day |> getPrice
            let value = plan.Rate
            let fee = value |> percent plan.Fee
            let count = (value - fee) / price
            { StockBought.Date = day
              Name = company
              Isin = company |> isin
              Count = count
              Price = price
              Fee = fee } 

        BenchmarkInteractor.buyBenchmarkByPlan plan benchmark getPrice start stop
        |> should equalList <|
            [
                at 2015 01 01 |> buy   "ETF 123" |> toDomainEvent
                at 2015 02 01 |> buy   "ETF 123" |> toDomainEvent
                at 2015 03 01 |> buy   "ETF 123" |> toDomainEvent
                at 2015 04 01 |> buy   "ETF 123" |> toDomainEvent
                at 2015 05 01 |> buy   "ETF 123" |> toDomainEvent
                at 2015 06 01 |> buy   "ETF 123" |> toDomainEvent
                at 2015 06 01 |> price "ETF 123" 15.0 |> toDomainEvent
            ]
