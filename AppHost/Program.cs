var builder = DistributedApplication.CreateBuilder(args);


builder.AddProject<Projects.Unify_People>("Web");


builder.Build().Run();
