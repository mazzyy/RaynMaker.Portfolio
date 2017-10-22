
- setup a vue project with webback and vue loader to support vue components
  - install npm?
  - separate FE and BE
  - add build.cmd which updates modules and builds project
  - how to build a package? (copy stuff to bin -> webpack?)
- create a WPF app which hosts suave BE and VUE FE



- over all performance
  - TWR
  - MWR
  - benchmark with ETF



- vue navigation
- agents in BE to remember state


- show cash transactions and current cash as sanity check

- sanity checks
  - value is never negative - the event type defines whether it has to be substracted or summed up

- refactor core positions logic
  - move to dedicated interactor and make buy and sell methods public so that we can test individually

- re-read boostrap book to have better html (all these paragraphs ...)

- anual roi actually is no simple math average ...

- closePosition event would have to calculate the fee based on broker conditions

- fix anual fee in benchmark

- show negative numbers in red

- vue navigation?

- test and impl event invariants (e.g. cannot sell more than i have)

- refactor to EventSourcing and CQRS with agents

- automatically download prices from
  - http://www.google.com/finance/historical?q=MSFT&output=csv