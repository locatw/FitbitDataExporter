namespace FitbitDataExporter

open FitbitDataExporter.FitbitApi
open FSharp.Data
open System

type IDataExportLogger =
    abstract member InfoHeartRateIntradayTimeSeries : DateTimeOffset -> unit

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

    let writeDataToFileAsync (dataKind : DataModel.DataKind) (json : JsonValue) (date : DateTimeOffset) =
        async {
            let dataKindString =
                match dataKind with
                | DataModel.DataKind.HeartRateIntradayTimeSeries -> "heart_rate_intraday_time_series"
                | DataModel.DataKind.SleepLogs -> "sleep_logs"
                | _ -> failwith (sprintf "Unknown data kind: %s" (dataKind.ToString()))
            let localDate = date.LocalDateTime
            let fileName = sprintf "%s_%04d_%02d_%02d.json" dataKindString localDate.Year localDate.Month localDate.Day
            do! jsonFileWriter.WriteAsync(fileName, json)
        }

    let writeHeartRateIntradayTimeSeriesToFileAsync (heartRateIntradayTimeSeries : DataModel.HeartRateIntradayTimeSeries.Root) (date : DateTimeOffset) =
        writeDataToFileAsync DataModel.DataKind.HeartRateIntradayTimeSeries heartRateIntradayTimeSeries.JsonValue date

    let writeSleepLogsToFileAsync (sleepLogs : DataModel.SleepLogs.SleepLog) (date : DateTimeOffset) =
        writeDataToFileAsync DataModel.DataKind.SleepLogs sleepLogs.JsonValue date

    let rec exportAllDayDataAsync(exportOneDayData : DateTimeOffset -> Async<unit>) (date : DateTimeOffset) (endDate : DateTimeOffset) =
        async {
            if date < endDate then
                do! exportOneDayData date
                do! Async.Sleep 1000
                return! exportAllDayDataAsync exportOneDayData (date.AddDays(1.0)) endDate
            else
                return ()
        }

    let exportHeartRateTimeSeriesAsync =
        let export =
            fun date ->
                async {
                    let! heartRateIntradayTimeSeries = client.GetHeartRateIntradayTimeSeriesAsync(date)
                    do! writeHeartRateIntradayTimeSeriesToFileAsync heartRateIntradayTimeSeries date

                    logger.InfoHeartRateIntradayTimeSeries(date)
                }
        exportAllDayDataAsync export

    let exportSleepLogsAsync =
        let export =
            fun date ->
                async {
                    let! sleepLogs = client.GetSleepLogsAsync(date)
                    do! writeSleepLogsToFileAsync sleepLogs date

                    logger.InfoSleepLogs(date)
                }
        exportAllDayDataAsync export

    member __.ExportAsync() =
        async {
            let! profile = client.GetProfileAsync()
            let offset = makeOffset profile.User.OffsetFromUtcMillis

            let now = DateTimeOffset.Now
            let startDate = new DateTimeOffset(profile.User.MemberSince, offset)
            let endDate = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, offset)

            do! exportHeartRateTimeSeriesAsync startDate endDate
            do! exportSleepLogsAsync startDate endDate
            return ()
        }
        |> Async.Ignore
