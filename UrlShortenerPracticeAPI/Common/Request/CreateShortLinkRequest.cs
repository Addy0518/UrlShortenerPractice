namespace UrlShortenerPractice.Common.Request
{
    public class CreateShortLinkRequest
    {
        /// <summary>
        /// 原網址 , 限制 2048 字元
        /// </summary>
        public string OriginalUrl { get; set; }
    }
}
