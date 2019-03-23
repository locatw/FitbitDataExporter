﻿namespace FitbitDataExporter

open FitbitDataExporter.FitbitApi
open FSharp.Data
open System

type IDataExportLogger =
    abstract member InfoSleepLogs : DateTimeOffset -> unit

type IJsonFileWriter =
    abstract member WriteAsync : string * JsonValue -> Async<unit>

type DataExporter(client : FitbitClient, logger : IDataExportLogger, jsonFileWriter : IJsonFileWriter) =
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

    let writeSleepLogsToFileAsync (sleepLogs : DataModel.SleepLogs.SleepLog) (date : DateTimeOffset) =
        async {
            let localDate = date.LocalDateTime
            let fileName = sprintf "sleep_logs_%04d_%02d_%02d.json" localDate.Year localDate.Month localDate.Day
            do! jsonFileWriter.WriteAsync(fileName, sleepLogs.JsonValue)
        }

    let rec exportSleepLogsAsync (date : DateTimeOffset) (endDate : DateTimeOffset) =
        async {
            if date < endDate then
                let! sleepLogs = client.GetSleepLogsAsync(date)
                do! writeSleepLogsToFileAsync sleepLogs date

                logger.InfoSleepLogs(date)

                do! Async.Sleep 1000

                return! exportSleepLogsAsync (date.AddDays(1.0)) endDate
            else
                return ()
        }

    member __.ExportAsync() =
        async {
            let! profile = client.GetProfileAsync()
            let offset = makeOffset profile.User.OffsetFromUtcMillis

            let now = DateTimeOffset.Now
            let startDate = new DateTimeOffset(profile.User.MemberSince, offset)
            let endDate = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, offset)

            return! exportSleepLogsAsync startDate endDate
        }
        |> Async.Ignore
