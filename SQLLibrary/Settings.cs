﻿using DbInterface.Models;

namespace SQLLibrary
{
    public class Settings
    {
        public static DbType Type = DbType.SQL;
        public static DbConnectionData ConnectionData { get; set; }
        public static string ConnectionString { get; set; }
    }
}
