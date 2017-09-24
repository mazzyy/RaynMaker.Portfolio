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

module EventStore =
    open System
    open FSharp.ExcelProvider
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities

    [<Literal>] 
    let private template = @"../../etc/Portfolio.Events.xlsx"

    type private EventsSheet = ExcelFile<template>

    type ParsedEvent = 
        | Event of DomainEvent
        | Unknown of string * DateTime

    let load path =
        let sheet = new EventsSheet(path)

        sheet.Data
        |> Seq.map(fun r -> 
            match r.Event with
            | EqualsI "StockBought" _ -> 
                { StockBought.Date = r.Date
                  Isin = r.ID
                  Name = r.Name
                  Count = r.Count |> int
                  Price = r.Value
                  Fee = r.Fee } |> StockBought |> Event
            | x -> Unknown(r.Event,r.Date)
        )
        |> List.ofSeq
