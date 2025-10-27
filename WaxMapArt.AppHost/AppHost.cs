var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("docker-env");

// PostgreSQL Database with password as secret parameter
var postgresPassword = builder.AddParameter("postgres-password", secret: true);
var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume()
    .WithPgAdmin();

var database = postgres.AddDatabase("waxmapart-db");

// MinIO Object Storage with HTTPS support
var minioAccessKey = builder.AddParameter("minio-access-key", secret: true);
var minioSecretKey = builder.AddParameter("minio-secret-key", secret: true);

var minio = builder.AddContainer("minio", "minio/minio")
    .WithHttpsEndpoint(port: 9000, targetPort: 9000, name: "api")
    .WithHttpsEndpoint(port: 9001, targetPort: 9001, name: "console")
    .WithEnvironment("MINIO_ROOT_USER", minioAccessKey)
    .WithEnvironment("MINIO_ROOT_PASSWORD", minioSecretKey)
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithBindMount("minio-data", "/data");

// JWT Secret Key as parameter
var jwtSecretKey = builder.AddParameter("jwt-secret-key", secret: true);

// Web Application with secure configuration
builder.AddProject<Projects.WaxMapArt_Web>("waxmapart-web")
    .WithReference(database)
    .WithEnvironment("MinIO__Endpoint", minio.GetEndpoint("api"))
    .WithEnvironment("MinIO__AccessKey", minioAccessKey)
    .WithEnvironment("MinIO__SecretKey", minioSecretKey)
    .WithEnvironment("JwtSettings__SecretKey", jwtSecretKey)
    .WithHttpsEndpoint(name: "https");

builder.Build().Run();
