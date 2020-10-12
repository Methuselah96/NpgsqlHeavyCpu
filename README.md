## Description

An example repository of failing to connect to Npgsql when using Azure Function while there is also heavy CPU usage.

## Usage

This function uses the boilerplate Durable Functions template. In order to reproduce the issue you can follow the boilerplate steps to [publish the project to Azure](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-create-first-csharp?pivots=code-editor-vscode#publish-the-project-to-azure) or use the Publish feature in Visual Studio.

I imagine that this behavior would be exhibited on any single-core computer where there are a lot of threads that are CPU-heavy.