namespace ServerCode.Storage.Postgres

open Npgsql

type PostgresConfiguration =
    {
        Connection : string
    }

module PostgresConfiguration =

    let make (connectionString : string) : PostgresConfiguration =
        { Connection = connectionString }

    let openConnection (table : PostgresConfiguration) : Async<NpgsqlConnection> =
        async {
            let connection = new NpgsqlConnection(table.Connection)
            do! connection.OpenAsync() |> Async.AwaitTask 
            return connection
        }