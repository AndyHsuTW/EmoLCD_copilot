namespace EmoLcd.Rendering.Framebuffer;

/// <summary>
/// 當 framebuffer 契約驗證失敗時拋出的例外。
/// </summary>
/// <remarks>
/// 常見情境包括：framebuffer 路徑無效、幾何資料檔案不存在、
/// 格式無法解析、或實際像素資料與契約不符。
/// </remarks>
public class FramebufferContractException : Exception
{
    /// <summary>
    /// 初始化 <see cref="FramebufferContractException"/> 的新實例。
    /// </summary>
    /// <param name="message">描述錯誤的訊息。</param>
    public FramebufferContractException(string message) : base(message)
    {
    }

    /// <summary>
    /// 初始化 <see cref="FramebufferContractException"/> 的新實例，並包含內部例外。
    /// </summary>
    /// <param name="message">描述錯誤的訊息。</param>
    /// <param name="innerException">導致此例外的內部例外。</param>
    public FramebufferContractException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
