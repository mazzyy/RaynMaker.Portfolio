﻿namespace RaynMaker.Portfolio.Gateways


module WebApp =
    open Suave
    open Suave.Successful
    open Suave.Operators
    open Suave.Filters
    open Newtonsoft.Json
    open Newtonsoft.Json.Serialization
    open RaynMaker.Portfolio.UseCases

    let JSON v =
        let jsonSerializerSettings = new JsonSerializerSettings()
        jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

        JsonConvert.SerializeObject(v, jsonSerializerSettings)
        |> OK
        >=> Writers.setMimeType "application/json; charset=utf-8"

    module private Handlers =
        //let closedPositions store (r:HttpRequest) =
        //    match r.queryParam "lastNth" with
        //    | Choice1Of2 msg -> msg |> int |> Some
        //    | Choice2Of2 msg -> None
        //    |> TransactionsInteractor.list store
        //    |> JSON
        let closedPositions store = store |> PositionsInteractor.listClosed |> JSON

    let createApp home store =
        choose [ 
            GET >=> choose
                [
                    path "/" >=> Files.file "Content/index.html"
                    pathScan "/Content/%s" (fun f -> Files.file (sprintf "%s/Content/%s" home f))
                    pathScan "/Scripts/%s" (fun f -> Files.file (sprintf "%s/Scripts/%s" home f))
                    //path "/api/closedPositions" >=> (request (Handlers.closedPositions store)) 
                    path "/api/closedPositions" >=> Handlers.closedPositions store
                ]
        ]

module ExcelEventStore =
    open System
    open FSharp.ExcelProvider
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities
    open RaynMaker.Portfolio.UseCases

    [<Literal>] 
    let private template = @"../../etc/Portfolio.Events.xlsx"

    type private EventsSheet = ExcelFile<template>

    type ParsedEvent = 
        | Event of DomainEvent
        | Unknown of string * DateTime * payload:obj list

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
            | x -> Unknown(r.Event,r.Date,[r.ID; r.Name; r.Value; r.Fee; r.Count; r.Comment])
        )
        |> List.ofSeq
