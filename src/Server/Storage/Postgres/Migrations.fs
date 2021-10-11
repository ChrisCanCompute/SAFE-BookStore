namespace ServerCode.Storage.Postgres

open SimpleMigrations

[<Migration(1L, "Create wish list table")>]
type ``Create wish list table``() =
    inherit Migration()

    override this.Up () =
        this.Execute("""
            CREATE TABLE wish_list (
                id SERIAL PRIMARY KEY,
                title text NOT NULL,
                authors text NOT NULL,
                image_link text NOT NULL,
                link text NOT NULL,
                username text NOT NULL
            )
        """)

    override this.Down () =
        this.Execute("""
            DROP TABLE wish_list;
        """)


