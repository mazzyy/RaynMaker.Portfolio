namespace RaynMaker.Portfolio.UseCases

module TransactionsInteractor =

    type EventVM = {
        date : string
        name : string
        }

    let list store lastNth =
        [
            { date = "2017-01-01"; name = "stock bought" }
            { date = "2017-03-03"; name = "stock sold" }
        ]