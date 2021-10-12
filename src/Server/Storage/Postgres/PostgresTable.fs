namespace ServerCode.Storage.Postgres

open System
open Npgsql
open ServerCode.Domain
open System.Data.Common

module PostgresTable =
    let rec private readBooks (reader : DbDataReader) acc =
        async {
            let! canRead = reader.ReadAsync() |> Async.AwaitTask
            if not canRead then
                return acc
            else
                let newBook = {
                    Title = reader.GetString(0)
                    Authors = reader.GetString(1)
                    ImageLink = reader.GetString(2)
                    Link = reader.GetString(3)
                }
                return! readBooks reader (newBook :: acc)
        }

    let getWishListFromDB postgresConfiguration (userName : string) =
        let sql = "SELECT title, authors, image_link, link FROM wish_list WHERE username = @username"
        async {
            use! connection = PostgresConfiguration.openConnection postgresConfiguration
            use cmd = new NpgsqlCommand(sql, connection)
            cmd.Parameters.AddWithValue("username", userName) |> ignore
            let! reader = cmd.ExecuteReaderAsync() |> Async.AwaitTask
            let! books = readBooks reader []
            return { Books = books; UserName = userName }
        }
        |> Async.StartAsTask

    let private insertBook (config : PostgresConfiguration) (user : string) (book : Book) =
        let sql =
            "INSERT INTO wish_list (
                title,
                authors,
                image_link,
                link,
                username,
                created_at
            ) VALUES (
                @title,
                @authors,
                @image_link,
                @link,
                @username,
                @created_at
            )"
        async {
            use! connection = PostgresConfiguration.openConnection config
            use cmd = new NpgsqlCommand(sql, connection)
            cmd.Parameters.AddWithValue("title", book.Title) |> ignore
            cmd.Parameters.AddWithValue("authors", book.Authors) |> ignore
            cmd.Parameters.AddWithValue("image_link", book.ImageLink) |> ignore
            cmd.Parameters.AddWithValue("link", book.Link) |> ignore
            cmd.Parameters.AddWithValue("username", user) |> ignore
            cmd.Parameters.AddWithValue("created_at", DateTime.Now) |> ignore
            let! _ = cmd.ExecuteNonQueryAsync() |> Async.AwaitTask
            return ()
        }
        
    let private removeBook (config : PostgresConfiguration) (user : string) (book : Book) =
        let sql =
            "DELETE FROM wish_list 
            WHERE title = @title
            AND authors = @authors
            AND image_link = @image_link
            AND link = @link
            AND username = @username"
        async {
            use! connection = PostgresConfiguration.openConnection config
            use cmd = new NpgsqlCommand(sql, connection)
            cmd.Parameters.AddWithValue("title", book.Title) |> ignore
            cmd.Parameters.AddWithValue("authors", book.Authors) |> ignore
            cmd.Parameters.AddWithValue("image_link", book.ImageLink) |> ignore
            cmd.Parameters.AddWithValue("link", book.Link) |> ignore
            cmd.Parameters.AddWithValue("username", user) |> ignore
            let! _ = cmd.ExecuteNonQueryAsync() |> Async.AwaitTask
            return ()
        }

    let saveWishListToDB (config : PostgresConfiguration) (wishList : WishList) =
        async {
            let! existing = getWishListFromDB config wishList.UserName |> Async.AwaitTask

            // Remove old books
            let removeTasks =
                existing.Books
                |> List.filter (fun book ->
                    wishList.Books |> Seq.contains book |> not)
                |> List.map (removeBook config wishList.UserName)

            // Add new books
            let addTasks =
                wishList.Books
                |> List.filter (fun book ->
                    existing.Books |> Seq.contains book |> not)
                |> List.map (insertBook config wishList.UserName)

            // Await all the tasks
            let! _ = addTasks @ removeTasks |> Async.Parallel
            return ()
        }
        |> Async.StartAsTask
