﻿using DbInterface.Models;
using System.Data.SQLite;
using System.IO;

namespace SQLiteLibrary
{
    public class CONNECTION
    {
        public CONNECTION(DbConnectionData conData)
        {
            if (string.IsNullOrEmpty(conData.Name)) return;

            if(string.IsNullOrEmpty(conData.Path))
            {
                Settings.ConnectionString = string.Format("Data Source={0}", conData.Name);
            }
            else
            {
                Settings.ConnectionString = string.Format("Data Source={0}", Path.Combine(conData.Path, conData.Name));
            }

            Settings.Con = new SQLiteConnection(Settings.ConnectionString);
        }
    }
}
