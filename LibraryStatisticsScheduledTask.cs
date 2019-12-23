﻿using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
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
		public LibraryStatisticsScheduledTask(ILogger logger, ILibraryManager libraryManager)
		{
			this.Logger = logger;
			this.LibraryManager = libraryManager;
		}

		private readonly ILogger Logger;
		private readonly ILibraryManager LibraryManager;

		public string Name => "Library statistics calculation";

		public string Description => "Calculates library statistics.";

		public string Category => "Library";
		
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
				this.Logger.LogInformation("Jellyfin.Plugin.LibraryStatistics: Recalculating library statistics newest item date");
				var newestItemQuery = new InternalItemsQuery()
				{
					OrderBy = new ValueTuple<string, SortOrder>[] { new ValueTuple<string, SortOrder>(ItemSortBy.DateCreated, SortOrder.Descending) },
					Recursive = true,
					IsMissing = false,
					Limit = 1,
					SourceTypes = new[] { SourceType.Library }
				};
				Plugin.Instance.LibraryStatistics.NewestItemDate = this.LibraryManager.GetItemsResult(newestItemQuery).Items.First().DateCreated;

				if (cancellationToken.IsCancellationRequested)
				{
					return Task.CompletedTask;
				}

				// Total file size
				progress.Report(60);
				this.Logger.LogInformation("Jellyfin.Plugin.LibraryStatistics: Recalculating library statistics total file size");
				var totalFileSizeQuery = new InternalItemsQuery()
				{
					Recursive = true,
					SourceTypes = new[] { SourceType.Library },
					IsMissing = false
				};

				long totalFileSize = 0;
				long totalFileSizeWithRedundancy = 0;
				foreach (var item in this.LibraryManager.GetItemsResult(totalFileSizeQuery).Items)
				{
					if (File.Exists(item.Path))
					{
						FileInfo fileInfo = new FileInfo(item.Path);
						totalFileSize += fileInfo.Length;
						totalFileSizeWithRedundancy += fileInfo.Length;

						if (!item.Path.Contains(@"\ForeignMedia\"))
						{
							totalFileSizeWithRedundancy += fileInfo.Length;
						}

						// Also cache this in item here to reduce filesystem access, see also Emby.Server.Implementations\Dto\DtoService.cs
						if ((item.Size == null || item.Size == 0))
						{
							item.Size = fileInfo.Length;
						}
					}
				}
				Plugin.Instance.LibraryStatistics.TotalFileSize = totalFileSize;
				Plugin.Instance.LibraryStatistics.TotalFileSizeWithRedundancy = totalFileSizeWithRedundancy;

				if (cancellationToken.IsCancellationRequested)
				{
					return Task.CompletedTask;
				}

				// Total run time ticks
				progress.Report(90);
				this.Logger.LogInformation("Jellyfin.Plugin.LibraryStatistics: Recalculating library statistics total run time ticks");
				var totalRunTimeTicksQuery = new InternalItemsQuery()
				{
					Recursive = true,
					SourceTypes = new[] { SourceType.Library },
					IsMissing = false
				};
				Plugin.Instance.LibraryStatistics.TotalRunTimeTicks = this.LibraryManager.GetItemsResult(totalRunTimeTicksQuery).Items.Sum(x => x.RunTimeTicks);
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
