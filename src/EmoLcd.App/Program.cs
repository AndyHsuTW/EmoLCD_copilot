using System.IO;
using EmoLcd.Domain.Models;
using EmoLcd.Rendering;
using EmoLcd.Rendering.Display;
using EmoLcd.Rendering.Framebuffer;

namespace EmoLcd.App;

/// <summary>
/// EmoLCD 應用程式的主入口點類別。
/// </summary>
/// <remarks>
/// 負責解析命令列參數、建立顯示目標、執行表情渲染流程，
/// 並處理例外狀況與降級輸出（fallback）。
/// </remarks>
public static class Program
{
    /// <summary>
    /// 允許的表情名稱陣列，用於顯示提示訊息。
    /// </summary>
    private static readonly string[] AllowedEmotions = Enum.GetNames<Emotion>();

    /// <summary>
    /// 應用程式主入口點。
    /// </summary>
    /// <param name="args">命令列參數陣列。</param>
    /// <returns>
    /// 結束代碼：0 表示成功，1 表示失敗。
    /// </returns>
    /// <remarks>
    /// 支援的參數：
    /// --emotion {表情名稱}：必要，指定要渲染的表情（Smile、Angry、Neutral 等）。
    /// --target {Lcd|DryRun}：必要，指定輸出目標。
    /// --framebuffer {路徑}：選用，預設為 /dev/fb0。
    /// --output {路徑}：選用，DryRun 模式的 PNG 輸出路徑。
    /// 
    /// 當 LCD 模式發生 framebuffer 契約驗證失敗時，會自動降級為 DryRun 輸出 PNG。
    /// </remarks>
    public static int Main(string[] args)
    {
        var parse = ParseArgs(args);
        if (!parse.Ok)
        {
            Console.Error.WriteLine(parse.ErrorMessage);
            if (parse.ShowAllowed)
            {
                Console.Error.WriteLine($"允許的表情：{string.Join(", ", AllowedEmotions)}");
            }
            return 1;
        }

        var pipeline = new RenderPipeline();
        var request = new RenderRequest
        {
            Emotion = parse.Emotion,
            Target = parse.Target,
            FramebufferPath = parse.Framebuffer,
            OutputPath = parse.Output
        };

        try
        {
            var displayTarget = CreateDisplayTarget(request.Target);
            var result = pipeline.Render(request, displayTarget);
            Console.WriteLine($"完成：{result.Target} 輸出，表情 {result.Emotion}，耗時 {result.DurationMs}ms，位置 {result.OutputPath}");
            return 0;
        }
        catch (FramebufferContractException ex) when (request.Target == RenderTarget.Lcd)
        {
            Console.Error.WriteLine($"Framebuffer 契約驗證失敗：{ex.Message}");
            Console.Error.WriteLine("將改以 dry-run 輸出 PNG 以協助除錯。");

            var fallbackPath = request.OutputPath;
            if (string.IsNullOrWhiteSpace(fallbackPath))
            {
                fallbackPath = Path.Combine(Path.GetTempPath(), $"emotion_{Guid.NewGuid():N}.png");
            }

            var fallbackRequest = new RenderRequest
            {
                Emotion = request.Emotion,
                Target = RenderTarget.DryRun,
                OutputPath = fallbackPath,
                FramebufferPath = request.FramebufferPath,
                Width = request.Width,
                Height = request.Height
            };

            var fallbackResult = pipeline.Render(fallbackRequest, new DryRunDisplayTarget());
            Console.WriteLine($"Fallback 已完成，請檢查 {fallbackResult.OutputPath}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"錯誤：{ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// 根據指定的輸出目標建立對應的 <see cref="IDisplayTarget"/> 實例。
    /// </summary>
    /// <param name="target">輸出目標類型。</param>
    /// <returns>對應的顯示目標實例。</returns>
    /// <exception cref="ArgumentOutOfRangeException">當 target 為未定義的值時拋出。</exception>
    private static IDisplayTarget CreateDisplayTarget(RenderTarget target) => target switch
    {
        RenderTarget.Lcd => new FramebufferDisplayTarget(),
        RenderTarget.DryRun => new DryRunDisplayTarget(),
        _ => throw new ArgumentOutOfRangeException(nameof(target))
    };

    /// <summary>
    /// 解析命令列參數並驗證其有效性。
    /// </summary>
    /// <param name="args">命令列參數陣列。</param>
    /// <returns>
    /// 包含解析結果的 <see cref="ParseResult"/>，其中 Ok 屬性表示是否成功。
    /// </returns>
    /// <remarks>
    /// 此方法會驗證：
    /// - 必要參數是否提供（--emotion、--target）
    /// - 表情名稱是否有效
    /// - framebuffer 路徑是否存在且可寫入（LCD 模式）
    /// - 輸出目錄是否存在（DryRun 模式）
    /// </remarks>
    internal static ParseResult ParseArgs(string[] args)
    {
        Emotion? emotion = null;
        RenderTarget? target = null;
        string framebuffer = "/dev/fb0";
        string output = Path.Combine(Path.GetTempPath(), "emotion.png");

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--emotion":
                    if (!TryGetValue(args, ref i, out var emotionValue))
                    {
                        return ParseResult.Fail("--emotion 需要指定值", showAllowed: true);
                    }
                    if (!Enum.TryParse<Emotion>(emotionValue, true, out var e))
                    {
                        return ParseResult.Fail($"不支援的表情：{emotionValue}", showAllowed: true);
                    }
                    emotion = e;
                    break;
                case "--target":
                    if (!TryGetValue(args, ref i, out var targetValue))
                    {
                        return ParseResult.Fail("--target 需要指定值", showAllowed: false);
                    }
                    if (!Enum.TryParse<RenderTarget>(targetValue, true, out var t))
                    {
                        return ParseResult.Fail($"不支援的輸出目標：{targetValue}", showAllowed: false);
                    }
                    target = t;
                    break;
                case "--framebuffer":
                    if (!TryGetValue(args, ref i, out var framebufferValue))
                    {
                        return ParseResult.Fail("--framebuffer 需要指定值", showAllowed: false);
                    }
                    framebuffer = framebufferValue;
                    break;
                case "--output":
                    if (!TryGetValue(args, ref i, out var outputValue))
                    {
                        return ParseResult.Fail("--output 需要指定值", showAllowed: false);
                    }
                    output = outputValue;
                    break;
            }
        }

        if (emotion is null)
        {
            return ParseResult.Fail("缺少必要參數 --emotion", showAllowed: true);
        }

        if (target is null)
        {
            return ParseResult.Fail("缺少必要參數 --target", showAllowed: false);
        }

        if (target == RenderTarget.Lcd)
        {
            if (!TryValidateFramebuffer(framebuffer, out var fbError))
            {
                return ParseResult.Fail(fbError, showAllowed: false);
            }
        }
        else if (target == RenderTarget.DryRun)
        {
            if (!TryValidateOutput(output, out var outputError))
            {
                return ParseResult.Fail(outputError, showAllowed: false);
            }
        }

        return ParseResult.Success(emotion.Value, target.Value, framebuffer, output);
    }

    /// <summary>
    /// 嘗試從參數陣列中取得下一個值。
    /// </summary>
    /// <param name="args">參數陣列。</param>
    /// <param name="index">目前的索引位置，成功時會自動遞增。</param>
    /// <param name="value">取得的值，失敗時為空字串。</param>
    /// <returns>成功取得值時回傳 true。</returns>
    private static bool TryGetValue(string[] args, ref int index, out string value)
    {
        if (index + 1 >= args.Length)
        {
            value = string.Empty;
            return false;
        }

        value = args[index + 1];
        index++;
        return true;
    }

    /// <summary>
    /// 驗證 framebuffer 路徑是否有效且可寫入。
    /// </summary>
    /// <param name="path">framebuffer 裝置路徑。</param>
    /// <param name="error">驗證失敗時的錯誤訊息。</param>
    /// <returns>驗證成功時回傳 true。</returns>
    /// <remarks>
    /// 驗證項目：路徑非空、檔案存在、具有寫入權限。
    /// </remarks>
    private static bool TryValidateFramebuffer(string path, out string error)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            error = "framebuffer 路徑不可為空";
            return false;
        }

        if (!File.Exists(path))
        {
            error = $"找不到 framebuffer：{path}";
            return false;
        }

        try
        {
            using var stream = File.Open(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
            error = string.Empty;
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            error = $"framebuffer 無法寫入：權限不足 - {path}";
            return false;
        }
        catch (Exception ex)
        {
            error = $"framebuffer 無法寫入：{ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// 驗證輸出路徑是否有效。
    /// </summary>
    /// <param name="path">輸出檔案路徑。</param>
    /// <param name="error">驗證失敗時的錯誤訊息。</param>
    /// <returns>驗證成功時回傳 true。</returns>
    /// <remarks>
    /// 驗證項目：路徑非空、父目錄存在。
    /// </remarks>
    private static bool TryValidateOutput(string path, out string error)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            error = "輸出路徑不可為空";
            return false;
        }

        var directory = Path.GetDirectoryName(path);
        var effectiveDirectory = string.IsNullOrWhiteSpace(directory) ? "." : directory;
        if (!Directory.Exists(effectiveDirectory))
        {
            error = $"輸出資料夾不存在：{effectiveDirectory}";
            return false;
        }

        error = string.Empty;
        return true;
    }

    /// <summary>
    /// 命令列參數解析結果。
    /// </summary>
    /// <param name="Ok">解析是否成功。</param>
    /// <param name="Emotion">解析出的表情類型。</param>
    /// <param name="Target">解析出的輸出目標。</param>
    /// <param name="Framebuffer">framebuffer 裝置路徑。</param>
    /// <param name="Output">輸出檔案路徑。</param>
    /// <param name="ShowAllowed">是否顯示允許的表情列表。</param>
    /// <param name="ErrorMessage">錯誤訊息，成功時為 null。</param>
    internal readonly record struct ParseResult(
        bool Ok,
        Emotion Emotion,
        RenderTarget Target,
        string Framebuffer,
        string Output,
        bool ShowAllowed,
        string? ErrorMessage)
    {
        /// <summary>
        /// 建立成功的解析結果。
        /// </summary>
        /// <param name="emotion">解析出的表情類型。</param>
        /// <param name="target">解析出的輸出目標。</param>
        /// <param name="framebuffer">framebuffer 裝置路徑。</param>
        /// <param name="output">輸出檔案路徑。</param>
        /// <returns>表示成功的 <see cref="ParseResult"/>。</returns>
        public static ParseResult Success(
            Emotion emotion,
            RenderTarget target,
            string framebuffer,
            string output) =>
            new(true, emotion, target, framebuffer, output, false, null);

        /// <summary>
        /// 建立失敗的解析結果。
        /// </summary>
        /// <param name="message">錯誤訊息。</param>
        /// <param name="showAllowed">是否顯示允許的表情列表。</param>
        /// <returns>表示失敗的 <see cref="ParseResult"/>。</returns>
        public static ParseResult Fail(string message, bool showAllowed) =>
            new(false, Emotion.Neutral, RenderTarget.Lcd, "/dev/fb0", "/tmp/emotion.png", showAllowed, message);
    }
}
