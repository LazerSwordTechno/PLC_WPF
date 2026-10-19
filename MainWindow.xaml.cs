using S7.Net;
using System.Net.Mime;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PlcIoPanel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Plc plc;
        private DispatcherTimer ConnectionChecker;
        private CheckBox[] chkBoxesInput;
        private CheckBox[] chkBoxesOutput;

        public MainWindow()
        {
            InitializeComponent();
            InitTimer();
            chkBoxesInput = new CheckBox[] { ChkI0, ChkI1, ChkI2, ChkI3, ChkI4, ChkI5, ChkI6, ChkI7 };
            chkBoxesOutput = new CheckBox[] { ChkQ0, ChkQ1, ChkQ2, ChkQ3, ChkQ4, ChkQ5, ChkQ6, ChkQ7 };
        }

        private void InitTimer()
        {
            ConnectionChecker = new DispatcherTimer();

            ConnectionChecker.Interval = TimeSpan.FromMilliseconds(100);

            ConnectionChecker.Tick += ConnCheck;    
        }
        private void ConnCheck(object sender, EventArgs e)
        {
            // 如果 plc 还没初始化（还没点过连接按钮），直接返回
            if (plc == null) return;

            try
            {
                //读取I0状态
                byte I0Byte = (Byte)plc.Read("IB0");

                bool[] I0States = new bool[8];
                for (int i = 0; i < 8; i++)
                {
                    I0States[i] = (I0Byte & (1 << i)) != 0;
                }

                for(int i = 0; i < 8; i++)
                {
                    chkBoxesInput[i].IsChecked = I0States[i];
                }

                //读取Q0状态
                byte Q0Byte = (Byte)plc.Read("QB0");

                bool[] Q0States = new bool[8];
                for (int i = 0; i < 8; i++)
                {
                    Q0States[i] = (Q0Byte & (1 << i)) != 0;
                }

                for (int i = 0; i < 8; i++)
                {
                    chkBoxesOutput[i].IsChecked = Q0States[i];
                }

                LedConn.Fill = Resources["Brush.InputOn"] as SolidColorBrush;
            }
            catch
            {
                LedConn.Fill = Resources["Brush.Danger"] as SolidColorBrush;
                TxtConnState.Text = "已断开";

                TxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] PLC 连接已断开" + Environment.NewLine);
                TxtLog.ScrollToEnd();

                ConnectionChecker.Stop();
            }
        }

        private async void ConnectPLC_Click(object sender, RoutedEventArgs e)
        {
            // 1. 在 UI 线程获取参数（防止后台线程访问 UI 控件 TxtIp 等崩溃）
            string ip = TxtIp.Text;
            int rack = Convert.ToInt16(TxtRack.Text);
            int slot = Convert.ToInt16(TxtSlot.Text);

            TxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 正在连接到：{TxtIp.Text}" + Environment.NewLine);
            // 2. 启动后台线程执行连接
            await Task.Run(() =>
            {
                try
                { 
                    // 初始化全局的 PLC 对象
                    plc = new Plc(CpuType.S7200Smart, ip, (short)rack, (short)slot);

                    // 在后台线程尝试连接（耗时操作）
                    plc.Open();

                    // 3. 连接完成后，使用 Dispatcher 安全地回到 UI 主线程更新界面
                    Dispatcher.Invoke(() =>
                    {
                        if (plc.IsConnected)
                        {
                            LedConn.Fill = this.Resources["Brush.InputOn"] as SolidColorBrush;
                            TxtConnState.Text = "已连接";


                            ConnectionChecker.Start();
                            TxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] PLC 已成功连接!" + Environment.NewLine);
                            TxtLog.ScrollToEnd(); // 自动滚动到底部
                        }
                        else
                        {
                            TxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 连接失败，请检查网络或参数。" + Environment.NewLine);
                            TxtLog.ScrollToEnd();
                        }
                    });
                }
                catch (Exception ex)
                {
                    // 4. 发生异常时，也必须通过 Dispatcher 回到主线程打印日志
                    Dispatcher.Invoke(() =>
                    {
                        TxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 连接异常: {ex.Message}" + Environment.NewLine);
                        TxtLog.ScrollToEnd();
                    });
                }
            });
        }

        private void DisconnectPLC_Click(object sender, RoutedEventArgs e)
        {
            if (plc == null)
            {
                TxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] PLC未连接" + Environment.NewLine);
            }
            else
            {
                plc = null;
                LedConn.Fill = this.Resources["Brush.Danger"] as SolidColorBrush;

                for (int i = 0; i < 8; i++)
                {
                    chkBoxesInput[i].IsChecked = false;
                    chkBoxesOutput[i].IsChecked = false;
                }

                TxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] PLC连接已断开" + Environment.NewLine);
                TxtConnState.Text = "已断开";
                TxtLog.ScrollToEnd();
            }
        }
    }
}