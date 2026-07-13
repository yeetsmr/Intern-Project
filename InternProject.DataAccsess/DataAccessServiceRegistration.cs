using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace InternProject.DataAccess
{
    public static class DataAccessServiceRegistration
    {
        public static IServiceCollection AddDataAccessServices(this IServiceCollection services, IConfiguration configuration)
        {
            string mongoConnectionString = configuration["MongoDbSettings:ConnectionString"]
                ?? "mongodb://localhost:27017/?maxPoolSize=1000";

            string databaseName = configuration["MongoDbSettings:DatabaseName"]
                ?? "TaskData";

            services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));

            services.AddScoped<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(databaseName);
            });

            services.AddScoped<TaskRepository>();

            return services;
        }
    }
}