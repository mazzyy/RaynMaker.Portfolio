
- do not crash if error in excel events - show helpful error message

- remove "given-when-then" from scenario description

- restructure features more towards end user features
  - restructure TestAPI accordingly

- migrate TestAPI to Controller basis
  - move "presenter" and "controller" independent from frameworks ot "raynmaker.portfolio" lib
  - create explict ViewModel types to have more safety - nothing forgotten

- add chart of "yield every month in percentage based on actual capital begin of month and end of month"
  - show the chart on "performance"
  - calc average of these yields
  - how to handle "disbursements"?

- find a way to automatically get the prices we need
  - add a service which collects in background and saves in store

- restructure backlog
  - road map?
  - issues?

=====================

- buying price is not correctly calculated when some parts of the position got sold
- errors from Agents cannot be shown as message box in Shell
  - anyhow we should maybe have a "log window" instead with error, warning and info logs
  - or even better: have it in the web page itself
- we definitively need much more tests!

- migrate "shell" to electron

- total return per year = (total return + 1)^(1/years)-1


- move "formatValue" into controller/presenter
  - but then sorting is difficult ...

document in BDD style
- vision statement
- business goals
- stakeholders
- features
- examples/scenarios

- create a website and clearly document how to use it

- separate user and project doc

=====================

- for router link highlighting see vue cookbook page 292

- benchmark
  - annual fee not considered
    // TODO: annual fee
    // -> what about just inventing an event because it could then be calculated when walking the positions
  - how do we hande cash in general in the system?


- over all performance
  - current implementation is too naive (we cannot just substract disbursement - we could have invested it in between ...)
  - TWR
  - MWR

- add tests for performance calculation

=====================

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

- annual roi actually is no simple math average ...

- show negative numbers in red

- refactor to EventSourcing and CQRS with agents

- automatically download prices from
  - http://www.google.com/finance/historical?q=MSFT&output=csv


# References 

- http://tidyjava.com/clean-architecture-screaming/
- https://github.com/cleancoders/CleanCodeCaseStudy/tree/master/src/cleancoderscom

