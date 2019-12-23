using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Services;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LibraryStatistics.Api
{
    [Route("/LibraryStatistics", "GET", Summary = "Provides library statistic")]
    [Authenticated]
    public class GetLibraryStatistics : IReturn<LibraryStatisticsBag>
    { }

    public class LibraryStatisticsService : IService
    {
        private readonly ILogger Logger;

        public LibraryStatisticsService(ILogger logger)
        {
            this.Logger = logger;
        }

        public LibraryStatisticsBag Get(GetLibraryStatistics request)
        {
            return Plugin.Instance.LibraryStatistics;
        }
    }
}
