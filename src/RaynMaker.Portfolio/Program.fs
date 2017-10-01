module RaynMaker.Portfolio.Main

open System.Threading
open System
open System.Reflection
open System.IO
open RaynMaker.Portfolio.Frameworks
open RaynMaker.Portfolio.Gateways

[<EntryPoint>]
let main argv = 
    let home = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location )

    printfn "Loading events ..."

    let storeLocation = 
        match argv with
        | [|path|] -> path
        | x -> Path.GetFullPath( Path.Combine(home, "..", "..", "..", "..", "etc", "Portfolio.Events.xlsx") )

    let store = ExcelEventStore.load storeLocation

    store
    |> Seq.choose (function |ExcelEventStore.Unknown(a,b,c) -> Some(a,b,c) | _ -> None)
    |> Seq.iter(fun (e,d,p) -> printfn "Unknown event skipped: %A|%s|%A" d e p)
    
    printfn "Starting ..."

    let app = 
        store
        |> List.choose (function |ExcelEventStore.Event e -> Some e | _ -> None)
        |> WebApp.createApp home
    let port,cts = Httpd.start app

    Browser.start port

    Console.ReadKey true |> ignore
    
    cts.Cancel()

    0