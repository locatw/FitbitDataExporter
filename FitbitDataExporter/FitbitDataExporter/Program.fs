open FitbitDataExporter
open FitbitDataExporter.FitbitApi
open FitbitDataExporter.Logging
open FSharp.Data
open System.IO
open System

type Secret = JsonProvider<"""{"UserId": "user-id", "AccessToken": "token"}""">

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

[<EntryPoint>]
let main _ =
    configureNLog ()
    let logger = NLog.LogManager.GetCurrentClassLogger() :> NLog.ILogger

    let secretFilePath = @".\Secret.json"
    let loadSecretResult = loadSecret secretFilePath
    match loadSecretResult with
    | Result.Ok secret ->
        let client = new FitbitClient(secret.UserId, secret.AccessToken)
        let exportLogger = new DataExportLogger(logger)
        let exporter = new DataExporter(client, exportLogger)

        exporter.ExportAsync() |> Async.RunSynchronously

        0
    | Result.Error e ->
        match e with
        | :? FileNotFoundException as ex ->
            logger.Error(sprintf "File does not exist - %s" ex.FileName)
            1
        | _ as ex ->
            logger.Error(sprintf "Error - %s" ex.Message)
            9