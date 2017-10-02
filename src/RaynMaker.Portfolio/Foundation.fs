[<AutoOpen>]
module RaynMaker.Portfolio.Foundation

open System

let (|?) = defaultArg

let (|EqualsI|_|) (lhs:string) rhs =
    if lhs.Equals(rhs, StringComparison.OrdinalIgnoreCase) then
        rhs |> Some
    else
        None 
