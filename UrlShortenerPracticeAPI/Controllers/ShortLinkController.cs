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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
    public async Task<IActionResult> CreateShortLink([FromBody] CreateShortLinkRequest request)
    {
        return Ok(await shortLinkService.CreateShortLink(request));
    }
}
