using System.Collections.Generic;
using Sitecore.Data;

namespace PT.Framework.ViewModelResolver.Infrastructure
{
    public class MappingTable
    {
        private static volatile MappingTable _instance;
        private static readonly object SyncRoot = new object();

        private MappingTable()
        {
        }

        public static MappingTable Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                lock (SyncRoot)
                {
                    if (_instance != null)
                        return _instance;
                    _instance = new MappingTable
                    {
                        Map = new Dictionary<string, ID>(),
                    };
                }
                return _instance;
            }
        }

        

        public Dictionary<string, ID> Map { get; set; }
    }
}