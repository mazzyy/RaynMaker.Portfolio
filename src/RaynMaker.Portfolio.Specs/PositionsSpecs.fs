namespace RaynMaker.Portfolio.Specs

open System
open NUnit.Framework
open FsUnit
open RaynMaker.Portfolio.Entities
open RaynMaker.Portfolio.Interactors

[<TestFixture>]
module ``Given some stock transactions`` =
    let broker = { 
        Name = "Dummy"
        Fee = 0.25M<Percentage>
        MinFee = 10.0M<Currency>
        MaxFee = 25.0M<Currency> }

    [<Test>]
    let ``<When> stock only bought <Then> position is open and full invest is sumed up``() =
        let positions =
            [
                at 2016 10 10 |> buy "Joe Inc" 10 10.0
                at 2016 12 12 |> buy "Joe Inc" 5 15.0
            ]
            |> Positions.create

        positions |> should haveLength 1

        positions.[0].OpenedAt |> should equal (at 2016 10 10)
        positions.[0].ClosedAt |> should equal None
        positions.[0].Count |> should equal 15
        positions.[0].Invested |> should equal ((10.0M * 10.0M<Currency> + fee) + (5.0M * 15.0M<Currency> + fee))
        positions.[0].Payouts |> should equal 0.0M<Currency>
        positions.[0].Dividends |> should equal 0.0M<Currency>

    [<Test>]
    let ``<When> an open position is closed <Then> position summary shows profits and ROIs``() =
        let events =
            [
                at 2014 01 01 |> buy "Joe Inc" 10 10.0
                at 2015 01 01 |> buy "Joe Inc" 5 15.0
                at 2016 01 01 |> sell "Joe Inc" 15 20.0
            ]

        let summary =
            events
            |> Positions.create
            |> PositionsInteractor.evaluatePositions broker (Events.LastPriceOf events)

        summary |> should haveLength 1

        let investedMoney = 175.0M<Currency> + (2.0M * fee)
        let profit = 300.0M<Currency> - fee - investedMoney
        let roi = profit / investedMoney * 100.0M<Percentage>

        summary.[0].PricedAt |> should equal (at 2016 01 01)
        summary.[0].Position.ClosedAt |> should equal (at 2016 01 01 |> Some)
        summary.[0].MarketProfit |> should equal profit
        summary.[0].MarketRoi |> should equal roi
        summary.[0].MarketRoiAnual |> should equal (roi / 2.0M)


