﻿using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.LibraryStatistics
{
	public class JellyfinLibraryStatistics : IServerEntryPoint
	{
		public JellyfinLibraryStatistics(ILogger<JellyfinLibraryStatistics> logger, IJsonSerializer jsonSerializer, ILibraryManager libraryManager)
		{
			this.Logger = logger;
			this.JsonSerializer = jsonSerializer;
			this.LibraryManager = libraryManager;
		}

		private readonly ILogger<JellyfinLibraryStatistics> Logger;
		private readonly IJsonSerializer JsonSerializer;
		private readonly ILibraryManager LibraryManager;

		public Task RunAsync()
		{
			try
			{
				this.Logger.LogInformation(String.Format("Jellyfin.Plugin.LibraryStatistics: Started with this configuration: {0}", this.JsonSerializer.SerializeToString(Plugin.Instance.PluginConfiguration)));

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

		public void Dispose()
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
