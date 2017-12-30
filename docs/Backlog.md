
- benchmark
  - anual fee not considered
    // TODO: anual fee
    // -> what about just inventing an event because it could then be calculated when walking the positions
  - how do we hande cash in general in the system?


- over all performance
  - current implementation is too naive (we cannot just substract disbursement - we could have invested it in between ...)
  - TWR
  - MWR

- add tests for performance calculation

=== > 1.0 ==================

- njsproj: npm install not running automatically with plainion.ci ...

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

- refactor core positions logic
  - move to dedicated interactor and make buy and sell methods public so that we can test individually

- anual roi actually is no simple math average ...

- show negative numbers in red

- refactor to EventSourcing and CQRS with agents

- automatically download prices from
  - http://www.google.com/finance/historical?q=MSFT&output=csv


# References 

- http://tidyjava.com/clean-architecture-screaming/
- https://github.com/cleancoders/CleanCodeCaseStudy/tree/master/src/cleancoderscom

