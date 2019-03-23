namespace FitbitDataExporter

open FitbitDataExporter.FitbitApi
open FSharp.Data
open System
open System.IO

type DataExporter(client : FitbitClient) =
    let makeOffset offsetFromUtcMillis =
        let mutable value = offsetFromUtcMillis

        let millis = value % 1000
        value <- (offsetFromUtcMillis - millis) / 1000

        let seconds = value % 60
        value <- (value - seconds) / 60

        let minutes = value % 60
        value <- (value - minutes) / 60

        let hours = value % 60

        new TimeSpan(hours, minutes, seconds)

    let writeToFile(filePath : string, json : JsonValue) =
        use writer = new StreamWriter(filePath)
        json.WriteTo(writer, JsonSaveOptions.None)

    let writeSleepLogsToFile (sleepLogs : DataModel.SleepLogs.SleepLog) (date : DateTimeOffset) =
        let localDate = date.LocalDateTime
        let filePath = sprintf @".\sleep_logs_%04d_%02d_%02d.json" localDate.Year localDate.Month localDate.Day
        writeToFile(filePath, sleepLogs.JsonValue)

    let rec exportSleepLogsAsync (date : DateTimeOffset) =
        async {
            let! sleepLogs = client.GetSleepLogsAsync(date)

            if sleepLogs.Sleep.Length <> 0 then
                writeSleepLogsToFile sleepLogs date

                do! Async.Sleep 1000

                return! exportSleepLogsAsync (date.AddDays(-1.0))
            else
                return sleepLogs
        }

    member __.ExportAsync() =
        async {
            let! profile = client.GetProfileAsync()
            let offset = makeOffset profile.User.OffsetFromUtcMillis

            return! exportSleepLogsAsync (new DateTimeOffset(2019, 3, 22, 0, 0, 0, offset))
        }
        |> Async.Ignore
