[<AutoOpen>]
module RaynMaker.Portfolio.Specs.TDK
    open NUnit.Framework.Constraints

    let equalList x =
        new CollectionEquivalentConstraint(x)


