using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Typesense;

namespace SearchService.Data
{
    public static class SearchInitializer
    {
        public static async Task EnsureIndexExists(ITypesenseClient client, int maxRetries = 10)
        {
            const string schemaName = "questions";

            // Retry logic for checking if collection exists
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await client.RetrieveCollection(schemaName);
                    Console.WriteLine($"Collection {schemaName} already exists.");
                    return;
                }
                catch (TypesenseApiNotFoundException)
                {
                    Console.WriteLine($"Collection {schemaName} does not exist. Creating...");
                    break;
                }
                catch (TypesenseApiUnprocessableEntityException ex) when (ex.Message.Contains("Not Ready or Lagging"))
                {
                    Console.WriteLine($"Typesense not ready (attempt {attempt}/{maxRetries}). Waiting 2s...");
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2));
                        continue;
                    }
                    throw;
                }
            }

            // Retry logic for creating collection
            var schema = new Schema(schemaName, new List<Field>
            {
                new("id", FieldType.String) ,
                new("title", FieldType.String) ,
                new("content", FieldType.String) ,
                new("tags", FieldType.StringArray) ,
                new("createdAt", FieldType.Int64) ,
                new("answerCount", FieldType.Int32),
                new("hasAcceptedAnswer", FieldType.Bool) ,
            })
            {
                DefaultSortingField = "createdAt"
            };

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await client.CreateCollection(schema);
                    Console.WriteLine($"Collection {schemaName} created successfully.");
                    return;
                }
                catch (TypesenseApiUnprocessableEntityException ex) when (ex.Message.Contains("Not Ready or Lagging"))
                {
                    Console.WriteLine($"Typesense not ready (attempt {attempt}/{maxRetries}). Waiting 2s...");
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}