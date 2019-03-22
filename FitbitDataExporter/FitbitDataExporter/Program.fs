open FitbitDataExporter
open FitbitDataExporter.FitbitApi
open FSharp.Data
open System.IO

type Secret = JsonProvider<"""{"UserId": "user-id", "AccessToken": "token"}""">

let loadSecret (filePath : string) =
    use reader = new StreamReader(filePath)
    let secretText = reader.ReadToEnd()
    Secret.Parse(secretText)

[<EntryPoint>]
let main _ =
    let secret = loadSecret @".\Secret.json"
    let client = new FitbitClient(secret.UserId, secret.AccessToken)

    Exporter.exportAsync client |> Async.RunSynchronously

    0
