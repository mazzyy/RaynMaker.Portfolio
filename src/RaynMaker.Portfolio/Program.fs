module RaynMaker.Portfolio.Main

open System.Threading
open System
open System.Reflection
open System.IO
open RaynMaker.Portfolio.Frameworks
open RaynMaker.Portfolio.Gateways


[<EntryPoint>]
let main argv = 
    printfn "Starting ..."

    let home = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location )

    let app = WebApp.createApp home
    let port,cts = Httpd.start app

    Browser.start port

    Console.ReadKey true |> ignore
    
    cts.Cancel()

    0