using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using System;

namespace Jellyfin.Plugin.LibraryStatistics
{
	public class Plugin : BasePlugin<PluginConfiguration>
    {
        public Plugin(IApplicationPaths appPaths, IXmlSerializer xmlSerializer) : base(appPaths, xmlSerializer)
        {
            this.LibraryStatistics = new LibraryStatisticsBag();
            Instance = this;
        }

        public LibraryStatisticsBag LibraryStatistics { get; private set; }

        public override string Name => "LibraryStatistics";

        public static Plugin Instance { get; private set; }

        public override string Description => "Provides a library statistic API";

        public PluginConfiguration PluginConfiguration => Configuration;

        private readonly Guid _id = new Guid("58b8cc55-8a62-4556-9ac9-c6c8b4dae57f");
        public override Guid Id => _id;
    }
}
