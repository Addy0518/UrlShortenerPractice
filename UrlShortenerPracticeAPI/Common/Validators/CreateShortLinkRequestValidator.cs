namespace UrlShortenerPractice.Common.Validators
{
    /// <summary>
    /// 建立短網址的驗證規則
    /// </summary>
    public class CreateShortLinkRequestValidator : AbstractValidator<CreateShortLinkRequest>
    {
        public CreateShortLinkRequestValidator()
        {
            // RuleFor:針對 OriginalUrl 這個屬性,依序套用以下規則
            RuleFor(x => x.OriginalUrl)
                // Cascade(Stop):遇到第一個失敗的規則就停止,不再檢查後面的 , 沒加的話,輸入空字串會同時觸發「不為空」「格式」「網址結構」好幾個錯誤
                // 加了之後只回報第一個 , 使用者一次只看到一條訊息
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("請輸入網址！")
                .MaximumLength(2048)
                .WithMessage("網址長度不可超過 2048 字元！")
                .Matches(@"^https?://\S+\z", RegexOptions.IgnoreCase)
                .WithMessage("網址必須以 http:// 或 https:// 開頭，且不可包含空白！")
                .Must(BeValidHttpUrl)
                .WithMessage("網址格式不正確！");
        }

        /// <summary>
        /// 確認是結構完整的 http 或 https 網址
        /// </summary>
        /// <param name="url">原網址</param>
        /// <returns>是否合法</returns>
        private static bool BeValidHttpUrl(string url)
        {
            // Uri.TryCreate : 嘗試把字串解析成 Uri 物件塞到 uri 變數 , UriKind.Absolute 則是規定 url 必須包含協定 ( https://... )
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                // uri.Scheme 會再一次把 Uri 轉成小寫 , 這裡是在確認一次只有 http 或 https
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                // 並確認有沒有包含網域 ( "http://" 這種沒有網域的 )
                && !string.IsNullOrEmpty(uri.Host);
        }
    }
}
