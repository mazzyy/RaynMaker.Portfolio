

- create explict VM types

- move controller/presenter logic without framework dependencies to "portfolio lib"

- improve TestAPI and test to be based on Controller/Presenter
  - TestAPI only bridges between test and SUT
  - TDK provides convenience functions

- restructure features more towards end user features
  - remove "given-when-then" from scenario description

- fix red tests

- add missing tests

- find a way to automatically get the prices we need
  - add a service which collects in background and saves in store
  - e.g.: http://www.google.com/finance/historical?q=MSFT&output=csv

- add chart of "yield every month in percentage based on actual capital begin of month and end of month"
  - show the chart on "performance"
  - calc average of these yields
  - how to handle "disbursements"?

- buying price is not correctly calculated when some parts of the position got sold

=====================

- restructure backlog
  - road map?
  - issues?

- migrate "shell" to electron

- total return per year = (total return + 1)^(1/years)-1

- move "formatValue" into controller/presenter
  - but then sorting is difficult ...

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

- keep the position details
  - at least by and sell activities
  - would also be easier to "get a last price"

# References 

- http://tidyjava.com/clean-architecture-screaming/
- https://github.com/cleancoders/CleanCodeCaseStudy/tree/master/src/cleancoderscom

