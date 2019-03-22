open FSharp.Data
open System.IO

type Secret = JsonProvider<"""{"AccessToken": "token"}""">

let loadSecret (filePath : string) =
    use reader = new StreamReader(filePath)
    let secretText = reader.ReadToEnd()
    Secret.Parse(secretText)

[<EntryPoint>]
let main _ =
    let secret = loadSecret @".\Secret.json"
    let accessToken = secret.AccessToken

    printfn "%A" accessToken

    0
