module FitbitDataExporter.FitbitApi.DataModel

open FSharp.Data

[<Literal>]
let private HeartRateIntradayTimeSeriesSample = """
{
  "activities-heart": [
    {
      "dateTime": "2019-01-01",
      "value": {
        "customHeartRateZones": [],
        "heartRateZones": [
          {
            "caloriesOut": 1000.1234,
            "max": 100,
            "min": 10,
            "minutes": 1000,
            "name": "Out of Range"
          }
        ],
        "restingHeartRate": 10
      }
    }
  ],
  "activities-heart-intraday": {
    "dataset": [
      {
        "time": "00:00:00",
        "value": 10
      }
    ],
    "datasetInterval": 1,
    "datasetType": "minute"
  }
}
"""

[<Literal>]
let private SleepLogsSample = """
{
  "sleep": [
    {
      "dateOfSleep": "2019-01-01",
      "duration": 10000000,
      "efficiency": 100,
      "endTime": "2019-01-01T00:00:00.000",
      "infoCode": 0,
      "isMainSleep": true,
      "levels": {
        "data": [
          {
            "dateTime": "2019-01-01T00:00:00.000",
            "level": "wake",
            "seconds": 100
          }
        ],
        "shortData": [
          {
            "dateTime": "2019-01-01T00:00:00.000",
            "level": "wake",
            "seconds": 100
          }
        ],
        "summary": {
          "deep": {
            "count": 1,
            "minutes": 1,
            "thirtyDayAvgMinutes": 1
          },
          "light": {
            "count": 1,
            "minutes": 1,
            "thirtyDayAvgMinutes": 1
          },
          "rem": {
            "count": 1,
            "minutes": 1,
            "thirtyDayAvgMinutes": 1
          },
          "wake": {
            "count": 1,
            "minutes": 1,
            "thirtyDayAvgMinutes": 1
          }
        }
      },
      "logId": 10000000000,
      "minutesAfterWakeup": 0,
      "minutesAsleep": 1,
      "minutesAwake": 1,
      "minutesToFallAsleep": 0,
      "startTime": "2019-01-01T00:00:00.000",
      "timeInBed": 1,
      "type": "stages"
    }
  ],
  "summary": {
    "stages": {
      "deep": 1,
      "light": 1,
      "rem": 1,
      "wake": 1
    },
    "totalMinutesAsleep": 1,
    "totalSleepRecords": 1,
    "totalTimeInBed": 1
  }
}
"""

[<Literal>]
let private ProfileSample = """
{
  "user": {
    "age": 1,
    "ambassador": false,
    "avatar": "https://static0.fitbit.com/images/profile/image.png",
    "avatar150": "https://static0.fitbit.com/images/profile/image.png",
    "avatar640": "https://static0.fitbit.com/images/profile/image.png",
    "averageDailySteps": 1000,
    "clockTimeDisplayFormat": "24hour",
    "corporate": false,
    "corporateAdmin": false,
    "dateOfBirth": "2018-01-01",
    "displayName": "name",
    "displayNameSetting": "name",
    "distanceUnit": "METRIC",
    "encodedId": "abcdef",
    "familyGuidanceEnabled": false,
    "features": { "exerciseGoal": true },
    "foodsLocale": "ja_JP",
    "fullName": "fullname",
    "gender": "MALE",
    "glucoseUnit": "METRIC",
    "height": 100.0,
    "heightUnit": "METRIC",
    "isChild": false,
    "locale": "ja_JP",
    "memberSince": "2019-01-01",
    "mfaEnabled": false,
    "offsetFromUTCMillis": 32400000,
    "startDayOfWeek": "SUNDAY",
    "strideLengthRunning": 100.0,
    "strideLengthRunningType": "default",
    "strideLengthWalking": 10.0,
    "strideLengthWalkingType": "default",
    "swimUnit": "METRIC",
    "timezone": "Asia/Tokyo",
    "topBadges": [
      {
        "badgeGradientEndColor": "000000",
        "badgeGradientStartColor": "000000",
        "badgeType": "badge type",
        "category": "category",
        "cheers": [],
        "dateTime": "2019-01-01",
        "description": "description",
        "earnedMessage": "earned message",
        "encodedId": "abcdef",
        "image100px": "https://static0.fitbit.com/images/badges_new/100px/image.png",
        "image125px": "https://static0.fitbit.com/images/badges_new/125px/image.png",
        "image300px": "https://static0.fitbit.com/images/badges_new/300px/image.png",
        "image50px": "https://static0.fitbit.com/images/badges_new/image.png",
        "image75px": "https://static0.fitbit.com/images/badges_new/75px/image.png",
        "marketingDescription": "marketing description",
        "mobileDescription": "mobile description",
        "name": "name",
        "shareImage640px": "https://static0.fitbit.com/images/badges_new/386px/shareLocalized/ja_JP/image.png",
        "shareText": "share text",
        "shortDescription": "short description",
        "shortName": "short name",
        "timesAchieved": 1,
        "value": 1
      }
    ],
    "waterUnit": "METRIC",
    "waterUnitName": "ml",
    "weight": 10,
    "weightUnit": "METRIC"
  }
}
"""

type DataKind =
| HeartRateIntradayTimeSeries = 0
| SleepLogs = 1

type HeartRateIntradayTimeSeries = JsonProvider<HeartRateIntradayTimeSeriesSample>

type SleepLogs = JsonProvider<SleepLogsSample, RootName="SleepLogs">

type Profile = JsonProvider<ProfileSample, RootName="Profile">