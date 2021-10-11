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


[<Migration(2L, "Add created at date")>]
type ``Add created at date``() =
    inherit Migration()

    override this.Up () =
        this.Execute("""
            ALTER TABLE wish_list
                ADD COLUMN created_at date;
        """)

    override this.Down () =
        this.Execute("""
            ALTER TABLE wish_list
                DROP COLUMN created_at;
        """)

