namespace RaynMaker.Portfolio.Specs

open NUnit.Framework
open FsUnit
open RaynMaker.Portfolio.Entities

[<TestFixture>]
module ``Feature: Evaluate open positions`` =
    open RaynMaker.Portfolio.TestAPI

    [<Test>]
    let ``<Given> [buy 5 at 10$ fee 5$; buy 5 at 13$ fee 7$] <when> calculating the buying price/value <Then> it is 12.7$/127$``() =
        let eval =
            [
                at 2014 01 01 |> buy "Joe Inc" (count 5) (price 10.0) (fee 5.0)
                at 2014 06 01 |> buy "Joe Inc" (count 5) (price 13.0) (fee 7.0)
            ]
            |> evaluate "Joe Inc"
        
        eval.BuyingPrice
        |> Option.get
        |> should equal 12.7M<Currency>

        eval.BuyingValue
        |> Option.get
        |> should equal 127M<Currency>

    // buyingPrice: average of individual buying prices. Sold shares are subtracted from previous buying count
    [<Test>]
    let ``<Given> [buy 50 at 10$ fee 5$; sell 25 at 15$ fee 9$; buy 20 at 12$ fee 8$] <When> calculating the buying price <Then> it is 11.178$``() =
        [
            at 2014 01 01 |> buy  "Joe Inc" (count 50) (price 10.0) (fee 5.0)
            at 2014 06 01 |> sell "Joe Inc" (count 25) (price 15.0) (fee 9.0)
            at 2014 09 01 |> buy  "Joe Inc" (count 20) (price 12.0) (fee 8.0)
        ]
        |> evaluate "Joe Inc"
        |> fun x -> x.BuyingPrice
        |> Option.get
        |> should equal 11.178M<Currency> 

    [<Test>]
    let ``<Given> [buy 50 at 10$ fee 5$; sell 50 at 10$ fee 5$; buy 20 at 12$ fee 8$] <When> calculating the buying price <Then> it is 12.40$``() =
        [
            at 2014 01 01 |> buy  "Joe Inc" (count 50) (price 10.0) (fee 5.0)
            at 2014 06 01 |> sell "Joe Inc" (count 50) (price 15.0) (fee 9.0)
            at 2014 09 01 |> buy  "Joe Inc" (count 20) (price 12.0) (fee 8.0)
        ]
        |> evaluate "Joe Inc"
        |> fun x -> x.BuyingPrice
        |> Option.get
        |> should equal 12.40M<Currency> 


    [<Test>]
    let ``<Given> [buy 5 at 10$ fee 5$; buy 5 at 13$ fee 7$] <when> calculating the market profit at price 17$ <Then> it is 43$``() =
        let eval =
            [
                at 2014 01 01 |> buy "Joe Inc" (count 5) (price 10.0) (fee 5.0)
                at 2014 06 01 |> buy "Joe Inc" (count 5) (price 13.0) (fee 7.0)
                at 2014 09 01 |> priced "Joe Inc" (price 17.0)
            ]
            |> evaluate "Joe Inc"
        
        eval.MarketProfit
        |> should equal 43.0M<Currency>

    [<Test>]
    let ``<Given> a dividend of 23$ fee 2$ <and> a dividend of 17$ fee 1.5$ <When> calculating dividend profit <Then> it is 36.5``() =
        [
            at 2014 01 01 |> buy  "Joe Inc" (count 50) (price 10.0) (fee 5.0)
            at 2014 06 01 |> dividend "Joe Inc" (price 23.0) (fee 2.0)
            at 2014 09 01 |> dividend "Joe Inc" (price 17.0) (fee 1.5)
        ]
        |> evaluate "Joe Inc"
        |> fun x -> x.DividendProfit
        |> should equal 36.5M<Currency>




        (*
MarketRoi = marketRoi
DividendRoi = dividendRoi

MarketProfitAnnual = if investedYears = 0.0M then 0.0M<Currency> else (value - p.Invested) / investedYears
DividendProfitAnnual = if investedYears = 0.0M then 0.0M<Currency> else p.Dividends / investedYears

MarketRoiAnnual = if investedYears = 0.0M then 0.0M<Percentage> else marketRoi / investedYears 
DividendRoiAnnual = if investedYears = 0.0M then 0.0M<Percentage> else dividendRoi / investedYears
        *)