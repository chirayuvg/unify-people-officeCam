var builder = DistributedApplication.CreateBuilder(args);


var pgusername = builder.AddParameter("pgusername");
var pgpassword = builder.AddParameter("pgpassword");

var postgres = builder.AddPostgres("postgres", port: 5432, userName: pgusername, password: pgpassword)
                         .WithAnnotation(new ContainerImageAnnotation { Image = "pgvector/pgvector", Tag = "pg17" })
                      .WithDataBindMount(source: @"C:\PostgreSQL\Data", isReadOnly: false)
                      .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050));

var applicationdb = postgres.AddDatabase("applicationdb");

var peopledb = postgres.AddDatabase("peopledb");



builder.AddProject<Projects.Unify_People>("Web")
    .WithReference(peopledb).WithReference(applicationdb)
    .WaitFor(peopledb).WaitFor(applicationdb);


builder.Build().Run();
