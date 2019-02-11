using System.Data.SqlClient;
using System.Threading.Tasks;

namespace NServiceBus.Connector.SqlServer
{
    /// <summary>
    /// An instance of a connector
    /// </summary>
    public interface IConnector
    {
        /// <summary>
        /// Starts the connector.
        /// </summary>
        Task Start();

        /// <summary>
        /// Returns an NServiceBus session that can be used to send/publish messages using provided connection/transaction.
        /// </summary>
        /// <param name="connection">Open connection to SQL Server.</param>
        /// <param name="transaction">Transaction to use when sending messages.</param>
        IMessageSession GetSession(SqlConnection connection, SqlTransaction transaction);

        /// <summary>
        /// Stops the connector.
        /// </summary>
        Task Stop();
    }
}