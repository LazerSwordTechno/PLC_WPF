using S7.Net;

public class IOControlService
{
    private readonly ConnectionService _connection;

    public IOControlService(ConnectionService connection)
    {
        _connection = connection;
    }

    public async Task<byte> ReadInputStateAsync()
    {
        var plc = _connection.Plc;
        if (plc == null || !plc.IsConnected)
            throw new InvalidOperationException("PLC 未连接");

        // 一次读取 I0 整个字节 (I0.0 ~ I0.7)
        byte[] buffer = await plc.ReadBytesAsync(DataType.Input, 0, 0, 1);
        return buffer[0];
    }
    public async Task<byte> ReadOutputStateAsync()
    {
        var plc = _connection.Plc;
        if (plc == null || !plc.IsConnected)
            throw new InvalidOperationException("PLC 未连接");

        // 一次读取 Q0 整个字节 (Q0.0 ~ Q0.7)
        byte[] buffer = await plc.ReadBytesAsync(DataType.Output, 0, 0, 1);
        return buffer[0];
    }


    public async Task WriteBitAsync(string address, bool value)
    {
        var plc = _connection.Plc;
        if (plc == null || !plc.IsConnected)
            throw new InvalidOperationException("PLC 未连接");

        await plc.WriteAsync(address, value);
    }
}