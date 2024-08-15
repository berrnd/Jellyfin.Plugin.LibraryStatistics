using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;

namespace Jellyfin.Plugin.LibraryStatistics.Api
{
	[ApiController]
	[Route("LibraryStatistics")]
	[Authorize]
	[Produces(MediaTypeNames.Application.Json)]
	public class LibraryStatisticsController : ControllerBase
	{
		public LibraryStatisticsController(ILogger<LibraryStatisticsController> logger)
		{
			this.Logger = logger;
		}

		private readonly ILogger<LibraryStatisticsController> Logger;

		[HttpGet("stats")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public ActionResult<LibraryStatisticsBag> GetLibraryStatistics()
		{
			return Ok(Plugin.Instance.LibraryStatistics);
		}
	}
}
