using EmoLcd.Domain.Models;

namespace EmoLcd.Rendering.Expressions;

/// <summary>
/// 表情幾何定義，封裝單一表情在畫布上的所有繪製參數。
/// </summary>
/// <param name="LeftEye">左眼圓心座標（像素）。</param>
/// <param name="RightEye">右眼圓心座標（像素）。</param>
/// <param name="MouthStart">嘴巴起始點座標（像素）。</param>
/// <param name="MouthEnd">嘴巴結束點座標（像素）。</param>
/// <param name="MouthCurveOffset">嘴巴曲線偏移量：正值向下彎（微笑）、負值向上彎（憤怒）、0 為直線。</param>
/// <param name="EyeRadius">眼睛外圓半徑（像素）。</param>
/// <param name="EyeStroke">眼睛圓環線條粗細（像素）。</param>
/// <param name="MouthThickness">嘴巴線條粗細（像素）。</param>
/// <param name="MouthSamples">繪製嘴巴貝茲曲線的取樣點數，數值越大曲線越平滑。</param>
/// <param name="BrowThickness">眉毛線條粗細（像素）。</param>
/// <param name="LeftBrow">左眉毛起始與結束座標，null 表示不繪製眉毛。</param>
/// <param name="RightBrow">右眉毛起始與結束座標，null 表示不繪製眉毛。</param>
public record ExpressionShape(
    (int X, int Y) LeftEye,
    (int X, int Y) RightEye,
    (int X, int Y) MouthStart,
    (int X, int Y) MouthEnd,
    int MouthCurveOffset,
    int EyeRadius,
    int EyeStroke,
    int MouthThickness,
    int MouthSamples,
    int BrowThickness,
    ((int X, int Y) Start, (int X, int Y) End)? LeftBrow = null,
    ((int X, int Y) Start, (int X, int Y) End)? RightBrow = null);

/// <summary>
/// 表情目錄，提供各種 <see cref="Emotion"/> 對應的 <see cref="ExpressionShape"/> 查詢。
/// </summary>
/// <remarks>
/// 此類別作為表情幾何定義的單一來源（Single Source of Truth），
/// 新增表情時只需在此處擴充 <see cref="Shapes"/> 字典即可。
/// </remarks>
public static class ExpressionCatalog
{
    private static readonly Dictionary<Emotion, ExpressionShape> Shapes = new()
    {
        [Emotion.Neutral] = new ExpressionShape(
            LeftEye: (160, 140),
            RightEye: (320, 140),
            MouthStart: (160, 220),
            MouthEnd: (320, 220),
            MouthCurveOffset: 0,
            EyeRadius: 12,
            EyeStroke: 12,
            MouthThickness: 4,
            MouthSamples: 20,
            BrowThickness: 3),
        [Emotion.Smile] = new ExpressionShape(
            LeftEye: (160, 140),
            RightEye: (320, 140),
            MouthStart: (150, 240),
            MouthEnd: (330, 240),
            MouthCurveOffset: 50,
            EyeRadius: 12,
            EyeStroke: 12,
            MouthThickness: 4,
            MouthSamples: 48,
            BrowThickness: 3),
        [Emotion.Angry] = new ExpressionShape(
            LeftEye: (160, 140),
            RightEye: (320, 140),
            MouthStart: (150, 250),
            MouthEnd: (330, 250),
            MouthCurveOffset: -70,
            EyeRadius: 12,
            EyeStroke: 10,
            MouthThickness: 5,
            MouthSamples: 48,
            BrowThickness: 4,
            LeftBrow: ((120, 120), (200, 145)),   // 向中心下斜
            RightBrow: ((350, 120), (270, 145)))  // 向中心下斜
    };

    /// <summary>
    /// 嘗試取得指定表情的幾何定義。
    /// </summary>
    /// <param name="emotion">要查詢的表情類型。</param>
    /// <param name="shape">若找到則回傳對應的 <see cref="ExpressionShape"/>；否則為 default。</param>
    /// <returns>若表情存在於目錄中則回傳 true；否則回傳 false。</returns>
    public static bool TryGet(Emotion emotion, out ExpressionShape shape)
    {
        if (Shapes.TryGetValue(emotion, out var found))
        {
            shape = found;
            return true;
        }

        shape = default!;
        return false;
    }

    /// <summary>
    /// 解析字串為表情類型，解析失敗時回傳 <see cref="Emotion.Neutral"/>。
    /// </summary>
    /// <param name="value">要解析的字串，不區分大小寫。</param>
    /// <returns>解析成功的表情類型，或預設的 <see cref="Emotion.Neutral"/>。</returns>
    public static Emotion ParseOrDefault(string value)
    {
        return Enum.TryParse<Emotion>(value, true, out var e) ? e : Emotion.Neutral;
    }
}
