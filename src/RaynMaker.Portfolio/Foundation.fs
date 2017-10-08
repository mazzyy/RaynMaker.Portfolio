[<AutoOpen>]
module RaynMaker.Portfolio.Foundation

open System
open System.Collections.Generic

let (|?) = defaultArg

let (|EqualsI|_|) (lhs:string) rhs =
    if lhs.Equals(rhs, StringComparison.OrdinalIgnoreCase) then
        rhs |> Some
    else
        None 

let memoize f = 
    let cache = Dictionary<_,_>()
    fun x -> 
        if cache.ContainsKey(x) then 
            cache.[x] 
        else 
            let res = f x
            cache.[x] <- res
            res

let remember f = 
    let ret = lazy( f() )
    fun () -> ret.Value
