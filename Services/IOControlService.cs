using S7.Net;
public class IOControlService
{
    private readonly ConnectionService _connection;

    public IOControlService(ConnectionService connection)
    {
        _connection = connection;
    }

    public bool ReadBit(string address)
    {
        var plc = _connection.Plc;
        if (plc == null || !plc.IsConnected)
            throw new InvalidOperationException("PLC 未连接");
        return false;
    }

    public void WriteBit(string address, bool value)
    {
        var plc = _connection.Plc;
        if (plc == null || !plc.IsConnected)
            throw new InvalidOperationException("PLC 未连接");

    }
}