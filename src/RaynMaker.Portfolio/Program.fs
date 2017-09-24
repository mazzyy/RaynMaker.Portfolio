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

    let store = EventStore.load storeLocation

    printfn "Starting ..."

    let app = WebApp.createApp home
    let port,cts = Httpd.start app

    Browser.start port

    Console.ReadKey true |> ignore
    
    cts.Cancel()

    0