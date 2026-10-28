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
}
