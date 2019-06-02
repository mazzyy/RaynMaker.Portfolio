namespace RaynMaker.Portfolio.Specs

open NUnit.Framework
open FsUnit
open RaynMaker.Portfolio.Entities

[<TestFixture>]
module ``Feature: Evaluate open positions`` =
    open RaynMaker.Portfolio.TestAPI

    [<Test>]
    let ``<Given> a position of 5 stocks bought at 10$ <And> a fee of 5$ <When> buying 5 more stocks at 13$ <And> a fee of 7$ <Then> the total buying price is 12.7$``() =
        [
            at 2014 01 01 |> buy "Joe Inc" (count 5) (price 10.0) (fee 5.0)
            at 2014 06 01 |> buy "Joe Inc" (count 5) (price 13.0) (fee 7.0)
        ]
        |> getBuyingPrice "Joe Inc"
        |> should equal 12.7M<Currency>





        (*
- buying price 
- buying value
- current price
- current value
MarketProfit = value - p.Invested
DividendProfit = p.Dividends
MarketRoi = marketRoi
DividendRoi = dividendRoi
MarketProfitAnnual = if investedYears = 0.0M then 0.0M<Currency> else (value - p.Invested) / investedYears
DividendProfitAnnual = if investedYears = 0.0M then 0.0M<Currency> else p.Dividends / investedYears
MarketRoiAnnual = if investedYears = 0.0M then 0.0M<Percentage> else marketRoi / investedYears 
DividendRoiAnnual = if investedYears = 0.0M then 0.0M<Percentage> else dividendRoi / investedYears

        *)