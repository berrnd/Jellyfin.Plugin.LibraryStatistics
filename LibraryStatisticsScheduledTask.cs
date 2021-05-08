using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.LibraryStatistics
{
	public class LibraryStatisticsScheduledTask : IScheduledTask, IConfigurableScheduledTask
	{
		public LibraryStatisticsScheduledTask(ILogger<LibraryStatisticsScheduledTask> logger, ILibraryManager libraryManager)
		{
			this.Logger = logger;
			this.LibraryManager = libraryManager;
		}

		private readonly ILogger<LibraryStatisticsScheduledTask> Logger;
		private readonly ILibraryManager LibraryManager;

		public string Name => "Library statistics calculation";

		public string Description => "Calculates library statistics.";

		public string Category => "Jellyfin.Plugin.LibraryStatistics";
		
		public string Key => "LibraryStatistics";

		public bool IsHidden => false;

		public bool IsEnabled => true;

		public bool IsLogged => true;

		public Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
		{
			if (Plugin.Instance.LibraryStatistics == null || Plugin.Instance.LibraryStatistics.NeedsRecalculation)
			{
				// Newest item date
				progress.Report(30);
				this.Logger.LogInformation("Jellyfin.Plugin.LibraryStatistics: Recalculating NewestItemDate");
				var newestItemQuery = new InternalItemsQuery()
				{
					SourceTypes = new[] { SourceType.Library },
					IncludeItemTypes = new[] { typeof(Movie).Name, typeof(Episode).Name },
					IsMissing = false,
					OrderBy = new ValueTuple<string, SortOrder>[] { new ValueTuple<string, SortOrder>(ItemSortBy.DateCreated, SortOrder.Descending) },
					Limit = 1
				};
				Plugin.Instance.LibraryStatistics.NewestItemDate = this.LibraryManager.GetItemsResult(newestItemQuery).Items.First().DateCreated;

				if (cancellationToken.IsCancellationRequested)
				{
					return Task.CompletedTask;
				}

				// Total file size
				progress.Report(60);
				this.Logger.LogInformation("Jellyfin.Plugin.LibraryStatistics: Recalculating TotalFileSize");
				var totalFileSizeQuery = new InternalItemsQuery()
				{
					SourceTypes = new[] { SourceType.Library },
					IncludeItemTypes = new[] { typeof(Movie).Name, typeof(Episode).Name },
					IsMissing = false
				};

				long totalFileSize = 0;
				foreach (var item in this.LibraryManager.GetItemsResult(totalFileSizeQuery).Items)
				{
					if (File.Exists(item.Path))
					{
						FileInfo fileInfo = new FileInfo(item.Path);
						totalFileSize += fileInfo.Length;

						// Also cache this in item here to reduce filesystem access, see also Emby.Server.Implementations\Dto\DtoService.cs
						if ((item.Size == null || item.Size == 0))
						{
							item.Size = fileInfo.Length;
						}
					}
				}
				Plugin.Instance.LibraryStatistics.TotalFileSize = totalFileSize;

				if (cancellationToken.IsCancellationRequested)
				{
					return Task.CompletedTask;
				}

				// Total run time ticks
				progress.Report(90);
				this.Logger.LogInformation("Jellyfin.Plugin.LibraryStatistics: Recalculating TotalRunTimeMinutes");
				var totalRunTimeTicksQuery = new InternalItemsQuery()
				{
					SourceTypes = new[] { SourceType.Library },
					IncludeItemTypes = new[] { typeof(Movie).Name, typeof(Episode).Name },
					IsMissing = false
				};
				Plugin.Instance.LibraryStatistics.TotalRunTimeMinutes = TimeSpan.FromTicks(this.LibraryManager.GetItemsResult(totalRunTimeTicksQuery).Items.Sum(x => x.RunTimeTicks).GetValueOrDefault()).TotalMinutes;
			}

			return Task.CompletedTask;
		}

		public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
		{
			return new TaskTriggerInfo[]
			{
				new TaskTriggerInfo { Type = TaskTriggerInfo.TriggerInterval, IntervalTicks = TimeSpan.FromDays(1).Ticks },
				new TaskTriggerInfo { Type = TaskTriggerInfo.TriggerStartup }
			};
		}
	}
}
