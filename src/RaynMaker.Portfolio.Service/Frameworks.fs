module RaynMaker.Portfolio.Frameworks

module Web =
    open Suave
    open Suave.Successful
    open Suave.Operators
    open Newtonsoft.Json
    open Newtonsoft.Json.Serialization

    let JSON v =
        let jsonSerializerSettings = new JsonSerializerSettings()
        jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

        JsonConvert.SerializeObject(v, jsonSerializerSettings)
        |> OK
        >=> Writers.setMimeType "application/json; charset=utf-8"

module Httpd =
    open Suave
    open System.Net
    open System.Threading
    open Suave.Logging
    
    let start app =
        let port = 2525us
        let local = HttpBinding.create HTTP IPAddress.Loopback port
                
        let cts = new CancellationTokenSource()
        let config = { defaultConfig with bindings = [ local ]
                                          cancellationToken = cts.Token }

        let _, server = startWebServerAsync config app
    
        Async.Start(server, cts.Token)

        port |> int32, cts

 module Browser =
    open System.Diagnostics

    let start port = 
        Process.Start(sprintf "http://localhost:%i/" port) |> ignore

