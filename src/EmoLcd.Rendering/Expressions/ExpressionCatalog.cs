using EmoLcd.Domain.Models;

namespace EmoLcd.Rendering.Expressions;

public record ExpressionShape(
    (int X, int Y) LeftEye,
    (int X, int Y) RightEye,
    (int X, int Y) MouthStart,
    (int X, int Y) MouthEnd,
    int MouthCurveOffset,
    ((int X, int Y) Start, (int X, int Y) End)? LeftBrow = null,
    ((int X, int Y) Start, (int X, int Y) End)? RightBrow = null);

public static class ExpressionCatalog
{
    private static readonly Dictionary<Emotion, ExpressionShape> Shapes = new()
    {
        [Emotion.Neutral] = new ExpressionShape(
            LeftEye: (160, 140),
            RightEye: (320, 140),
            MouthStart: (160, 220),
            MouthEnd: (320, 220),
            MouthCurveOffset: 0),
        [Emotion.Smile] = new ExpressionShape(
            LeftEye: (160, 140),
            RightEye: (320, 140),
            MouthStart: (150, 240),
            MouthEnd: (330, 240),
            MouthCurveOffset: 50),
        [Emotion.Angry] = new ExpressionShape(
            LeftEye: (160, 140),
            RightEye: (320, 140),
            MouthStart: (150, 250),
            MouthEnd: (330, 250),
            MouthCurveOffset: -70,
            LeftBrow: ((120, 120), (200, 145)),   // 向中心下斜
            RightBrow: ((350, 120), (270, 145)))  // 向中心下斜
    };

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

    public static Emotion ParseOrDefault(string value)
    {
        return Enum.TryParse<Emotion>(value, true, out var e) ? e : Emotion.Neutral;
    }
}
