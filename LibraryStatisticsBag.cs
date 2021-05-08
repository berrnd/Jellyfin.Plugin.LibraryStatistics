using System;

namespace Jellyfin.Plugin.LibraryStatistics
{
	public class LibraryStatisticsBag
	{
		public LibraryStatisticsBag()
		{
			this.NeedsRecalculation = true;
			this.TotalRunTimeMinutes = 0;
			this.NewestItemDate = DateTime.MinValue;
			this.TotalFileSize = 0;
		}

		public double TotalRunTimeMinutes { get; set; }
		public DateTime? NewestItemDate { get; set; }
		public long? TotalFileSize { get; set; }
		public bool NeedsRecalculation { get; set; }
	}
}
