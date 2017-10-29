

- create a WPF app which hosts suave BE and VUE FE



- over all performance
  - TWR
  - MWR
  - benchmark with ETF


- add more tests
  - for the interactors
  - maybe also for the gateways? can we make those that simple that it is not required?


- show for "position closed" the date in the web page to know from when the prices are

- agents in BE to remember state

- show cash transactions and current cash as sanity check
- add overview on disposals per year

- sanity checks
  - value is never negative - the event type defines whether it has to be substracted or summed up

- refactor core positions logic
  - move to dedicated interactor and make buy and sell methods public so that we can test individually

- anual roi actually is no simple math average ...

- closePosition event would have to calculate the fee based on broker conditions

- fix anual fee in benchmark

- show negative numbers in red

- vue navigation?

- test and impl event invariants (e.g. cannot sell more than i have)

- refactor to EventSourcing and CQRS with agents

- automatically download prices from
  - http://www.google.com/finance/historical?q=MSFT&output=csv