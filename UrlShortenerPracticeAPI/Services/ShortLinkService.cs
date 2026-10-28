using Microsoft.Extensions.Options;
using UrlShortenerPractice.Common.Request;
using UrlShortenerPractice.Common.Responses;
using UrlShortenerPractice.Infrastructures.Options;

namespace UrlShortenerPractice.API.Services;

public class ShortLinkService(UrlShortenerPracticeContext context, IOptions<ShortLinkOptions> options)
    : IShortLinkService
{
    /// <summary>
    /// 短碼可用的字元(英數字,區分大小寫)
    /// </summary>
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    /// <summary>
    /// 短碼長度
    /// </summary>
    private const int CodeLength = 7;

    /// <summary>
    /// 撞碼時最多重試幾次
    /// </summary>
    private const int MaxRetry = 5;

    /// <summary>
    /// 建立短網址
    /// </summary>
    /// <param name="request">建立請求</param>
    /// <returns>短網址</returns>
    public async Task<ApiResponse<ShortLinkResponse>> CreateShortLink(CreateShortLinkRequest request)
    {
        for (int i = 0; i <= MaxRetry; i++)
        {
            var link = new ShortLink
            {
                Code = GenerateCode(),
                OriginalUrl = request.OriginalUrl,
                Status = (int)ShortLinkStatusEnum.啟用,
                ClickCount = 0,
                CreatedAt = DateTime.Now,
            };

            context.ShortLinks.Add(link);

            try
            {
                await context.SaveChangesAsync();

                var response = new ShortLinkResponse
                {
                    Code = link.Code,
                    ShortUrl = $"{options.Value.BaseUrl.TrimEnd('/')}/{link.Code}",
                };

                return ApiResponseHelper.Success(response);
            } // DbUpdateException : 資料庫異常
            catch (DbUpdateException ex)
                // 當異常例外是 SqlException ( 資料庫的問題 ) 且錯誤號碼是 2601 或 2627 時，表示違反了唯一性約束 ，代表有人嘗試插入重複的資料 , 就擋下來並報錯誤
                when (ex.InnerException is SqlException s && (s.Number == 2601 || s.Number == 2627))
            {
                context.Entry(link).State = EntityState.Detached;
            }
        }

        var errors = new Dictionary<string, string[]> { { "Code", new[] { "短碼產生失敗，請稍後再試！" } } };
        return ApiResponseHelper.RequestError<ShortLinkResponse>(errors);
    }

    /// <summary>
    /// 產生隨機短碼
    /// </summary>
    /// <returns>短碼</returns>
    private static string GenerateCode()
    {
        return string.Create(
            CodeLength,
            Alphabet,
            (span, alphabet) =>
            {
                for (int i = 0; i < span.Length; i++)
                    span[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
            }
        );
    }
}
