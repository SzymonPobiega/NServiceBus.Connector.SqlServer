using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

public static class DatabaseIntegration
{
    public static void UseSqlServer(this IServiceCollection serviceCollection, string connectionString)
    {
        serviceCollection.AddScoped((serviceProvider) =>
        {
            return new SqlConnection(connectionString);
        });
    }

    public static void UseOneTransactionPerHttpCall(this IServiceCollection serviceCollection, IsolationLevel level = IsolationLevel.ReadUncommitted)
    {
        serviceCollection.AddScoped((serviceProvider) =>
        {
            var connection = serviceProvider
                .GetService<SqlConnection>();
            connection.Open();

            return connection.BeginTransaction(level);
        });

        serviceCollection.AddScoped(typeof(UnitOfWorkFilter), typeof(UnitOfWorkFilter));

        serviceCollection
            .AddMvc(setup =>
            {
                setup.Filters.AddService<UnitOfWorkFilter>(1);
            });
    }

    class UnitOfWorkFilter : IAsyncActionFilter
    {
        private readonly SqlTransaction transaction;

        public UnitOfWorkFilter(SqlTransaction transaction)
        {
            this.transaction = transaction;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var connection = transaction.Connection;
            if (connection.State != ConnectionState.Open)
                throw new NotSupportedException("The provided connection was not open!");

            var executedContext = await next.Invoke();
            if (executedContext.Exception == null)
            {
                transaction.Commit();
            }
            else
            {
                transaction.Rollback();
            }
        }
    }
}
