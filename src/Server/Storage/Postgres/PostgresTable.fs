namespace ServerCode.Storage.Postgres

open Npgsql
open ServerCode.Domain
open System.Data.Common

module PostgresTable = 
    let make (connectionString : string) : PostgresConfiguration =
        { Connection = connectionString }

    let rec private readBooks (reader : DbDataReader) acc =
        async {
            let! canRead = reader.ReadAsync() |> Async.AwaitTask
            if canRead then
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

    let getWishListFromDB table (userName : string) =
        let sql = "SELECT title, authors, image_link, link FROM wish_list WHERE user = @user"
        async {
            use! connection = PostgresConfiguration.openConnection table
            use cmd = new NpgsqlCommand(sql, connection)
            cmd.Parameters.AddWithValue("user", userName) |> ignore
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
                user
            ) VALUES (
                @title,
                @authors,
                @image_link,
                @link,
                @user
            )"
        async {
            use! connection = PostgresConfiguration.openConnection config
            use cmd = new NpgsqlCommand(sql, connection)
            cmd.Parameters.AddWithValue("title", book.Title) |> ignore
            cmd.Parameters.AddWithValue("authors", book.Authors) |> ignore
            cmd.Parameters.AddWithValue("image_link", book.ImageLink) |> ignore
            cmd.Parameters.AddWithValue("link", book.Link) |> ignore
            cmd.Parameters.AddWithValue("user", user) |> ignore
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
            AND user = @user"
        async {
            use! connection = PostgresConfiguration.openConnection config
            use cmd = new NpgsqlCommand(sql, connection)
            cmd.Parameters.AddWithValue("title", book.Title) |> ignore
            cmd.Parameters.AddWithValue("authors", book.Authors) |> ignore
            cmd.Parameters.AddWithValue("image_link", book.ImageLink) |> ignore
            cmd.Parameters.AddWithValue("link", book.Link) |> ignore
            cmd.Parameters.AddWithValue("user", user) |> ignore
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
