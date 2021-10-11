namespace Cleaner

open System
open ServerCode.Storage.Postgres
open Npgsql

module Program =

    let (|Postgres|_|) args : string option =
        args
        |> List.tryFind(fun (arg : string) ->
            arg.StartsWith "PostgresConnection=")
        |> Option.map (fun (arg : string) ->
            // Wants to look like
            // Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase
            arg.Substring "PostgresConnection=".Length)

    let assertLatestMigrations connectionString =
        if not (Migrator.isLatest connectionString |> Async.RunSynchronously) then
            failwith "Migrations are not up to date, will not do any cleaning!"

    let clean connection =
        
        ()


    [<EntryPoint>]
    let main args =
        try
            let args = Array.toList args
            let connectionString =
                match args with
                | Postgres connectionString -> connectionString
                | _ -> failwithf "Could not find postgres connection string in args: %A" args
            
            assertLatestMigrations connectionString
            let connection = connectionString |> PostgresConfiguration.make |> PostgresConfiguration.openConnection |> Async.RunSynchronously
            use transaction = connection.BeginTransaction ()

            clean connection

            transaction.Commit ()

            0
        with
        | exn ->
            let color = Console.ForegroundColor
            Console.ForegroundColor <- System.ConsoleColor.Red
            Console.WriteLine(exn.Message)
            Console.ForegroundColor <- color
            1