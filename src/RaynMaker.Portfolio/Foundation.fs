[<AutoOpen>]
module RaynMaker.Portfolio.Foundation

open System
open System.Collections.Generic
open Plainion

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

/// MailboxProcessor catching unhandled exceptions and reporting those as event.
type Agent<'T> private(f:Agent<'T> -> Async<unit>) as self =
    let event = Event<_>()
    let inbox = new MailboxProcessor<_>(fun _ ->
        let rec loop() = 
            async {
                try 
                    return! f self
                with e ->
                    event.Trigger(e)
                    return! loop()
            }
        loop())
    member __.Error = event.Publish
    member __.Start() = inbox.Start()
    member __.Receive() = inbox.Receive()
    member __.Post(v:'T) = inbox.Post(v)
    member __.PostAndReply(buildMessage:(AsyncReplyChannel<'Reply> -> 'T)) = inbox.PostAndReply(buildMessage)
    member __.PostAndAsyncReply(buildMessage:(AsyncReplyChannel<'Reply> -> 'T)) = inbox.PostAndAsyncReply(buildMessage)
    static member Start(f) =
        let mbox = new Agent<_>(f)
        mbox.Start()
        mbox

let dumpException (ex:Exception) = 
    ex |> ExceptionExtensions.Dump

let handleLastChanceException (ex:Exception) = 
    Console.Error.WriteLine( "FATAL ERROR: " + ExceptionExtensions.Dump(ex) )

    Environment.Exit(1)