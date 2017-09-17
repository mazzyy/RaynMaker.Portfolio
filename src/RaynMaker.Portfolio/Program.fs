module RaynMaker.Portfolio.Main

open Suave
open Suave.Successful
open Suave.Operators
open Suave.Filters
open System.Net
open System.Threading
open System
open System.Diagnostics
open System.Reflection
open System.IO


[<EntryPoint>]
let main argv = 
    printfn "Starting ..."

    let app : WebPart =
        choose [ 
            //path "/" >=> OK (home ()) 
            path "/" >=> Redirection.redirect "/index.html"
            Files.browseHome
        ]

    let home = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), "html" )
    let local = HttpBinding.create HTTP IPAddress.Loopback 2525us
                
    let cts = new CancellationTokenSource()
    let config = { defaultConfig with bindings = [ local ]
                                      homeFolder = home |> Some
                                      cancellationToken = cts.Token }

    let _, server = startWebServerAsync config app
    
    Async.Start(server, cts.Token)

    Process.Start("http://localhost:2525") |> ignore
    
    
    Console.ReadKey true |> ignore
    
    cts.Cancel()


    0