namespace RaynMaker.Portfolio.Specs

open NUnit.Framework
open FsUnit
open RaynMaker.Portfolio.UseCases

[<TestFixture>]
module ``Given an EventStore`` =
    open FakeBroker

    [<Test>]
    let ``<When> initialized with empty list <Then> empty list is returned on Get``() =
        let store = EventStore.create(fun () -> [])

        store.Get() 
        |> should be Empty

    [<Test>]
    let ``<When> initialized with some events <Then> these events are returned on Get``() =
        let events =
            [
                at 2014 01 01 |> buy "Joe Inc" 10 10.0 |> toDomainEvent
                at 2015 01 01 |> buy "Joe Inc" 5 15.0 |> toDomainEvent
                at 2016 01 01 |> sell "Joe Inc" 15 20.0 |> toDomainEvent
            ]

        let store = EventStore.create(fun () -> events)

        store.Get() 
        |> should equal events

    [<Test>]
    let ``<When> events are added <Then> these events are returned on Get``() =
        let events =
            [
                at 2014 01 01 |> buy "Joe Inc" 10 10.0 |> toDomainEvent
                at 2015 01 01 |> buy "Joe Inc" 5 15.0 |> toDomainEvent
                at 2016 01 01 |> sell "Joe Inc" 15 20.0 |> toDomainEvent
            ]

        let store = EventStore.create(fun () -> events)
        at 2016 02 01 |> buy "Jack Inc" 100 10.0 |> toDomainEvent |> store.Post

        store.Get()
        |> Seq.last
        |> should equal (at 2016 02 01 |> buy "Jack Inc" 100 10.0 |> toDomainEvent)
