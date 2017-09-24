namespace RaynMaker.Portfolio.Gateways


module WebApp =
    open Suave
    open Suave.Successful
    open Suave.Operators
    open Suave.Filters

    module private Handlers =
        open RaynMaker.Portfolio.UseCases

        let sayHello =
            request (fun r ->
                match r.queryParam "message" with
                | Choice1Of2 msg -> 
                    let response = SayHelloInteractor.hello msg
                    OK (sprintf "{ \"message\" : \"%s\" }" response )
                | Choice2Of2 msg -> RequestErrors.BAD_REQUEST msg)

    let createApp home =
        choose [ 
            GET >=> choose
                [
                    path "/" >=> Files.file "Content/index.html"
                    pathScan "/Content/%s" (fun f -> Files.file (sprintf "%s/Content/%s" home f))
                    pathScan "/Scripts/%s" (fun f -> Files.file (sprintf "%s/Scripts/%s" home f))
                    path "/api/hello" >=> Handlers.sayHello
                ]
        ]


