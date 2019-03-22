module FitbitDataExporter.Exporter

open FitbitDataExporter.FitbitApi
open FSharp.Data
open System
open System.IO

let private makeOffset offsetFromUtcMillis =
    let mutable value = offsetFromUtcMillis

    let millis = value % 1000
    value <- (offsetFromUtcMillis - millis) / 1000

    let seconds = value % 60
    value <- (value - seconds) / 60

    let minutes = value % 60
    value <- (value - minutes) / 60

    let hours = value % 60

    new TimeSpan(hours, minutes, seconds)

let private writeToFile(filePath : string, json : JsonValue) =
    use writer = new StreamWriter(filePath)
    json.WriteTo(writer, JsonSaveOptions.None)

let private writeSleepLogsToFile (sleepLogs : DataModel.SleepLogs.SleepLog) (date : DateTimeOffset) =
    let localDate = date.LocalDateTime
    let filePath = sprintf @".\sleep_logs_%04d_%02d_%02d.json" localDate.Year localDate.Month localDate.Day
    writeToFile(filePath, sleepLogs.JsonValue)

let rec private exportSleepLogsAsync (client : FitbitClient) (date : DateTimeOffset) =
    async {
        let! sleepLogs = client.GetSleepLogsAsync(date)

        if sleepLogs.Sleep.Length <> 0 then
            writeSleepLogsToFile sleepLogs date

            do! Async.Sleep 1000

            return! exportSleepLogsAsync client (date.AddDays(-1.0))
        else
            return sleepLogs
    }

let exportAsync (client : FitbitClient) =
    async {
        let! profile = client.GetProfileAsync()
        let offset = makeOffset profile.User.OffsetFromUtcMillis

        return! exportSleepLogsAsync client (new DateTimeOffset(2019, 3, 22, 0, 0, 0, offset))
    }
    |> Async.Ignore
