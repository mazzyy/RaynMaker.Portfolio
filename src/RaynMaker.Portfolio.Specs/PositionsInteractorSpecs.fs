namespace RaynMaker.Portfolio.Specs.PositionsInteractor

open System
open NUnit.Framework
open FsUnit
open RaynMaker.Portfolio.Entities
open RaynMaker.Portfolio.Interactors

[<AutoOpen>]
module internal Broker = 
    let fee = 9.9M<Currency> 

    let at year month day = new DateTime(year, month, day)

    let buy company count price date =
        { StockBought.Date = date 
          Name = company
          Isin = sprintf "US%i" (company.GetHashCode())
          Count = count 
          Price = 1.0M<Currency> * (price |> decimal)
          Fee = fee
        } |> StockBought

[<TestFixture>]
module ``Given buy and sell events`` =
    
    [<Test>]
    let ``<When> only buy events given <Then> position is open and full invest is sumed up`` =
        let positions =
            [
                at 2016 10 10 |> buy "Joe Inc" 10 10.0
                at 2016 12 12 |> buy "Joe Inc" 5 15.0
            ]
            |> PositionsInteractor.getPositions

        positions |> should haveLength 1

        positions.[0].Open |> should equal (at 2016 10 10)
        positions.[0].Close |> should equal None
        positions.[0].Count |> should equal 15
        positions.[0].Invested |> should equal (10.0M * 10.0M<Currency> + fee)
        positions.[0].Payouts |> should equal 0.0M<Currency>
        positions.[0].Dividends |> should equal 0.0M<Currency>


