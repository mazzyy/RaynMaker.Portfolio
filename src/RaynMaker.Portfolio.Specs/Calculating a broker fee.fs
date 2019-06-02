namespace RaynMaker.Portfolio.Specs

open NUnit.Framework
open FsUnit
open RaynMaker.Portfolio.Entities

[<TestFixture>]
module ``Feature: Calculating a broker fee`` =
    open RaynMaker.Portfolio.TestAPI

    let broker = { Name = "dummy"; Fee = 0.25M<Percentage>; MinFee = 10.0M<Currency>; MaxFee = 50.0M<Currency> }
    
    [<Test>]
    let ``<Given> a small traded value <When> calculating broker fee <Then> broker's minimal fee is returned``() =
        1000.0M<Currency>
        |> getFee broker
        |> should equal broker.MinFee
    
    [<Test>]
    let ``<Given> a huge traded value <When> calculating broker fee <Then> broker's maximal fee is returned``() =
        100000.0M<Currency>
        |> getFee broker
        |> should equal broker.MaxFee
    
    [<Test>]
    let ``<Given> an traded value <When> calculating broker fee <Then> broker's percentage based fee is returned``() =
        5000.0M<Currency>
        |> getFee broker
        |> should equal 12.5M<Currency>

