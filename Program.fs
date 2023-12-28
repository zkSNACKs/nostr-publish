// For more information see https://aka.ms/fsharp-console-apps
open System
open System.Net.WebSockets
open System.Threading
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Control
open NBitcoin.Secp256k1
open Nostra
open Nostra.Client

let expiresInOneMonth (event : Event.UnsignedEvent) =
    { event with Tags = ["expiration", [DateTime.Now.AddDays 30 |> Utils.toUnixTime |> string]] }

let publishNote note relay =
    let ws = new ClientWebSocket()
    let ctx = Communication.buildContext ws Console.Out  
    let pushToRelay = Monad.injectedWith ctx (Communication.sender ())

    async {
        use cts = new CancellationTokenSource(TimeSpan.FromSeconds 10)
        do! ws.ConnectAsync (relay, cts.Token) |> Async.AwaitTask
        pushToRelay (Request.CMEvent note)
    }

[<EntryPoint>]
let main args =
    let secret = args[0] |> Utils.fromHex |> ECPrivKey.Create 
    let msg = args[1]
    let unsignedNote = Event.createNoteEvent msg |> expiresInOneMonth
    let note = Event.sign secret unsignedNote
    
    [
        Uri "wss://relay.austrich.net"
        Uri "wss://nostr.cro.social"
        Uri "wss://relay.koreus.social"
        Uri "wss://nostr.massmux.com"
        Uri "wss://spore.ws"
        Uri "wss://nostr.btcmp.com"
        Uri "wss://relay.nostrified.org"
        Uri "wss://nostr.robotesc.ro"
        Uri "wss://relay.nostromo.social"
        Uri "wss://relay.nostrich.de"
        Uri "wss://nostr.l00p.org"
        Uri "wss://relay.stoner.com"
        Uri "wss://nostr.handyjunky.com"
        Uri "wss://relay.humanumest.social"
        Uri "wss://nostr-pub.wellorder.net"
        Uri "wss://relay.nostr.snblago.com"
        Uri "wss://nostr.web3infra.xyz"
        Uri "wss://nostr.mouton.dev"
        Uri "wss://relay.damus.io"
        Uri "wss://nostr.mutinywallet.com"
    ]
    |> List.map (publishNote note)
    |> List.map Async.Catch
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
    0
