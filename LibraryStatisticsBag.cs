using System;

namespace Jellyfin.Plugin.LibraryStatistics
{
	public class LibraryStatisticsBag
	{
		public LibraryStatisticsBag()
		{
			this.NeedsRecalculation = true;
			this.TotalRunTimeTicks = 0;
			this.NewestItemDate = DateTime.MinValue;
			this.TotalFileSize = 0;
		}

		public long? TotalRunTimeTicks { get; set; }
		public DateTime? NewestItemDate { get; set; }
		public long? TotalFileSize { get; set; }
		public long? TotalFileSizeWithRedundancy { get; set; }
		public bool NeedsRecalculation { get; set; }
	}
}
