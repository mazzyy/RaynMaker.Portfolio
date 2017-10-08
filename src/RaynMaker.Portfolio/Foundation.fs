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

/// if day is a working day this one is retuned, otherwise the next working day is returned
let workingDay (day:DateTime) =
    if day.DayOfWeek = DayOfWeek.Saturday then
        day.AddDays(2.0)
    elif day.DayOfWeek = DayOfWeek.Sunday then
        day.AddDays(1.0)
    else
        day
