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

    let home = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location )

    let sayHello =
        request (fun r ->
            match r.queryParam "message" with
            | Choice1Of2 msg -> OK (sprintf "{ \"message\" : \"%s - pong\" }" msg)
            | Choice2Of2 msg -> RequestErrors.BAD_REQUEST msg)

    let app : WebPart =
        choose [ 
            GET >=> choose
                [
                    path "/" >=> Files.file "Content/index.html"
                    pathScan "/Content/%s" (fun f -> Files.file (sprintf "%s/Content/%s" home f))
                    pathScan "/Scripts/%s" (fun f -> Files.file (sprintf "%s/Scripts/%s" home f))
                    path "/api/hello" >=> sayHello
                ]
        ]

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