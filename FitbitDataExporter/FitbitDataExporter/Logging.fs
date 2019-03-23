namespace FitbitDataExporter.Logging

open FitbitDataExporter
open System

type DataExportLogger(logger : NLog.ILogger) =
    interface IDataExportLogger with
        member __.InfoSleepLogs(date : DateTimeOffset) =
            let message = sprintf "Export sleep log of %s" (date.ToString("yyyy/MM/dd"))
            logger.Info(message)