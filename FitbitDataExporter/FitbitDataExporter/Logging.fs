namespace FitbitDataExporter.Logging

open FitbitDataExporter
open System

type DataExportLogger(logger : NLog.ILogger) =
    let info (dataKind : string) (date : DateTimeOffset) =
        let message = sprintf "Export %s of %s" dataKind (date.ToString("yyyy/MM/dd"))
        logger.Info(message)

    interface IDataExportLogger with
        member __.InfoHeartRateIntradayTimeSeries(date : DateTimeOffset) =
            info "heart rate intraday time series" date

        member __.InfoSleepLogs(date : DateTimeOffset) =
            info "sleep log" date
