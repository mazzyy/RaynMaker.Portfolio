namespace RaynMaker.Portfolio.Service

module Httpd =
    open Suave
    open Suave.ServerErrors
    open System.Net
    open System.Threading
    open System
    
    let start errorHandler app =
        let port = 2525us
        let local = HttpBinding.create HTTP IPAddress.Loopback port
                
        let cts = new CancellationTokenSource()
        let customErrorHandler ex msg ctx =
            errorHandler msg ex
            INTERNAL_ERROR (sprintf "Failed to process request: %s%s%A" msg Environment.NewLine ex) ctx

        let config = { defaultConfig with 
                                        bindings = [ local ]
                                        cancellationToken = cts.Token 
                                        errorHandler = customErrorHandler
                     }

        let _, server = startWebServerAsync config app
    
        Async.Start(server, cts.Token)

        port |> int32, cts

