var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak", 6001)
    .WithDataVolume("keycloak_data");


builder.Build().Run();
