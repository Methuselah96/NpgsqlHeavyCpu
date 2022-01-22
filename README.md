## Description

An example repository of failing to connect to Npgsql when using Azure Function while there is also heavy CPU usage.

## Usage

This function uses the boilerplate Durable Functions template.

The Azure Function can be published by following the boilerplate steps to [publish the project to Azure](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-create-first-csharp?pivots=code-editor-vscode#publish-the-project-to-azure) or use the Publish feature in Visual Studio.

The `PostgresConnectionString` application setting needs to be set to a valid PostgreSQL connection string. For testing I have been using an "Azure Database for PostgreSQL server" with PostgreSQL version 11 and Performance configuration of "General Purpose, 2 vCore(s), 100 GB". The connection string should also include `Timeout=300` otherwise it will timeout trying to establish a connection. The need to increase the connection timeout also indicates that network tasks are being starved.

To reproduce the issue call the `NpgsqlOrchestration_HttpStart` function route. Note that the issue might not happen every time the function is run, so it might have to be run more than once.
