[<AutoOpen>]
module RaynMaker.Portfolio.Specs.Foundations
    open FsUnit
    open NUnit.Framework.Constraints

    let equalList x =
        new CollectionEquivalentConstraint(x)


