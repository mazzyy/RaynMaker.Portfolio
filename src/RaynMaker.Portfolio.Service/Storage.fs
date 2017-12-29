namespace RaynMaker.Portfolio.Gateways

module EventsReader =
    open System
    open FSharp.ExcelProvider
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities

    type private Sheet = ExcelFile<"../../etc/Events.xlsx">

    let readExcel path =
        let sheet = new Sheet(path)

        let error msg (r:Sheet.Row) = 
            let payload : obj list = [r.Date;r.ID; r.Name; r.Value; r.Fee; r.Count; r.Comment]
            sprintf "%s: Event=%s Payload=%A" msg r.Event payload

        let tryRead (r:Sheet.Row) =
            match r.Event with
            | EqualsI "StockBought" _ -> 
                { StockBought.Date = r.Date
                  Isin = r.ID |> Isin
                  Name = r.Name
                  Count = r.Count |> decimal
                  Price = (r.Value |> decimal) * 1.0M<Currency>
                  Fee = (r.Fee |> decimal) * 1.0M<Currency>} |> StockBought |> Some
            | EqualsI "StockSold" _ -> 
                { StockSold.Date = r.Date
                  Isin = r.ID |> Isin
                  Name = r.Name
                  Count = r.Count |> decimal
                  Price = (r.Value |> decimal) * 1.0M<Currency>
                  Fee = (r.Fee |> decimal) * 1.0M<Currency>} |> StockSold |> Some
            | EqualsI "DividendReceived" _ -> 
                { DividendReceived.Date = r.Date
                  Isin = r.ID |> Isin
                  Name = r.Name
                  Value = (r.Value |> decimal) * 1.0M<Currency>
                  Fee = (r.Fee |> decimal) * 1.0M<Currency>} |> DividendReceived |> Some
            | EqualsI "DepositAccounted" _ -> 
                { DepositAccounted.Date = r.Date
                  Value = (r.Value |> decimal) * 1.0M<Currency>} |> DepositAccounted |> Some
            | EqualsI "DisbursementAccounted" _ -> 
                { DisbursementAccounted.Date = r.Date
                  Value = (r.Value |> decimal) * 1.0M<Currency>} |> DisbursementAccounted |> Some
            | EqualsI "InterestReceived" _ -> 
                { InterestReceived.Date = r.Date
                  Value = (r.Value |> decimal) * 1.0M<Currency>} |> InterestReceived |> Some
            | EqualsI "PositionClosed" _
            | EqualsI "StockPriced" _ -> 
                { StockPriced.Date = if r.Date = DateTime.MinValue then DateTime.Today else r.Date
                  Isin = r.ID |> Isin
                  Name = r.Name
                  Price = (r.Value |> decimal) * 1.0M<Currency> } |> StockPriced |> Some
            | x -> None

        let tryParseEvent errors (r:Sheet.Row) =
            try
                match r |> tryRead with
                | Some e -> Some(e),errors
                | None -> None, (error "Unknown event type" r)::errors
            with 
                | ex -> None, (error "Failed to parse event" r)::errors

        let events, errors =  
            sheet.Data
            |> Seq.filter(fun r -> String.IsNullOrEmpty(r.Event) |> not)
            |> Seq.mapFold tryParseEvent []
        
        (events |> Seq.choose id |> List.ofSeq), errors

module HistoricalPricesReader =
    open System
    open FSharp.Data
    open RaynMaker.Portfolio.Entities

    type private Sheet = CsvProvider<"../../etc/FR0011079466.history.csv",";">
    
    let readCsv (path:string) =
        let sheet = Sheet.Load(path)
        sheet.Rows
        |> Seq.map(fun row -> 
            { Day = DateTime.Parse(row.Date)
              Value = row.Price * 1.0M<Currency> } )
        |> Seq.sortBy(fun x -> x.Day)
        |> List.ofSeq



