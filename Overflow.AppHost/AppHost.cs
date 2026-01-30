var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIRECERTIFICATES001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var keycloak = builder.AddKeycloak("keycloak", 6001)
    .WithoutHttpsCertificate()
    .WithDataVolume("keycloak_data");
#pragma warning restore ASPIRECERTIFICATES001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


var postgres = builder.AddPostgres("postgres", port: 5432)
    .WithDataVolume("postgres-data")
    .WithPgAdmin();

var questionDb = postgres.AddDatabase("questiondb");

var typesenseApiKey = builder.AddParameter("typesense-api-key", secret: true);

var typesense = builder.AddContainer("typesense", "typesense/typesense", "29.0")
    .WithArgs("--data-dir", "/data", "--api-key", typesenseApiKey, "--enable-cors")
    .WithVolume("typesense_data", "/data")
    .WithHttpEndpoint(8108, 8108, name: "typesense");

var typesenseContainer = typesense.GetEndpoint("typesense");

var questionService = builder.AddProject<Projects.QuestionService>("question-svc")
    .WithReference(keycloak)
    .WithReference(questionDb)
    .WaitFor(keycloak)
    .WaitFor(questionDb);

var searchService = builder.AddProject<Projects.SearchService>("search-svc")
    .WithEnvironment("typesense-api-key", typesenseApiKey)
    .WithReference(typesenseContainer)
    .WaitFor(typesense);

builder.Build().Run();
