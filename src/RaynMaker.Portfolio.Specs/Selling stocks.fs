namespace RaynMaker.Portfolio.Specs

open NUnit.Framework
open FsUnit
open RaynMaker.Portfolio.Entities

[<TestFixture>]
module ``Feature: Selling stocks`` =
    open RaynMaker.Portfolio.TestAPI

    [<Test>]
    let ``<Given> an open position of 25 stocks <When> selling 10 stocks <Then> the total owning stock count is 15``() =
        [
            at 2014 01 01 |> buy "Joe Inc" (count 25) (price 40.0) (fee 1.0)
            at 2014 03 01 |> sell "Joe Inc" (count 5) (price 45.0) (fee 1.0)
            at 2014 06 01 |> sell "Joe Inc" (count 5) (price 50.0) (fee 1.0)
        ]
        |> getOwningStockCount "Joe Inc"
        |> should equal 15

    [<Test>]
    let ``<Given> a position with an investment of 1000$ <When> selling for 500$ <And> a fee of 9$ <Then> the position is an investment of 509$``() =
        [
            at 2014 01 01 |> buy "Joe Inc" (count 20) (price 50.0) (fee 0.0)
            at 2015 01 01 |> sell "Joe Inc" (count 10) (price 50.0) (fee 9.0)
        ]
        |> getActiveInvestment "Joe Inc"
        |> should equal 509.0M<Currency>

    [<Test>]
    let ``<Given> an active position <When> selling all stocks <Then> the position is closed``() =
        [
            at 2014 01 01 |> buy "Joe Inc" (count 20) (price 50.0) (fee 10.0)
            at 2014 01 01 |> sell "Joe Inc" (count 20) (price 60.0) (fee 11.0)
        ]
        |> getActiveInvestment "Joe Inc"
        |> should equal 0.0M<Currency>

    [<Test>]
    let ``<Given> 1000$ cash <When> selling 10 stocks for 50$ each <And> paying 7$ fee <Then> cash is 1493$``() =
        [
            at 2014 01 01 |> deposit (price 1500.0)
            at 2015 01 01 |> buy "Joe Inc" (count 10) (price 50.0) (fee 0.0)  
            at 2015 01 01 |> sell "Joe Inc" (count 10) (price 50.0) (fee 7.0)  
        ]
        |> getBalance
        |> should equal 1493.0M<Currency>

