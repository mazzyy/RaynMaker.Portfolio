namespace RaynMaker.Portfolio.UseCases

module TransactionsInteractor =
    open RaynMaker.Portfolio.Entities
    open System

    type EventVM = {
        date : string
        name : string
        }

    let private strDate (date:DateTime) =
        date.ToString("yyyy-MM-dd")

    let list store lastNth =
        let nth = lastNth |> Option.map(fun n -> (store |> List.length) - n) |> defaultArg <| 0
        store
        |> Seq.map (function
            | StockBought e -> { date = e.Date |> strDate; name = "Buy" }
            | StockSold e -> { date = e.Date |> strDate; name = "Sell" }
            | DividendReceived e -> { date = e.Date |> strDate; name = "Dividend" }
            | DepositAccounted e -> { date = e.Date |> strDate; name = "Deposit" }
            | SavingsPlanRateAccounted e -> { date = e.Date |> strDate; name = "Savings plan" }
            | DisbursementAccounted e -> { date = e.Date |> strDate; name = "Disbursement" }
            | InterestReceived e -> { date = e.Date |> strDate; name = "Interests" })
        |> Seq.skip nth
        |> List.ofSeq
