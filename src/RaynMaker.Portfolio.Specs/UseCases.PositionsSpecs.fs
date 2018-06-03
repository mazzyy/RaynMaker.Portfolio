namespace RaynMaker.Portfolio.Specs

open System
open NUnit.Framework
open FsUnit
open RaynMaker.Portfolio.Entities
open RaynMaker.Portfolio.UseCases

[<TestFixture>]
module ``Given some Depot`` =
    open FakeBroker

    [<Test>]
    let ``<When> initialized <Then> positions are created from domain events``() =
        let events =
            [
                at 2014 01 01 |> buy "Joe Inc" 10 10.0 |> toDomainEvent
                at 2015 01 01 |> buy "Jack Corp" 5 15.0 |> toDomainEvent
            ]

        let depot = Depot.create(fun () -> events)

        let positions = events |> Positions.create

        depot.Get() |> should equal positions 

[<TestFixture>]
module ``Given a closed position to evaluate`` =
    open FakeBroker

    [<Test>]
    let ``<When> provit is calcualted <Then> it is based on closed price``() =
        let events =
            [
                at 2014 01 01 |> buy "Joe Inc" 10 10.0 |> toDomainEvent
                at 2015 01 01 |> buy "Joe Inc" 5 15.0 |> toDomainEvent
                at 2016 01 01 |> sell "Joe Inc" 15 20.0 |> toDomainEvent
            ]

        let summary =
            events
            |> Positions.create
            |> PositionsInteractor.evaluateClosedPositions

        summary |> should haveLength 1

        let investedMoney = (withFee 100.0M<Currency>) + (withFee 75.0M<Currency>)
        let profit = (withoutFee 300.0M<Currency>) - investedMoney
        let roi = profit / investedMoney * 100.0M<Percentage>

        summary.[0].Position.ClosedAt |> should equal (at 2016 01 01 |> Some)
        summary.[0].TotalProfit |> should equal profit
        summary.[0].TotalRoi |> should equal roi
        summary.[0].MarketRoiAnnual |> should equal (roi / 2.0M)

[<TestFixture>]
module ``Given an open position to evaluate`` =
    open FakeBroker

    [<Test>]
    let ``<When> profit is calculated <Then> it is based on last price found in events``() =
        let events =
            [
                at 2014 01 01 |> buy "Joe Inc" 10 10.0 |> toDomainEvent
                at 2015 01 01 |> buy "Joe Inc" 5 15.0 |> toDomainEvent
                at 2016 01 01 |> price "Joe Inc" 20.0 |> toDomainEvent
            ]

        let summary =
            events
            |> Positions.create
            |> PositionsInteractor.evaluateOpenPositions FakeBroker.instance (Events.LastPriceOf events)

        summary |> should haveLength 1

        let investedMoney = (withFee 100.0M<Currency>) + (withFee 75.0M<Currency>)
        let profit = (withoutFee 300.0M<Currency>) - investedMoney
        let roi = profit / investedMoney * 100.0M<Percentage>

        summary.[0].PricedAt |> should equal (at 2016 01 01)
        summary.[0].Position.ClosedAt |> should equal None
        summary.[0].MarketProfit |> should equal profit
        summary.[0].MarketRoi |> should equal roi
        summary.[0].MarketRoiAnnual |> should equal (roi / 2.0M)

[<TestFixture>]
module ``Given a set of positions`` =
    open FakeBroker

    [<Test>]
    let ``<When> diversification is calculated <Then> closed positions are ignored``() =
        let events =
            [
                at 2014 01 01 |> buy "Joe Inc" 5 10.0 |> toDomainEvent
                at 2015 01 01 |> sell "Joe Inc" 5 15.0 |> toDomainEvent
                at 2016 01 01 |> buy "Jack Corp" 10 20.0 |> toDomainEvent
            ]

        let report =
            events
            |> Positions.create
            |> StatisticsInteractor.getDiversification (Events.LastPriceOf events)

        report.Positions |> should haveLength 1

    [<Test>]
    let ``<When> diversification is calculated <Then> positions are valued based on currently invested capital``() =
        let events =
            [
                at 2014 01 01 |> buy "Joe Inc" 5 100.0 |> toDomainEvent
                at 2015 01 01 |> sell "Joe Inc" 2 80.0 |> toDomainEvent
                at 2016 01 01 |> buy "Jack Corp" 10 20.0 |> toDomainEvent
            ]

        let report =
            events
            |> Positions.create
            |> StatisticsInteractor.getDiversification (Events.LastPriceOf events)

        let chunks =
            [
                "Joe Inc", (80.0M<Currency> * 3.0M)
                "Jack Corp", (20.0M<Currency> * 10.0M)
            ]

        report.Positions |> should equalList chunks
