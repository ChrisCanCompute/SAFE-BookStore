namespace ServerCode.Storage.Postgres

open Npgsql
open SimpleMigrations
open SimpleMigrations.DatabaseProvider

module Migrator =
    let private make (connection : NpgsqlConnection) (schema : string) (createSchema : bool) =
        let provider = PostgresqlDatabaseProvider(connection)
        provider.SchemaName <- schema
        provider.CreateSchema <- createSchema
        let migrationsAssembly = typeof<``Create wish list table``>.Assembly
        SimpleMigrator(migrationsAssembly, provider)

    let isLatest (connectionString : string) =
        let postgresConfig = PostgresConfiguration.make connectionString
        async {
            use! connection = PostgresConfiguration.openConnection postgresConfig
            let migrator = make connection "public" false
            migrator.Load()
            return migrator.CurrentMigration.Version = migrator.LatestMigration.Version
        }

    let migrate (connectionString : string) =
        let postgresConfig = PostgresConfiguration.make connectionString
        async {
            use! connection = PostgresConfiguration.openConnection postgresConfig
            let migrator = make connection "public" false
            migrator.Load()
            if migrator.CurrentMigration.Version < migrator.LatestMigration.Version then
                migrator.MigrateToLatest()
        }