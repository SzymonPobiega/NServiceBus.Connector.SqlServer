using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;

public class SendMessageController :
    Controller
{
    IMessageSession messageSession;
    SqlConnection conn;
    SqlTransaction trans;

    #region MessageSessionInjection
    public SendMessageController(IMessageSession messageSession, SqlConnection conn, SqlTransaction trans)
    {
        this.messageSession = messageSession;
        this.conn = conn;
        this.trans = trans;
    }
    #endregion

    #region MessageSessionUsage
    [HttpGet]
    public async Task<string> Get()
    {
        await messageSession.Send(new MyMessage())
            .ConfigureAwait(false);

        await messageSession.Publish(new MyEvent())
            .ConfigureAwait(false);

        using (var command = new SqlCommand("insert into Widgets default values;", conn, trans))
        {
            await command.ExecuteNonQueryAsync()
                .ConfigureAwait(false);
        }

        return "Message sent to endpoint";
    }
    #endregion
}
