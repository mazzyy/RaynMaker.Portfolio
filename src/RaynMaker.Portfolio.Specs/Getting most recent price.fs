namespace RaynMaker.Portfolio.Specs

open NUnit.Framework
open FsUnit
open RaynMaker.Portfolio
open RaynMaker.Portfolio.Entities

[<TestFixture>]
module ``Feature: Getting most recent price`` =

    [<Test>]
    let ``<Given> no transactions <When> searching for most recent price <Then> no price found``() =
        [] 
        |> getMostRecentPrice "Joe Inc"
        |> should equal None

    [<Test>]
    let ``<Given> last transaction of 'Joe Inc' was a 'buy' at 12$ <When> searching for most recent price <Then> 12$ is returned``() =
        [
            at 2016 10 10 |> buy "Joe Inc" (count 10) (price 10.0) (fee 5.0) 
            at 2016 11 10 |> sell "Joe Inc" (count 5) (price 11.0) (fee 5.0) 
            at 2016 12 10 |> buy "Joe Inc" (count 5) (price 12.0) (fee 5.0) 
            at 2016 12 12 |> buy "Jacky Limited" (count 15) (price 200.0) (fee 10.0) 
        ] 
        |> getMostRecentPrice "Joe Inc"
        |> should equal (Some 12.0M<Currency>)

    [<Test>]
    let ``<Given> last transaction of 'Joe Inc' was a 'sell' at 17$ <When> searching for most recent price <Then> 17$ is returned``() =
        [
            at 2016 10 10 |> buy "Joe Inc" (count 10) (price 10.0) (fee 5.0) 
            at 2016 11 10 |> sell "Joe Inc" (count 5) (price 17.0) (fee 5.0) 
            at 2016 12 12 |> buy "Jacky Limited" (count 15) (price 200.0) (fee 10.0) 
        ] 
        |> getMostRecentPrice "Joe Inc"
        |> should equal (Some 17.0M<Currency>)


    [<Test>]
    let ``<When> last transaction of 'Joe Inc' was a 'priced' event at 14$ <when> searching for most recent price <then> 14$ is returned``() =
        [
            at 2016 10 10 |> buy "Joe Inc" (count 10) (price 10.0) (fee 5.0) 
            at 2016 11 10 |> sell "Joe Inc" (count 5) (price 17.0) (fee 5.0) 
            at 2016 11 10 |> priced "Joe Inc" (price 14.0) 
            at 2016 12 12 |> buy "Jacky Limited" (count 15) (price 200.0) (fee 10.0) 
        ] 
        |> getMostRecentPrice "Joe Inc"
        |> should equal (Some 14.0M<Currency>)
