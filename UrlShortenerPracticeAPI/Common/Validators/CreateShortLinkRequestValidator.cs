namespace UrlShortenerPractice.Common.Validators
{
    /// <summary>
    /// 建立短網址的驗證規則
    /// </summary>
    public class CreateShortLinkRequestValidator : AbstractValidator<CreateShortLinkRequest>
    {
        public CreateShortLinkRequestValidator()
        {
            RuleFor(x => x.OriginalUrl)
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
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                && !string.IsNullOrEmpty(uri.Host);
        }
    }
}
