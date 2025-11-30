using System.Diagnostics;

namespace EmoLcd.GpioApp;

/// <summary>
/// GPIO 按鈕監聽器，透過 gpiomon 工具偵測按鈕按下與放開事件。
/// </summary>
/// <remarks>
/// 專為 Raspberry Pi 5 設計，使用 libgpiod v2 工具（gpiomon、gpioget）。
/// 內建 50ms 防抖機制避免誤觸發。
/// </remarks>
public sealed class GpioButtonMonitor : IDisposable
{
    private const string GpioChip = "gpiochip0";
    private const int ButtonPin = 5;
    private static readonly TimeSpan DebounceInterval = TimeSpan.FromMilliseconds(50);

    private Process? _gpiomonProcess;
    private readonly CancellationTokenSource _cts = new();
    private DateTime _lastEventTime = DateTime.MinValue;
    private bool _disposed;

    /// <summary>
    /// 當按鈕被按下時觸發（falling edge，電壓從 HIGH 降到 LOW）。
    /// </summary>
    public event Action? OnPressed;

    /// <summary>
    /// 當按鈕被放開時觸發（rising edge，電壓從 LOW 回到 HIGH）。
    /// </summary>
    public event Action? OnReleased;

    /// <summary>
    /// 讀取按鈕的當前狀態。
    /// </summary>
    /// <returns>true 表示按鈕目前被按下（LOW），false 表示放開（HIGH）。</returns>
    /// <exception cref="InvalidOperationException">無法讀取 GPIO 狀態。</exception>
    public static bool ReadCurrentState()
    {
        EnsureGpioToolsAvailable();

        var output = RunCommand("gpioget", $"-c {GpioChip} -b pull-up {ButtonPin}");
        return output.Trim() == "0"; // LOW = 按下
    }

    /// <summary>
    /// 啟動 GPIO 事件監聽，此方法會阻塞直到收到取消訊號。
    /// </summary>
    /// <param name="cancellationToken">取消 Token，用於停止監聽。</param>
    /// <exception cref="InvalidOperationException">gpiomon 工具未安裝。</exception>
    public void Start(CancellationToken cancellationToken)
    {
        EnsureGpioToolsAvailable();

        _gpiomonProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "gpiomon",
                Arguments = $"-c {GpioChip} -b pull-up -e both {ButtonPin}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        // 連結外部取消 Token
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);

        _gpiomonProcess.Start();

        try
        {
            while (!linkedCts.Token.IsCancellationRequested)
            {
                var line = _gpiomonProcess.StandardOutput.ReadLine();
                if (line == null) break; // 程序結束

                ProcessEvent(line);
            }
        }
        finally
        {
            Stop();
        }
    }

    /// <summary>
    /// 停止 GPIO 監聽並終止 gpiomon 程序。
    /// </summary>
    public void Stop()
    {
        _cts.Cancel();

        if (_gpiomonProcess != null && !_gpiomonProcess.HasExited)
        {
            try
            {
                _gpiomonProcess.Kill();
                _gpiomonProcess.WaitForExit(1000);
            }
            catch
            {
                // 忽略終止錯誤
            }
        }

        _gpiomonProcess?.Dispose();
        _gpiomonProcess = null;
    }

    /// <summary>
    /// 處理 gpiomon 輸出的事件行，包含防抖邏輯。
    /// </summary>
    private void ProcessEvent(string line)
    {
        var now = DateTime.UtcNow;

        // 防抖：距離上次事件不足 50ms 則忽略
        if (now - _lastEventTime < DebounceInterval)
        {
            return;
        }

        // gpiomon v2 輸出格式: "2 5" (falling) 或 "1 5" (rising)
        // 也可能是 "falling 5" / "rising 5"
        bool isFalling = line.Contains("falling") || line.StartsWith("2");
        bool isRising = line.Contains("rising") || line.StartsWith("1");

        if (isFalling)
        {
            _lastEventTime = now;
            OnPressed?.Invoke();
        }
        else if (isRising)
        {
            _lastEventTime = now;
            OnReleased?.Invoke();
        }
    }

    /// <summary>
    /// 確認 gpiomon 與 gpioget 工具已安裝。
    /// </summary>
    /// <exception cref="InvalidOperationException">工具未安裝。</exception>
    private static void EnsureGpioToolsAvailable()
    {
        var whichGpiomon = RunCommand("which", "gpiomon");
        var whichGpioget = RunCommand("which", "gpioget");

        if (string.IsNullOrWhiteSpace(whichGpiomon) || string.IsNullOrWhiteSpace(whichGpioget))
        {
            throw new InvalidOperationException(
                "找不到 gpiomon 或 gpioget 工具。請安裝 libgpiod-utils 套件：\n" +
                "  sudo apt install gpiod");
        }
    }

    /// <summary>
    /// 執行外部命令並回傳標準輸出。
    /// </summary>
    private static string RunCommand(string cmd, string args)
    {
        try
        {
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = cmd,
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            proc?.WaitForExit(3000);
            return proc?.StandardOutput.ReadToEnd() ?? "";
        }
        catch
        {
            return "";
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Stop();
        _cts.Dispose();
    }
}
