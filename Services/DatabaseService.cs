using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace BookmarkManager.Services;

public static class DatabaseService
{
    private static string DbPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BookmarkManager", "bookmarks.db");

    public static string ConnectionString => $"Data Source={DbPath}";

    public static void Initialize()
    {
        var dir = Path.GetDirectoryName(DbPath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Categories (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Icon TEXT DEFAULT '📁',
                SortOrder INTEGER DEFAULT 0,
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS Bookmarks (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Url TEXT NOT NULL,
                Title TEXT NOT NULL,
                Description TEXT DEFAULT '',
                CategoryId INTEGER DEFAULT 0,
                SortOrder INTEGER DEFAULT 0,
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
            );

            CREATE TABLE IF NOT EXISTS Tags (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE,
                Color TEXT DEFAULT '#4A90D9'
            );

            CREATE TABLE IF NOT EXISTS BookmarkTags (
                BookmarkId INTEGER,
                TagId INTEGER,
                PRIMARY KEY (BookmarkId, TagId),
                FOREIGN KEY (BookmarkId) REFERENCES Bookmarks(Id) ON DELETE CASCADE,
                FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Settings (
                Key TEXT PRIMARY KEY,
                Value TEXT
            );
        ";
        cmd.ExecuteNonQuery();
    }
}
