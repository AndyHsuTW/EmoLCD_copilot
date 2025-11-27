using EmoLcd.Domain.Models;
using EmoLcd.Rendering;

namespace EmoLcd.App;

public static class Program
{
    private static readonly string[] AllowedEmotions = Enum.GetNames<Emotion>();

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
            RenderResult result = request.Target switch
            {
                RenderTarget.Lcd => pipeline.RenderToFramebuffer(request),
                RenderTarget.DryRun => pipeline.RenderToFile(request),
                _ => throw new ArgumentOutOfRangeException(nameof(request.Target))
            };

            Console.WriteLine($"完成：{result.Target} 輸出，表情 {result.Emotion}，耗時 {result.DurationMs}ms，位置 {result.OutputPath}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"錯誤：{ex.Message}");
            return 1;
        }
    }

    private static ParseResult ParseArgs(string[] args)
    {
        Emotion emotion = Emotion.Neutral;
        RenderTarget target = RenderTarget.Lcd;
        string framebuffer = "/dev/fb0";
        string output = "/tmp/emotion.png";
        bool emotionProvided = false;
        bool targetProvided = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--emotion":
                    if (i + 1 < args.Length)
                    {
                        emotionProvided = true;
                        var value = args[i + 1];
                        if (!Enum.TryParse<Emotion>(value, true, out var e))
                        {
                            return ParseResult.Fail($"不支援的表情：{value}", showAllowed: true);
                        }
                        emotion = e;
                        i++;
                    }
                    break;
                case "--target":
                    if (i + 1 < args.Length)
                    {
                        targetProvided = true;
                        var value = args[i + 1];
                        if (!Enum.TryParse<RenderTarget>(value, true, out var t))
                        {
                            return ParseResult.Fail($"不支援的輸出目標：{value}", showAllowed: false);
                        }
                        target = t;
                        i++;
                    }
                    break;
                case "--framebuffer":
                    if (i + 1 < args.Length)
                    {
                        framebuffer = args[i + 1];
                        i++;
                    }
                    break;
                case "--output":
                    if (i + 1 < args.Length)
                    {
                        output = args[i + 1];
                        i++;
                    }
                    break;
            }
        }

        // 若使用者未指定 target，維持預設 Lcd；dry-run 需 output，已設預設路徑
        return ParseResult.Success(emotion, target, framebuffer, output, emotionProvided, targetProvided);
    }

    private readonly record struct ParseResult(
        bool Ok,
        Emotion Emotion,
        RenderTarget Target,
        string Framebuffer,
        string Output,
        bool ShowAllowed,
        string? ErrorMessage)
    {
        public static ParseResult Success(
            Emotion emotion,
            RenderTarget target,
            string framebuffer,
            string output,
            bool emotionProvided,
            bool targetProvided) =>
            new(true, emotion, target, framebuffer, output, false, null);

        public static ParseResult Fail(string message, bool showAllowed) =>
            new(false, Emotion.Neutral, RenderTarget.Lcd, "/dev/fb0", "/tmp/emotion.png", showAllowed, message);
    }
}
