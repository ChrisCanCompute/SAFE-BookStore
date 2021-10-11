/// Functions for managing the database.
module ServerCode.Database

open ServerCode.Storage.AzureTable
open ServerCode.Storage.Postgres
open ServerCode
open System.Threading.Tasks
open FSharp.Control.Tasks.ContextInsensitive
open Npgsql

[<RequireQualifiedAccess>]
type DatabaseType =
    | FileSystem
    | AzureStorage of connectionString : AzureConnection
    | Postgres of configuration : string

type IDatabaseFunctions =
    abstract member LoadWishList : string -> Task<Domain.WishList>
    abstract member SaveWishList : Domain.WishList -> Task<unit>
    abstract member GetLastResetTime : unit -> Task<System.DateTime>

/// Start the web server and connect to database
let getDatabase databaseType startupTime =
    match databaseType with
    | DatabaseType.AzureStorage connection ->
        //Storage.WebJobs.startWebJobs connection
        { new IDatabaseFunctions with
            member __.LoadWishList key = getWishListFromDB connection key
            member __.SaveWishList wishList = saveWishListToDB connection wishList
            member __.GetLastResetTime () = task {
                let! resetTime = getLastResetTime connection
                return
                    resetTime
                    |> Option.defaultValue startupTime
            }
        }

    | DatabaseType.Postgres connectionString ->
        let postgresConfig = PostgresConfiguration.make connectionString
        { new IDatabaseFunctions with
            member __.LoadWishList key = PostgresTable.getWishListFromDB postgresConfig key
            member __.SaveWishList wishList = PostgresTable.saveWishListToDB postgresConfig wishList
            member __.GetLastResetTime () = Task.FromResult startupTime
        }

    | DatabaseType.FileSystem ->
        { new IDatabaseFunctions with
            member __.LoadWishList key = Task.FromResult (Storage.FileSystem.getWishListFromDB key)
            member __.SaveWishList wishList = Task.FromResult (Storage.FileSystem.saveWishListToDB wishList)
            member __.GetLastResetTime () = Task.FromResult startupTime
        }