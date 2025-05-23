IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RedisResource> cache = builder.AddRedis("cache");

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Api>("backend");

builder.AddProject<Projects.Web>("frontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
