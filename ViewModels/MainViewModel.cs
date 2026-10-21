using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using S7.Net;
using System.Net;
using System.Net.Sockets;

public partial class MainViewModel : ObservableObject
{
    private readonly ConnectionService _connection;
    private readonly IOControlService _ioService;

    [ObservableProperty]
    private string _logText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string _ipAddress = "192.168.0.2";

    [ObservableProperty]
    private int _port = 102;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private short? _rack = 0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private short? _slot = 1;

    public MainViewModel()
    {
        _connection = new ConnectionService();
        _ioService = new IOControlService(_connection);
    }
    [RelayCommand(CanExecute = nameof(CanConnect))]
    public async Task Connect()
    {
        try
        {
            AppendLog($"{IpAddress} 正在连接");
            var ok = await _connection.ConnectAsync(
                CpuType.S7200Smart, IpAddress, (short)Rack, (short)Slot);

            if (ok)
            {
                AppendLog($"{IpAddress} 连接成功");

                var Inputstate = await _ioService.ReadInputStateAsync();
                var Outputstate = await _ioService.ReadOutputStateAsync();
                // Do something with the states
                AppendLog($"当前 Input 状态：{Convert.ToString(Inputstate, 2).PadLeft(8, '0')}");
                AppendLog($"当前 Output 状态：{Convert.ToString(Outputstate, 2).PadLeft(8, '0')}");
            }
            else
                AppendLog("连接失败");
        }
        catch (Exception e)
        {
            AppendLog($"连接异常：{e.Message}");
        }
    }

    [RelayCommand]
    public void Disconnection()
    {
        _connection.Disconnect();
        AppendLog("PLC 已断开连接");
    }

    private void AppendLog(string message)
    {
        LogText += $"{DateTime.Now:HH:mm:ss} {message}{Environment.NewLine}";
    }

    private bool CanConnect()
    {
        return IPAddress.TryParse(IpAddress, out var addr)
               && addr.AddressFamily == AddressFamily.InterNetwork
               && Rack >= 0 && Rack <= 255
               && Slot >= 0 && Slot <= 255;
    }

    private async void ChangeI0State()
    {
        try
        {
            var Inputstate = await _ioService.ReadInputStateAsync();
            AppendLog($"当前 Input 状态：{Convert.ToString(Inputstate, 2).PadLeft(8, '0')}");
        }
        catch (Exception e)
        {
            AppendLog($"读取 I0 状态异常：{e.Message}");
        }
    }
}