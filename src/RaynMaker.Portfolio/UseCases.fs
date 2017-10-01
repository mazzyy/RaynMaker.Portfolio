namespace RaynMaker.Portfolio.UseCases

module PositionsInteractor =
    open RaynMaker.Portfolio.Entities
    open System

    type EventVM = {
        date : string
        name : string
        }

    let private strDate (date:DateTime) =
        date.ToString("yyyy-MM-dd")

    let listClosed store =
        store
        |> Seq.map (function
            | StockBought e -> { date = e.Date |> strDate; name = "Buy" }
            | StockSold e -> { date = e.Date |> strDate; name = "Sell" }
            | DividendReceived e -> { date = e.Date |> strDate; name = "Dividend" }
            | DepositAccounted e -> { date = e.Date |> strDate; name = "Deposit" }
            | SavingsPlanRateAccounted e -> { date = e.Date |> strDate; name = "Savings plan" }
            | DisbursementAccounted e -> { date = e.Date |> strDate; name = "Disbursement" }
            | InterestReceived e -> { date = e.Date |> strDate; name = "Interests" })
        |> List.ofSeq
