﻿/// Server program entry point module.
module ServerCode.Program

open System
open System.IO
open Microsoft.Extensions.Logging
open Saturn.Application
open Microsoft.Extensions.DependencyInjection
open Thoth.Json.Giraffe
open ServerCode.Database
open ServerCode.Storage.Postgres

let GetEnvVar var =
    match Environment.GetEnvironmentVariable(var) with
    | null -> None
    | value -> Some value

let getPortsOrDefault defaultVal =
    match Environment.GetEnvironmentVariable("GIRAFFE_FABLE_PORT") with
    | null -> defaultVal
    | value -> value |> uint16

let serviceConfig (services : IServiceCollection) =
    services.AddSingleton<Giraffe.Serialization.Json.IJsonSerializer>(ThothSerializer())

let (|Azure|_|) args =
    args
    |> List.tryFind(fun (arg : string) ->
        arg.StartsWith "AzureConnection=")
    |> Option.map (fun arg ->
        arg.Substring "AzureConnection=".Length)

let (|Postgres|_|) args : string option =
    args
    |> List.tryFind(fun (arg : string) ->
        arg.StartsWith "PostgresConnection=")
    |> Option.map (fun (arg : string) ->
        // Wants to look like
        // Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase
        arg.Substring "PostgresConnection=".Length)

let azureDatabase connectionString =
    connectionString
    |> Storage.AzureTable.AzureConnection
    |> DatabaseType.AzureStorage

let postgresDatabase configuration =
    Migrator.migrate configuration |> Async.RunSynchronously
    configuration |> DatabaseType.Postgres

let fileBackedDatabase () =
    DatabaseType.FileSystem

[<EntryPoint>]
let main args =
    try
        let args = Array.toList args
        let clientPath =
            match args with
            | clientPath:: _  when Directory.Exists clientPath -> clientPath
            | _ ->
                // did we start from server folder?
                let devPath = Path.Combine("..","Client")
                if Directory.Exists devPath then devPath
                else
                    // maybe we are in root of project?
                    let devPath = Path.Combine("src", "Client")
                    if Directory.Exists devPath then devPath
                    else @"./client"
            |> Path.GetFullPath

        let database =
            match args with
            | Azure connectionString ->
                azureDatabase connectionString
            | Postgres connectionString ->
                postgresDatabase connectionString
            | _ ->
                fileBackedDatabase ()

        let port = getPortsOrDefault 8085us

        let app = application {
            use_router (WebServer.webApp database)
            url ("http://0.0.0.0:" + port.ToString() + "/")

            use_jwt_authentication JsonWebToken.secret JsonWebToken.issuer
            service_config serviceConfig
            use_static clientPath
            use_gzip
        }
        run app
        0
    with
    | exn ->
        let color = Console.ForegroundColor
        Console.ForegroundColor <- System.ConsoleColor.Red
        Console.WriteLine(exn.Message)
        Console.ForegroundColor <- color
        1
