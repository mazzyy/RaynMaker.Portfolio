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


