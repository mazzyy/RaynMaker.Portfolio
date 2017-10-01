namespace RaynMaker.Portfolio.UseCases

module PositionsInteractor =
    open RaynMaker.Portfolio.Entities
    open System

    type Position = {
        Open : DateTime 
        Close : DateTime option
        Isin : string
        Name : string
        MarketProfit : decimal<Currency>
        DividendProfit : decimal<Currency>
        MarketRoi : float<Percentage>
        DividendRoi : float<Percentage>
        MarketRoiAnual : float<Percentage>
        DividendRoiAnual : float<Percentage>
        }

    let listClosed store =
        //store
        //|> Seq.map (function
        //    | StockBought e -> { date = e.Date |> strDate; name = "Buy" }
        //    | StockSold e -> { date = e.Date |> strDate; name = "Sell" }
        //    | DividendReceived e -> { date = e.Date |> strDate; name = "Dividend" }
        //|> List.ofSeq
        [
            { 
                Open = new DateTime(2016,1,1)
                Close = new DateTime(2017,6,1) |> Some
                Isin = "DE1111111111"
                Name = "Noname AG"
                MarketProfit = 1200.54M<Currency>
                DividendProfit = 231.12M<Currency>
                MarketRoi = 12.00<Percentage>
                DividendRoi = 3.19<Percentage>
                MarketRoiAnual = (365.0 / (new DateTime(2017,6,1) -  DateTime(2016,1,1)).TotalDays) * 12.00<Percentage>
                DividendRoiAnual = (365.0 / (new DateTime(2017,6,1) -  DateTime(2016,1,1)).TotalDays) * 3.19<Percentage>
            }
        ]
