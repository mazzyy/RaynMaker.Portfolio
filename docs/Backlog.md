
- over all performance
  - TWR
  - MWR
  - benchmark with ETF

- refactor core positions logic
  - move to dedicated interactor and make buy and sell methods public so that we can test individually

- anual roi actually is no simple math average ...

- closePosition event would have to calculate the fee based on broker conditions

- fix anual fee in benchmark

- show negative numbers in red

- vue navigation?

- test and impl event invariants (e.g. cannot sell more than i have)

- report: pie chart of positions + cash compared to total investment capital

- refactor to EventSourcing and CQRS with agents

- automatically download prices from
  - http://www.google.com/finance/historical?q=MSFT&output=csv