namespace RaynMaker.Portfolio.Specs

open NUnit.Framework
open FsUnit
open RaynMaker.Portfolio.Entities

[<TestFixture>]
module ``Feature: Receiving dividends`` =
    open RaynMaker.Portfolio.TestAPI

    [<Test>]
    let ``<Given> a position of 25 stocks <When> receiving 2.00$ dividend each <And> paying a tax of 25% <Then> total received divident payment is 37.5$``() =
        [
            at 2014 01 01 |> buy "Joe Inc" (count 25) (price 40.0) (fee 1.0)
            at 2014 09 01 |> dividend "Joe Inc" (price 50.00) (fee 12.5)
        ]
        |> getNettoDividend "Joe Inc"
        |> should equal 37.5M<Currency>
