using UrlShortenerPractice.Common.Request;
using UrlShortenerPractice.Common.Responses;

namespace UrlShortenerPractice.API.Services.Interface;

public interface IShortLinkService
{
    /// <summary>
    /// 建立短網址
    /// </summary>
    /// <param name="request">建立請求</param>
    /// <returns>短網址</returns>
    Task<ApiResponse<ShortLinkResponse>> CreateShortLink(CreateShortLinkRequest request);

    /// <summary>
    /// 依短碼取得原網址並累加點擊數
    /// </summary>
    /// <param name="code">短碼</param>
    /// <returns>原網址</returns>
    Task<ApiResponse<string>> GetOriginalUrl(string code);
}
