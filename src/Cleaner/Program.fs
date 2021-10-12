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
        let earliestBook = DateTime.Now - TimeSpan.FromDays 7.0

        let countSql = "SELECT COUNT(*) FROM wish_list WHERE created_at < @delete_before"
        use countCmd = new NpgsqlCommand(countSql, connection)
        countCmd.Parameters.AddWithValue("delete_before", earliestBook) |> ignore
        use reader = countCmd.ExecuteReaderAsync().Result
        reader.ReadAsync().Result |> ignore
        let count = reader.GetInt32(0)
        reader.Dispose()
        countCmd.Dispose()
        printf "Deleting %i books from the wish list ... " count

        let deleteSql = "DELETE FROM wish_list WHERE created_at < @delete_before"
        use deleteCmd = new NpgsqlCommand(deleteSql, connection)
        deleteCmd.Parameters.AddWithValue("delete_before", earliestBook) |> ignore
        deleteCmd.ExecuteNonQueryAsync().Result |> ignore
        printfn "Deleted"

    [<EntryPoint>]
    let main args =
        try
            let args = Array.toList args
            let connectionString =
                match args with
                | Postgres connectionString -> connectionString
                | _ -> failwithf "Could not find postgres connection string in args: '%s'" (args |> String.concat " ")
            
            assertLatestMigrations connectionString
            let connection = connectionString |> PostgresConfiguration.make |> PostgresConfiguration.openConnection |> Async.RunSynchronously
            use transaction = connection.BeginTransaction ()

            clean connection

            printf "Committing transaction ... "
            transaction.Commit ()
            printfn "Committed"

            0
        with
        | exn ->
            let color = Console.ForegroundColor
            Console.ForegroundColor <- System.ConsoleColor.Red
            Console.WriteLine(exn.Message)
            Console.ForegroundColor <- color
            1