namespace RaynMaker.Portfolio.Gateways


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
        open System
        open RaynMaker.Portfolio

        let (=>) k v = k,v |> box
        let formatDate (date:DateTime) = date.ToString("yyyy-MM-dd")
        let formatTimespan (span:TimeSpan) = 
            match span.TotalDays with
            | x when x > 365.0 -> sprintf "%.2f years" (span.TotalDays / 365.0)
            | x when x > 90.0 -> sprintf "%.2f months" (span.TotalDays / 30.0)
            | x -> sprintf "%.0f days" span.TotalDays
        let formatMoney = sprintf "%.2f"
        let formatPercentage = sprintf "%.2f"

        let closedPositions store = 
            store 
            |> PositionsInteractor.listClosed
            |> List.map(fun p ->
                dict [
                    "name" => p.Name
                    "isin" => p.Isin
                    "open" => (p.Open |> formatDate)
                    "close" => (p.Close |> Option.map formatDate |? "-")
                    "duration" => (p.Close |> Option.map(fun c -> (c - p.Open) |> formatTimespan) |? "-")
                    "marketProfit" => (p.MarketProfit |> formatMoney)
                    "dividendProfit" => (p.DividendProfit |> formatMoney)
                    "totalProfit" => (p.MarketProfit + p.DividendProfit |> formatMoney)
                    "marketRoi" => (p.MarketRoi |> formatPercentage)
                    "dividendRoi" => (p.DividendRoi |> formatPercentage)
                    "totalRoi" => (p.MarketRoi + p.DividendRoi |> formatPercentage)
                    "marketRoiAnual" => (p.MarketRoiAnual |> formatPercentage)
                    "dividendRoiAnual" => (p.DividendRoiAnual |> formatPercentage)
                    "totalRoiAnual" => (p.MarketRoiAnual + p.DividendRoiAnual |> formatPercentage)
                ])
            |> JSON

    let createApp home store =
        choose [ 
            GET >=> choose
                [
                    path "/" >=> Files.file "Content/index.html"
                    pathScan "/Content/%s" (fun f -> Files.file (sprintf "%s/Content/%s" home f))
                    pathScan "/Scripts/%s" (fun f -> Files.file (sprintf "%s/Scripts/%s" home f))
                    path "/api/closedPositions" >=> Handlers.closedPositions store
                ]
        ]

module ExcelEventStore =
    open System
    open FSharp.ExcelProvider
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities

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
                  Price = (r.Value |> decimal) * 1.0M<Currency>
                  Fee = (r.Fee |> decimal) * 1.0M<Currency>} |> StockBought |> Event
            | x -> Unknown(r.Event,r.Date,[r.ID; r.Name; r.Value; r.Fee; r.Count; r.Comment])
        )
        |> List.ofSeq
