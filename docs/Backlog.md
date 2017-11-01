- closePosition event would have to calculate the fee based on broker conditions
  - remove fee property

-> move position to entities
-> move "buy" and "sell" and "close" to entities
   - have method working on one position
   - methods for collections to be checked - might be interactor still
   -> add tests
- positions as agent



- controllers are no interactors 
  - move as much out of controllers so that they become data transformators only



- over all performance
  - TWR
  - MWR

- add more tests
  - for the interactors
  - gateways
    - ExcelStoreReader: success and errors
	- HistoricalPrices




- do we want to introduce an entity called stock? 
  (or stock id?)

- should we remove StockPriced event and translate it into a "get info from service" activity?
  - like with historical prices
  - maybe we first need to read more about event sourcing?

- keep the position details
  - at least by and sell activities
  - would also be easier to "get a last price"

- money accounting
  - show cash transactions and current cash as sanity check
    (similar to account report)
  - add overview on disposals per year


- sanity checks
  - value is never negative - the event type defines whether it has to be substracted or summed up

- refactor core positions logic
  - move to dedicated interactor and make buy and sell methods public so that we can test individually

- anual roi actually is no simple math average ...

- fix anual fee in benchmark

- show negative numbers in red

- test and impl event invariants (e.g. cannot sell more than i have)

- refactor to EventSourcing and CQRS with agents

- automatically download prices from
  - http://www.google.com/finance/historical?q=MSFT&output=csv


# References 

- http://tidyjava.com/clean-architecture-screaming/
- https://github.com/cleancoders/CleanCodeCaseStudy/tree/master/src/cleancoderscom

