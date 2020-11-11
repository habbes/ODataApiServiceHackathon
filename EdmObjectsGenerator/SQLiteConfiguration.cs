using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using System.Text;

namespace EdmObjectsGenerator
{
    /// <summary>
    /// Configuration class to register SQLite provider to EF6 db contexts
    /// see: https://docs.microsoft.com/en-us/ef/ef6/fundamentals/providers/
    /// see: https://stackoverflow.com/questions/20460357/problems-using-entity-framework-6-and-sqlite/23237737#23237737
    /// see: https://stackoverflow.com/questions/38501836/ef6-with-sqlite-in-net-4-0-code-based-configuration-unable-to-determine-the-d
    /// </summary>
    public class SQLiteConfiguration : DbConfiguration
    {
        public SQLiteConfiguration()
        {
            SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
            SetProviderServices("System.Data.SQLite", (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
            SetProviderServices("System.Data.SQLite.EF6", (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
        }
    }
}
