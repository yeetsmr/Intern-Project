using Microsoft.Extensions.DependencyInjection;

namespace InternProject.Business
{
    public static class BusinessServiceRegistration
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<TaskService>();
            services.AddScoped<AuthService>();

            return services;
        }
    }
}