namespace RaynMaker.Portfolio.Specs

open System
open NUnit.Framework
open FsUnit
open RaynMaker.Portfolio.Entities

[<TestFixture>]
module ``Given a sequence of events`` =
    open FakeEvents

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

[<TestFixture>]
module ``Given a position`` =
    let openNew at = Positions.openNew at (isin "Joe Inc") "Joe Inc"
    let buy = FakeBroker.buy "Joe Inc"
    let sell = FakeBroker.sell "Joe Inc"
    let dividends = FakeBroker.dividends "Joe Inc"

    [<Test>]
    let ``<When> buying new shares <Then> share count and position value increased by bought shares``() =
        let p = openNew (at 2015 01 01)

        let p =
            [
                at 2015 04 01 |> buy  5 17.5 
                at 2015 06 01 |> buy  7 18.9 
            ]
            |> Seq.fold Positions.accountBuy p

        p.Count |> should equal 12
        p.Invested |> should equal ((5M * 17.5M<Currency> + FakeBroker.fee) + (7M * 18.9M<Currency> + FakeBroker.fee))
        p.Dividends |> should equal 0.0M<Currency>
        p.Payouts |> should equal 0.0M<Currency>

    [<Test>]
    let ``<When> selling some shares <Then> share count and position value decreased by the sold shares``() =
        let p = openNew (at 2015 01 01)

        let p = at 2015 04 01 |> buy  5 17.5 |> Positions.accountBuy p 
        let p = at 2015 06 01 |> sell 2 18.9 |> Positions.accountSell p

        p.Count |> should equal 3
        p.Invested |> should equal (5M * 17.5M<Currency> + FakeBroker.fee)
        p.Dividends |> should equal 0.0M<Currency>
        p.Payouts |> should equal (2M * 18.9M<Currency> - FakeBroker.fee)

    [<Test>]
    let ``<When> selling all shares <Then> position is closed, share count is zero and position value is calculated by sold price``() =
        let p = openNew (at 2015 01 01)

        let p = at 2015 04 01 |> buy  5 17.5 |> Positions.accountBuy p 
        let p = at 2015 06 01 |> sell 5 18.9 |> Positions.accountSell p

        p.Count |> should equal 0
        p.ClosedAt |> should equal (at 2015 06 01 |> Some)
        p.Invested |> should equal (5M * 17.5M<Currency> + FakeBroker.fee)
        p.Dividends |> should equal 0.0M<Currency>
        p.Payouts |> should equal (5M * 18.9M<Currency> - FakeBroker.fee)

    [<Test>]
    let ``<When> receiving dividends <Then> position dividends value is increased by dividend``() =
        let p = openNew (at 2015 01 01)

        let p = at 2015 04 01 |> buy  5 17.5 |> Positions.accountBuy p 
        let p = at 2015 06 01 |> dividends 23.4 |> Positions.accountDividends p

        p.Count |> should equal 5
        p.Invested |> should equal (5M * 17.5M<Currency> + FakeBroker.fee)
        p.Dividends |> should equal (23.4M<Currency> * (1.0M - FakeBroker.dividendFee))
        p.Payouts |> should equal 0.0M<Currency>

[<TestFixture>]
module ``Given a list of stock events`` =
    open FakeEvents
    [<Test>]
    let ``<When> multipe stocks are bought <Then> multiple positions are created``() =
        let positions =
            [
                at 2015 04 01 |> buy  "A Inc" 100 17.5 
                at 2015 06 01 |> buy  "C Limited" 5 102.0
                at 2015 10 01 |> dividend "A Inc" 2.1
                at 2016 01 01 |> buy  "B Corp" 20 42.1 
                at 2016 06 01 |> buy  "B Corp" 10 51.7 
                at 2016 10 01 |> dividend "A Inc" 2.9
                at 2016 12 01 |> sell  "C Limited" 5 202.0
            ]
            |> Positions.create

        positions |> should haveLength 3
        
        let ``A Inc`` = positions |> Seq.find(fun p -> p.Name = "A Inc")
        ``A Inc``.ClosedAt |> should equal None 
        ``A Inc``.Count |> should equal 100
        ``A Inc``.Dividends |> should equal (2.1M<Currency> * (1.0M - FakeBroker.dividendFee) + 2.9M<Currency> * (1.0M - FakeBroker.dividendFee))
        ``A Inc``.Invested |> should equal (17.5M<Currency> * 100.0M + FakeBroker.fee)
        ``A Inc``.Payouts |> should equal 0.0M<Currency>
        
        let ``B Corp`` = positions |> Seq.find(fun p -> p.Name = "B Corp")
        ``B Corp``.ClosedAt |> should equal None
        ``B Corp``.Count |> should equal 30.0M
        ``B Corp``.Dividends |> should equal 0.0M<Currency>
        ``B Corp``.Invested |> should equal (42.1M<Currency> * 20.0M + FakeBroker.fee + 51.7M<Currency> * 10.0M + FakeBroker.fee)
        ``B Corp``.Payouts |> should equal 0.0M<Currency>
        
        let ``C Limited`` = positions |> Seq.find(fun p -> p.Name = "C Limited")
        ``C Limited``.ClosedAt |> should equal (at 2016 12 01 |> Some)
        ``C Limited``.Count |> should equal 0.0M
        ``C Limited``.Dividends |> should equal 0.0M<Currency>
        ``C Limited``.Invested |> should equal (102M<Currency> * 5.0M + FakeBroker.fee)
        ``C Limited``.Payouts |> should equal (202.0M<Currency> * 5.0M - FakeBroker.fee);
