# Sleepr

## SQLite Output Store

Sleepr stores agent output using an SQLite database. The default database file
location is configured in `appsettings.json` under `OutputDb:Path`.

Set the connection string in a `.env` file before running the application:

```
OUTPUT_DB_CONNECTION_STRING=Data Source=data/agent-output.db
```

Create the `.env` file at the repository root (it is ignored by Git).
