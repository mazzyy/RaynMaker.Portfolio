[<AutoOpen>]
module RaynMaker.Portfolio.Specs.Foundation
    open NUnit.Framework.Constraints

    let equalList x =
        new CollectionEquivalentConstraint(x)


