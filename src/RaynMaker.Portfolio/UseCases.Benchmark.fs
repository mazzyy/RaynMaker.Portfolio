namespace RaynMaker.Portfolio.UseCases

module BenchmarkInteractor =
    open System
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities
    open PositionsInteractor

    type SavingsPlan = {
        Fee : decimal<Percentage>
        AnnualFee : decimal<Percentage>
        Rate : decimal<Currency> }
    
    type Benchmark = {
        Isin : AssetId
        Name : string }

    let private pricePosition benchmark getPrice day =
        day
        |> getPrice
        |> Option.map(fun price ->
            { Date = day
              Name = benchmark.Name
              AssetId = benchmark.Isin
              Price = price } )

    /// Based on the original buy & sell events new benchmarking events are generated which simulate 
    /// the performance one could have achieved by buying the benchmark asset (e.g. an ETF) instead
    let buyBenchmarkInstead broker (benchmark:Benchmark) getPrice (store:DomainEvent list) =
        let buy day (value:decimal<Currency>) =
            day 
            |> getPrice
            |> Option.map(fun price ->
                let fee = value |> Broker.getFee broker
                let count = (value - fee) / price
                { AssetId = benchmark.Isin
                  Name = benchmark.Name
                  Date = day
                  Fee = fee
                  Price = price
                  Count = count })

        let sell day (value:decimal<Currency>) =
            day 
            |> getPrice
            |> Option.map(fun price ->
                let fee = value |> Broker.getFee broker
                let count = value / price
                { AssetId = benchmark.Isin
                  Name = benchmark.Name
                  Date = day
                  Fee = fee
                  Price = price
                  Count = count })
        
        let lastDay = store |> List.last |> DomainEvent.Date
        seq {
            yield! store
                    |> Seq.choose (function
                        | StockBought e -> buy e.Date (e.Price * e.Count + e.Fee) |> Option.map StockBought 
                        | StockSold e -> sell e.Date (e.Price * e.Count) |> Option.map StockSold
                        | _ -> None)
            let lastEvent = lastDay |> pricePosition benchmark getPrice |> Option.map StockPriced
            if lastEvent |> Option.isSome then
                yield lastEvent |> Option.get
        }
        |> List.ofSeq

    /// Simulate buying a benchmark whenever a deposit was made considering the cash limit
    let buyBenchmarkByPlan savingsPlan (benchmark:Benchmark) getPrice (start:DateTime) (stop:DateTime) =
        let buy day  =
            day 
            |> getPrice
            |> Option.map(fun price ->
                let value = savingsPlan.Rate
                let fee = value |> percent savingsPlan.Fee
                let count = (value - fee) / price
                { AssetId = benchmark.Isin
                  Name = benchmark.Name
                  Date = day
                  Fee = fee
                  Price = price
                  Count = count })

        // TODO: annual fee
        // -> what about just inventing an event because it could then be calculated when walking the positions
    
        let numMonths = (stop.Year - start.Year) * 12 + (stop.Month - start.Month)
        
        seq {
            yield!  [0 .. numMonths]
                    |> Seq.map(fun m -> (new DateTime(start.Year, start.Month, 1)).AddMonths(m))
                    |> Seq.choose(fun day -> day |> buy |> Option.map StockBought)
            let lastEvent = stop |> pricePosition benchmark getPrice |> Option.map StockPriced
            if lastEvent |> Option.isSome then
                yield lastEvent |> Option.get
        }
        |> List.ofSeq

    type Performance = {
        BuyInstead : ProfitEvaluation
        BuyPlan : ProfitEvaluation }
    
    let getBenchmarkPerformance (store:EventStore.Api) broker savingsPlan (historicalPrices:PricesRepository.Api) (benchmark:Benchmark) = 
        let getPrice day = 
            let history = benchmark.Isin |> historicalPrices.Get
            Prices.getPrice 30.0 history day

        let eval events = 
            events
            |> Positions.create
            |> PositionsInteractor.evaluateProfit broker (DomainEvent.LastPriceOf events)
            |> Seq.head

        let events = store.Get()

        {
            BuyInstead = events |> buyBenchmarkInstead broker benchmark getPrice |> eval
            BuyPlan = 
                let start = events.Head |> DomainEvent.Date
                let stop = events |> List.last |> DomainEvent.Date
                buyBenchmarkByPlan savingsPlan benchmark getPrice start stop |> eval
        }