using S7.Net;

public class ConnectionService
{
    public Plc? Plc { get; private set; }

    public bool IsConnected => Plc != null && Plc.IsConnected;

    public async Task<bool> ConnectAsync(CpuType cpuType, string ip, short rack, short slot)
    {
        if (IsConnected)
            return true;

        Plc = new Plc(cpuType, ip, rack, slot);
        await Plc.OpenAsync();
        return Plc.IsConnected;
    }

    public void Disconnect()
    {
        if (Plc != null)
        {
            Plc.Close();
            Plc = null;
        }
    }
}