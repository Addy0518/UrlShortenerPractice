using Microsoft.Extensions.Options;
using UrlShortenerPractice.Common.Request;
using UrlShortenerPractice.Common.Responses;
using UrlShortenerPractice.Infrastructures.Options;

namespace UrlShortenerPractice.API.Services;

public class ShortLinkService(
    UrlShortenerPracticeContext context,
    IOptions<ShortLinkOptions> options,
    IValidator<CreateShortLinkRequest> validator
) : IShortLinkService
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
        // ValidateAsync : 拿 Validator 裡的所有規則驗證 request
        var validation = await validator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return ApiResponseHelper.RequestError<ShortLinkResponse>(
                new Dictionary<string, string[]>(validation.ToDictionary())
            );
        }

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
                // 如果失敗了要取消追蹤 , 不然的話就會變成
                // 第一遍轉換短網址失敗 ,  Add ( link1 ) 還是繼續追蹤的 , link1 還在 Add 清單
                // 跑第二遍時 Add ( link2 ) 這時就會有 link1 跟 link2 都在追蹤 , 那就會一直失敗
                context.Entry(link).State = EntityState.Detached;
            }
        }

        var errors = new Dictionary<string, string[]> { { "Code", new[] { "短碼產生失敗，請稍後再試！" } } };
        return ApiResponseHelper.RequestError<ShortLinkResponse>(errors);
    }

    /// <summary>
    /// 依短碼取得原網址並累加點擊數
    /// </summary>
    /// <param name="code">短碼</param>
    /// <returns>原網址</returns>
    public async Task<ApiResponse<string>> GetOriginalUrl(string code)
    {
        // 找到符合條件的網址 , 回傳 「 受影響的筆數 」 , 並增加點擊數 ( ClickCount + 1 )
        // ExecuteUpdateAsync : 直接對符合條件的資料執行 Update
        // s.SetProperty(...) : s 是符合條件的資料清單 , 用 SetProperty 一項一項加進去
        // x => x.ClickCount : 要改 ClickCount 這個欄位
        // x => x.ClickCount + 1 : 要改變的值
        var affected = await context
            .ShortLinks.Where(x =>
                x.Code == code
                && x.Status == (int)ShortLinkStatusEnum.啟用
                && (x.ExpiresAt > DateTime.UtcNow || x.ExpiresAt == null)
            )
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ClickCount, x => x.ClickCount + 1));

        // 所以這一整句其實也等於 SQL 的
        // UPDATE ShortLinks
        // SET ClickCount = ClickCount + 1
        // WHERE Code = @code AND Status = 1 AND(ExpiresAt > GETUTCDATE() OR ExpiresAt IS NULL)

        if (affected == 0)
        {
            var errors = new Dictionary<string, string[]> { { "Code", new[] { "此短網址不存在、已停用或已到期！" } } };
            return ApiResponseHelper.RequestError<string>(errors);
        }

        string uri = await context
            .ShortLinks.Where(x => x.Code == code)
            .Select(x => x.OriginalUrl)
            .FirstOrDefaultAsync();

        return ApiResponseHelper.Success(uri);
    }

    /// <summary>
    /// 產生隨機短碼
    /// </summary>
    /// <returns>短碼</returns>
    private static string GenerateCode()
    {
        // 建立個空間為 7 個字元的變數字串
        var chars = new char[CodeLength];

        // 根據我自訂的字母表隨機拼湊成短碼
        for (int i = 0; i < CodeLength; i++)
        {
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        return new string(chars);
    }
}
