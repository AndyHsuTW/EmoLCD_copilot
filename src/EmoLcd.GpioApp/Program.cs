using EmoLcd.Domain.Models;
using EmoLcd.Rendering;
using EmoLcd.Rendering.Display;
using EmoLcd.Rendering.Framebuffer;

namespace EmoLcd.GpioApp;

/// <summary>
/// EmoLCD GPIO 互動應用程式。
/// </summary>
/// <remarks>
/// 監聽 GPIO5 按鈕事件，按下顯示笑臉，放開顯示中性表情。
/// </remarks>
public static class Program
{
    private static readonly object RenderLock = new();
    private static readonly object StateLock = new();
    private static RenderPipeline? _pipeline;
    private static IDisplayTarget? _displayTarget;
    private static RenderTarget _targetMode;
    private static string _framebufferPath = "/dev/fb0";
    private static string _outputPath = "";
    
    // 狀態管理
    private static volatile bool _isRendering = false;
    private static Emotion _currentRenderedEmotion = Emotion.Neutral; // 假設初始為 Neutral，或由 Main 更新
    private static Emotion _latestRequestedEmotion = Emotion.Neutral;

    /// <summary>
    /// 應用程式主入口點。
    /// </summary>
    /// <param name="args">命令列參數。</param>
    /// <returns>結束代碼：0 成功，1 失敗。</returns>
    /// <remarks>
    /// 支援的參數：
    /// --target {Lcd|DryRun}：選用，預設為 Lcd。
    /// --framebuffer {路徑}：選用，預設為 /dev/fb0。
    /// --output {路徑}：選用，DryRun 模式的 PNG 輸出基底路徑。
    /// </remarks>
    public static int Main(string[] args)
    {
        if (!ParseArgs(args))
        {
            return 1;
        }

        Console.WriteLine("=== EmoLCD GPIO 互動模式 ===");
        Console.WriteLine($"輸出目標: {_targetMode}");
        Console.WriteLine("按鈕按下 → 笑臉 (Smile)");
        Console.WriteLine("按鈕放開 → 中性表情 (Neutral)");
        Console.WriteLine("按 Ctrl+C 結束程式");
        Console.WriteLine();

        // 初始化渲染管線
        _pipeline = new RenderPipeline();
        try
        {
            _displayTarget = CreateDisplayTarget(_targetMode);
        }
        catch (FramebufferContractException ex)
        {
            Console.Error.WriteLine($"Framebuffer 契約驗證失敗：{ex.Message}");
            Console.Error.WriteLine("請確認 LCD 已正確設定，或使用 --target DryRun 測試。");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"初始化失敗：{ex.Message}");
            return 1;
        }

        // 初始化 GPIO 監聽器
        using var monitor = new GpioButtonMonitor();
        using var cts = new CancellationTokenSource();

        // 註冊事件
        monitor.OnPressed += () => RequestRender(Emotion.Smile);
        monitor.OnReleased += () => RequestRender(Emotion.Neutral);

        // 處理 Ctrl+C
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("\n正在停止...");
            cts.Cancel();
        };

        try
        {
            // 讀取初始狀態並顯示對應表情
            var isPressed = GpioButtonMonitor.ReadCurrentState();
            var initialEmotion = isPressed ? Emotion.Smile : Emotion.Neutral;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 初始狀態: {(isPressed ? "按下" : "放開")} → {initialEmotion}");
            
            // 設定初始狀態
            lock (StateLock)
            {
                _latestRequestedEmotion = initialEmotion;
                _currentRenderedEmotion = initialEmotion;
            }
            RenderEmotion(initialEmotion);

            Console.WriteLine("\n開始監聽按鈕事件...\n");

            // 開始監聽（此方法會阻塞直到取消）
            monitor.Start(cts.Token);
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"錯誤：{ex.Message}");
            return 1;
        }
        catch (OperationCanceledException)
        {
            // 正常結束
        }

        Console.WriteLine("程式已結束");
        return 0;
    }

    /// <summary>
    /// 請求渲染指定表情。此方法不會阻塞，會將請求排入佇列。
    /// </summary>
    private static void RequestRender(Emotion emotion)
    {
        lock (StateLock)
        {
            _latestRequestedEmotion = emotion;
            if (!_isRendering)
            {
                _isRendering = true;
                Task.Run(RenderLoop);
            }
        }
    }

    /// <summary>
    /// 背景渲染迴圈，確保總是渲染最新的請求。
    /// </summary>
    private static void RenderLoop()
    {
        while (true)
        {
            Emotion emotionToRender;
            lock (StateLock)
            {
                emotionToRender = _latestRequestedEmotion;
            }

            // 如果需要渲染的表情與當前不同，則執行渲染
            // 注意：這裡可以加入邏輯決定是否要強制重繪，目前假設只有變更才重繪
            // 但為了確保狀態一致，若有請求進來通常代表有事件，我們檢查是否與"上一次渲染完成的"不同
            // 或者簡單點，只要進來就渲染，但避免重複渲染相同表情太頻繁？
            // 題目問題是"連續觸發同樣的表情"，所以如果 _latestRequestedEmotion == _currentRenderedEmotion，我們可以跳過
            
            if (emotionToRender != _currentRenderedEmotion)
            {
                RenderEmotion(emotionToRender);
                _currentRenderedEmotion = emotionToRender;
            }

            lock (StateLock)
            {
                // 如果最新的請求已經被處理（即等於我們剛渲染的，或是我們決定不渲染的），則結束迴圈
                if (_latestRequestedEmotion == _currentRenderedEmotion)
                {
                    _isRendering = false;
                    return;
                }
                // 否則，繼續迴圈處理新的請求
            }
        }
    }

    /// <summary>
    /// 渲染指定表情到輸出目標。
    /// </summary>
    private static void RenderEmotion(Emotion emotion)
    {
        lock (RenderLock)
        {
            try
            {
                var request = new RenderRequest
                {
                    Emotion = emotion,
                    Target = _targetMode,
                    FramebufferPath = _framebufferPath,
                    OutputPath = GetOutputPath(emotion)
                };

                var result = _pipeline!.Render(request, _displayTarget!);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 渲染 {emotion}，耗時 {result.DurationMs}ms");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 渲染錯誤：{ex.Message}");
            }
        }
    }

    /// <summary>
    /// 取得 DryRun 模式的輸出路徑。
    /// </summary>
    private static string GetOutputPath(Emotion emotion)
    {
        if (_targetMode == RenderTarget.Lcd)
        {
            return _framebufferPath;
        }

        if (!string.IsNullOrWhiteSpace(_outputPath))
        {
            var dir = Path.GetDirectoryName(_outputPath) ?? ".";
            var name = Path.GetFileNameWithoutExtension(_outputPath);
            var ext = Path.GetExtension(_outputPath);
            if (string.IsNullOrEmpty(ext)) ext = ".png";
            return Path.Combine(dir, $"{name}_{emotion}{ext}");
        }

        return Path.Combine(Path.GetTempPath(), $"gpio_emotion_{emotion}.png");
    }

    /// <summary>
    /// 建立輸出目標實例。
    /// </summary>
    private static IDisplayTarget CreateDisplayTarget(RenderTarget target) => target switch
    {
        RenderTarget.Lcd => new FramebufferDisplayTarget(),
        RenderTarget.DryRun => new DryRunDisplayTarget(),
        _ => throw new ArgumentOutOfRangeException(nameof(target))
    };

    /// <summary>
    /// 解析命令列參數。
    /// </summary>
    private static bool ParseArgs(string[] args)
    {
        _targetMode = RenderTarget.Lcd; // 預設為 LCD

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--target":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("--target 需要指定值 (Lcd 或 DryRun)");
                        return false;
                    }
                    i++;
                    if (!Enum.TryParse<RenderTarget>(args[i], true, out var target))
                    {
                        Console.Error.WriteLine($"不支援的 target 值：{args[i]}");
                        return false;
                    }
                    _targetMode = target;
                    break;

                case "--framebuffer":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("--framebuffer 需要指定路徑");
                        return false;
                    }
                    i++;
                    _framebufferPath = args[i];
                    break;

                case "--output":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("--output 需要指定路徑");
                        return false;
                    }
                    i++;
                    _outputPath = args[i];
                    break;

                case "--help":
                case "-h":
                    PrintUsage();
                    return false;

                default:
                    Console.Error.WriteLine($"未知參數：{args[i]}");
                    PrintUsage();
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 印出使用說明。
    /// </summary>
    private static void PrintUsage()
    {
        Console.WriteLine("用法: EmoLcd.GpioApp [選項]");
        Console.WriteLine();
        Console.WriteLine("選項:");
        Console.WriteLine("  --target {Lcd|DryRun}   輸出目標（預設: Lcd）");
        Console.WriteLine("  --framebuffer {路徑}    Framebuffer 裝置路徑（預設: /dev/fb0）");
        Console.WriteLine("  --output {路徑}         DryRun 輸出 PNG 路徑");
        Console.WriteLine("  --help, -h              顯示此說明");
    }
}
