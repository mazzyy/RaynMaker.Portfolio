namespace RaynMaker.Portfolio.Interactors

module BenchmarkInteractor =
    open System
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities

    type SavingsPlan = {
        Fee : decimal<Percentage>
        AnualFee : decimal<Percentage>
        Rate : decimal<Currency>
        }

    type ManualOrder = {
        Fee : decimal<Percentage>
        MinFee : decimal<Currency>
        MaxFee : decimal<Currency>
        }
    
    let getFee order value =
        match value |> percent order.Fee with
        | x when x < order.MinFee -> order.MinFee
        | x when x > order.MaxFee -> order.MaxFee
        | x -> x

    type Benchmark = {
        Isin : Isin
        Name : string
        SavingsPlan : SavingsPlan
        Manual : ManualOrder
        }

    let private pricePosition benchmark getPrice =
        { StockPriced.Date = DateTime.Today
          Name = benchmark.Name
          Isin = benchmark.Isin
          Price = DateTime.Today |> getPrice
          Fee = 10.0M<Currency> } 

    /// Based on the original buy & sell events new benchmarking events are generated which simulate 
    /// the performance one could have achived by buying the benchmark asset (e.g. an ETF) instead
    let buyBenchmarkInstead (benchmark:Benchmark) getPrice (store:DomainEvent list) =
        let getFee = getFee benchmark.Manual

        let buy day (value:decimal<Currency>) =
            let price = day |> getPrice
            let fee = value |> getFee
            let count = (value - fee) / price
            { StockBought.Isin = benchmark.Isin
              Name = benchmark.Name
              Date = day
              Fee = fee
              Price = price
              Count = count }

        let sell day (value:decimal<Currency>) =
            let price = day |> getPrice
            let fee = value |> getFee
            let count = value / price
            { StockSold.Isin = benchmark.Isin
              Name = benchmark.Name
              Date = day
              Fee = fee
              Price = price
              Count = count }
        
        seq {
            yield! store
                    |> Seq.choose (function
                        | StockBought e -> buy e.Date (e.Price * e.Count + e.Fee) |> StockBought |> Some
                        | StockSold e -> sell e.Date (e.Price * e.Count) |> StockSold |> Some
                        | _ -> None)
            yield pricePosition benchmark getPrice |> StockPriced
        }
        |> List.ofSeq

    /// Simulate buying a benchmark whenever a deposit was made considering the cash limit
    let buyBenchmarkByPlan (benchmark:Benchmark) getPrice (store:DomainEvent list) =

        let buy day  =
            let price = day |> getPrice
            let value = benchmark.SavingsPlan.Rate
            let fee = value |> percent benchmark.SavingsPlan.Fee
            let count = (value - fee) / price
            { StockBought.Isin = benchmark.Isin
              Name = benchmark.Name
              Date = day
              Fee = fee
              Price = price
              Count = count }

        // TODO: anual fee
        // -> what about just inventing an event because it could then be calculated when walking the positions
    
        let start = store.Head |> Events.GetDate
        let stop = DateTime.Today
        let numMonths = (stop.Year - start.Year) * 12 + (stop.Month - start.Month + 1)
        
        seq {
            yield!  [0 .. numMonths]
                    |> Seq.map(fun m -> (new DateTime(start.Year, start.Month, 1)).AddMonths(m))
                    |> Seq.map workingDay
                    |> Seq.map(fun day -> day |> buy |> StockBought)
            yield pricePosition benchmark getPrice |> StockPriced
        }
        |> List.ofSeq


