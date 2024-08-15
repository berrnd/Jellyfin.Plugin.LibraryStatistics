using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.LibraryStatistics
{
	public class JellyfinLibraryStatistics : IHostedService
	{
		public JellyfinLibraryStatistics(ILogger<JellyfinLibraryStatistics> logger, ILibraryManager libraryManager)
		{
			this.Logger = logger;
			this.LibraryManager = libraryManager;
		}

		private readonly ILogger<JellyfinLibraryStatistics> Logger;
		private readonly ILibraryManager LibraryManager;

		public Task StartAsync(CancellationToken cancellationToken)
		{
			try
			{
				this.Logger.LogInformation(String.Format("Jellyfin.Plugin.LibraryStatistics: Started with this configuration: {0}", JsonSerializer.Serialize(Plugin.Instance.PluginConfiguration)));

				this.LibraryManager.ItemAdded += ItemAddedOrUpdatedOrRemoved;
				this.LibraryManager.ItemUpdated += ItemAddedOrUpdatedOrRemoved;
				this.LibraryManager.ItemRemoved += ItemAddedOrUpdatedOrRemoved;
				this.ItemAddedOrUpdatedOrRemoved(null, null);
			}
			catch (Exception ex)
			{
				this.Logger.LogError(ex, $"Jellyfin.Plugin.LibraryStatistics: {ex.Message}");
				return Task.CompletedTask;
			}

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			try
			{
				this.LibraryManager.ItemAdded -= ItemAddedOrUpdatedOrRemoved;
				this.LibraryManager.ItemUpdated -= ItemAddedOrUpdatedOrRemoved;
				this.LibraryManager.ItemRemoved -= ItemAddedOrUpdatedOrRemoved;
			}
			catch (Exception ex)
			{
				this.Logger.LogError(ex, $"Jellyfin.Plugin.LibraryStatistics: {ex.Message}");
			}

			return Task.CompletedTask;
		}

		private void ItemAddedOrUpdatedOrRemoved(object sender, ItemChangeEventArgs e)
		{
			if (Plugin.Instance.LibraryStatistics != null)
			{
				Plugin.Instance.LibraryStatistics.NeedsRecalculation = true;
			}
		}
	}
}
