namespace RaynMaker.Portfolio.Specs

open NUnit.Framework
open FsUnit
open RaynMaker.Portfolio.Entities

[<TestFixture>]
module ``Feature: Adding and removing cash`` =
    open RaynMaker.Portfolio.TestAPI
    
    [<Test>]
    let ``<Given> 1000$ cash <When> making a deposit of 500$ <Then> remaining cash is 1500$``() =
        [
            at 2014 01 01 |> deposit (price 1000.0)
            at 2014 02 01 |> deposit (price 500.0)
        ]
        |> getBalance
        |> should equal 1500.0M<Currency>

    
    [<Test>]
    let ``<Given> 1000$ cash <When> making a disbursement of 500$ <Then> remaining cash is 500$``() =
        [
            at 2014 01 01 |> deposit (price 1000.0)
            at 2014 02 01 |> disbursement (price 500.0)
        ]
        |> getBalance
        |> should equal 500.0M<Currency>

