namespace UrlShortenerPractice.Common.Responses
{
    public class ShortLinkResponse
    {
        /// <summary>
        /// 短碼 , 唯一索引 , 轉址時就靠它查
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 短網址
        /// </summary>
        public string ShortUrl { get; set; }
    }
}
