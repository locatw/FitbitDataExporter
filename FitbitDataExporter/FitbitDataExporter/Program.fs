open FitbitDataExporter
open FitbitDataExporter.FitbitApi
open FitbitDataExporter.Logging
open FSharp.Data
open System.IO
open System

type Secret = JsonProvider<"""{"UserId": "user-id", "AccessToken": "token"}""">

type JsonFileWriter (outputDir : string) =
    let makeFilePath fileName =
        Path.Combine(outputDir, fileName)

    interface IJsonFileWriter with
        member __.WriteAsync(fileName : string, json : JsonValue) =
            async {
                let filePath = makeFilePath fileName
                use writer = new StreamWriter(filePath)
                json.WriteTo(writer, JsonSaveOptions.None)
            }

[<Literal>]
let secretFilePath = @".\Secret.json"

let loadSecret (filePath : string) =
    try
        use reader = new StreamReader(filePath)
        let secretText = reader.ReadToEnd()

        Result.Ok (Secret.Parse(secretText))
    with
    | _ as e -> Result.Error e

let configureNLog () =
    let now = DateTimeOffset.Now
    let config = new NLog.Config.LoggingConfiguration()

    // log format like: [2019/03/23 22:38:15.712] [INFO ] message
    let layout = new NLog.Layouts.SimpleLayout("[${date}] [${level:uppercase=true:padding=-5}] ${message}")

    let logFile = new NLog.Targets.FileTarget("logFile")
    let fileName = sprintf "export_%s.log" (now.ToString("yyyyMMddhhmmssfff"))
    logFile.FileName <- new NLog.Layouts.SimpleLayout(fileName)
    logFile.Layout <- layout

    let logConsole = new NLog.Targets.ConsoleTarget("logConsole")
    logConsole.Layout <- layout

    config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logFile)
    config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logConsole)

    NLog.LogManager.Configuration <- config

let createLogger () =
    configureNLog ()
    NLog.LogManager.GetCurrentClassLogger() :> NLog.ILogger

let run (logger : NLog.ILogger) (secret : Secret.Root) =
    let client = new FitbitClient(secret.UserId, secret.AccessToken)
    let exportLogger = new DataExportLogger(logger)
    let jsonFileWriter = new JsonFileWriter(".")
    let exporter = new DataExporter(client, exportLogger, jsonFileWriter)

    exporter.ExportAsync() |> Async.RunSynchronously

[<EntryPoint>]
let main _ =
    let logger = createLogger ()

    let result = loadSecret secretFilePath
    match result with
    | Result.Ok secret ->
        run logger secret
        0
    | Result.Error e ->
        match e with
        | :? FileNotFoundException as ex ->
            logger.Error(sprintf "File does not exist - %s" ex.FileName)
            1
        | _ as ex ->
            logger.Error(sprintf "Error - %s" ex.Message)
            9