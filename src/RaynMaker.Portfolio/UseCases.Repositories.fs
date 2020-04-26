namespace RaynMaker.Portfolio.UseCases

module EventStore =
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities

    type private Msg = 
        | Init of (unit -> DomainEvent list)
        | Post of DomainEvent
        | Get of AsyncReplyChannel<DomainEvent list>
        | Stop 

    type Api = {
        Post: DomainEvent -> unit
        Get: unit -> DomainEvent list
        Stop: unit -> unit
    }

    let create handleLastChanceException init =
        let agent = Agent<Msg>.Start(fun inbox ->
            let rec loop store =
                async {
                    let! msg = inbox.Receive()

                    match msg with
                    | Init f -> return! loop (f())
                    | Post evt -> return! loop (evt::store |> List.rev)
                    | Get replyChannel -> 
                        replyChannel.Reply store
                        return! loop store
                    | Stop -> return ()
                }
            loop [] ) 

        agent.Error.Add(fun ex -> handleLastChanceException "Loading events failed" ex)
        
        agent.Post(init |> Init)

        { Post = fun evt -> agent.Post(evt |> Post)
          Get = fun () -> agent.PostAndReply( fun replyChannel -> replyChannel |> Get)
          Stop = fun () -> agent.Post Stop }


module PricesRepository =
    open RaynMaker.Portfolio
    open RaynMaker.Portfolio.Entities

    type private Msg = 
        | Get of Isin * AsyncReplyChannel<Price list>
        | Stop 

    type Api = {
        Get: Isin -> Price list
        Stop: unit -> unit
    }

    let create handleLastChanceException fetch read =
        let agent = Agent<Msg>.Start(fun inbox ->
            let rec loop store =
                async {
                    let! msg = inbox.Receive()

                    match msg with
                    | Get(isin, replyChannel) -> 
                        let newStore, prices = 
                            match store |> Map.tryFind isin with
                            | Some prices -> store, prices
                            | None -> let prices = read isin
                                      (store |> Map.add isin prices), prices

                        replyChannel.Reply prices

                        return! loop newStore
                    | Stop -> return ()
                }
            loop Map.empty ) 

        agent.Error.Add(fun ex -> handleLastChanceException "Loading prices failed" ex)
        
        let collector = Agent<Msg>.Start(fun inbox ->
            let rec loop () = async {
                //do! Async.Sleep(1000)
                //return! loop ()
                fetch()
                return ()
            }
            loop () ) 

        collector.Error.Add(fun ex -> handleLastChanceException "Collecting new prices failed" ex)

        { Get = fun isin -> agent.PostAndReply( fun replyChannel -> (isin,replyChannel) |> Get)
          Stop = fun () -> agent.Post Stop }
