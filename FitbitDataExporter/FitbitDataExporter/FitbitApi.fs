namespace FitbitDataExporter.FitbitApi

open System
open System.Net.Http

type FitbitClient(userId : string, accessToken : string) =
    let apiUrl = "https://api.fitbit.com"
    let version = "1.2"

    let client = new HttpClient()

    let makeRequest (httpMethod : HttpMethod) (url : string) =
        let request = new HttpRequestMessage(httpMethod, url)
        let authValue = sprintf "Bearer %s" accessToken
        request.Headers.Add("Authorization", authValue)
        request

    let formatDateInLocal (date : DateTimeOffset) =
        let localDate = date.LocalDateTime
        sprintf "%04d-%02d-%02d" localDate.Year localDate.Month localDate.Day

    member __.GetSleepLogsAsync(date : DateTimeOffset) =
        async {
            let dateValue = formatDateInLocal date
            let url = sprintf "%s/%s/user/%s/sleep/date/%s.json" apiUrl version userId dateValue
            let request = makeRequest HttpMethod.Get url
            let! response = client.SendAsync(request) |> Async.AwaitTask
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

            return DataModel.SleepLogs.Parse(content)
        }

    member __.GetProfileAsync() =
        async {
            let url = sprintf "%s/%s/user/%s/profile.json" apiUrl version userId
            let request = makeRequest HttpMethod.Get url
            let! response = client.SendAsync(request) |> Async.AwaitTask
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

            return DataModel.Profile.Parse(content)
        }
