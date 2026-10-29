using UrlShortenerPractice.Common.Request;
using UrlShortenerPractice.Common.Responses;

namespace BookingPractice.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<ProblemDetails>))]
[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<Dictionary<string, string[]>>))]
public class ShortLinkController(IShortLinkService shortLinkService) : ControllerBase
{
    /// <summary>
    /// 建立短網址
    /// </summary>
    /// <param name="request">建立請求</param>
    /// <returns>短網址</returns>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ShortLinkResponse>))]
    public async Task<IActionResult> CreateShortLink([FromBody] CreateShortLinkRequest request)
    {
        return Ok(await shortLinkService.CreateShortLink(request));
    }

    /// <summary>
    /// 依短碼轉原網址 , 給 Get 路徑從 「 api/ShortLink/ 」 改成 「 /code 」 , 這樣才能保持原本的短網址
    /// </summary>
    /// <param name="code">短碼</param>
    /// <returns>原網址</returns>
    [HttpGet("/{code:regex(^[[A-Za-z0-9]]{{6,16}}$)}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RedirectToOriginal(string code)
    {
        var result = await shortLinkService.GetOriginalUrl(code);

        if (string.IsNullOrEmpty(result?.ReturnData))
        {
            return NotFound();
        }

        return Redirect(result.ReturnData);
    }
}
