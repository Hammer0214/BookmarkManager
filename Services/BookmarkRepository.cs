using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using BookmarkManager.Models;

namespace BookmarkManager.Services;

public static class BookmarkRepository
{
    private static string ConnStr => DatabaseService.ConnectionString;

    // ========== Categories ==========
    public static async Task<List<Category>> GetCategoriesAsync()
    {
        using var conn = new SqliteConnection(ConnStr);
        var list = await conn.QueryAsync<Category>(
            "SELECT * FROM Categories ORDER BY SortOrder, Id");
        return new List<Category>(list);
    }

    public static async Task<Category?> GetCategoryAsync(int id)
    {
        using var conn = new SqliteConnection(ConnStr);
        return await conn.QueryFirstOrDefaultAsync<Category>(
            "SELECT * FROM Categories WHERE Id=@Id", new { Id = id });
    }

    public static async Task<int> AddCategoryAsync(Category cat)
    {
        using var conn = new SqliteConnection(ConnStr);
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO Categories (Name, Icon, SortOrder) VALUES (@Name, @Icon, @SortOrder); SELECT last_insert_rowid();", cat);
    }

    public static async Task UpdateCategoryAsync(Category cat)
    {
        using var conn = new SqliteConnection(ConnStr);
        await conn.ExecuteAsync(
            "UPDATE Categories SET Name=@Name, Icon=@Icon, SortOrder=@SortOrder WHERE Id=@Id", cat);
    }

    public static async Task DeleteCategoryAsync(int id)
    {
        using var conn = new SqliteConnection(ConnStr);
        await conn.ExecuteAsync("DELETE FROM Categories WHERE Id=@Id", new { Id = id });
    }

    // ========== Bookmarks ==========
    public static async Task<List<Bookmark>> GetBookmarksAsync(int? categoryId = null)
    {
        using var conn = new SqliteConnection(ConnStr);
        if (categoryId.HasValue && categoryId.Value > 0)
        {
            var list = await conn.QueryAsync<Bookmark>(
                "SELECT * FROM Bookmarks WHERE CategoryId=@CatId ORDER BY SortOrder, Id",
                new { CatId = categoryId.Value });
            return new List<Bookmark>(list);
        }
        var all = await conn.QueryAsync<Bookmark>(
            "SELECT * FROM Bookmarks ORDER BY SortOrder, Id");
        return new List<Bookmark>(all);
    }

    public static async Task<List<Bookmark>> SearchBookmarksAsync(string keyword)
    {
        using var conn = new SqliteConnection(ConnStr);
        var list = await conn.QueryAsync<Bookmark>(
            "SELECT * FROM Bookmarks WHERE Title LIKE @kw OR Url LIKE @kw OR Description LIKE @kw ORDER BY SortOrder, Id",
            new { kw = $"%{keyword}%" });
        return new List<Bookmark>(list);
    }

    public static async Task<int> AddBookmarkAsync(Bookmark bm)
    {
        using var conn = new SqliteConnection(ConnStr);
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Bookmarks (Url, Title, Description, CategoryId, SortOrder)
              VALUES (@Url, @Title, @Description, @CategoryId, @SortOrder);
              SELECT last_insert_rowid();", bm);
    }

    public static async Task UpdateBookmarkAsync(Bookmark bm)
    {
        bm.UpdatedAt = System.DateTime.Now;
        using var conn = new SqliteConnection(ConnStr);
        await conn.ExecuteAsync(
            @"UPDATE Bookmarks SET Url=@Url, Title=@Title, Description=@Description,
              CategoryId=@CategoryId, SortOrder=@SortOrder, UpdatedAt=@UpdatedAt WHERE Id=@Id", bm);
    }

    public static async Task DeleteBookmarkAsync(int id)
    {
        using var conn = new SqliteConnection(ConnStr);
        await conn.ExecuteAsync("DELETE FROM Bookmarks WHERE Id=@Id", new { Id = id });
    }

    // ========== Tags ==========
    public static async Task<List<Tag>> GetTagsAsync()
    {
        using var conn = new SqliteConnection(ConnStr);
        var list = await conn.QueryAsync<Tag>("SELECT * FROM Tags ORDER BY Name");
        return new List<Tag>(list);
    }

    public static async Task<int> AddTagAsync(Tag tag)
    {
        using var conn = new SqliteConnection(ConnStr);
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO Tags (Name, Color) VALUES (@Name, @Color); SELECT last_insert_rowid();", tag);
    }

    public static async Task DeleteTagAsync(int id)
    {
        using var conn = new SqliteConnection(ConnStr);
        await conn.ExecuteAsync("DELETE FROM Tags WHERE Id=@Id", new { Id = id });
    }

    public static async Task<List<Tag>> GetTagsForBookmarkAsync(int bookmarkId)
    {
        using var conn = new SqliteConnection(ConnStr);
        var list = await conn.QueryAsync<Tag>(
            @"SELECT t.* FROM Tags t
              INNER JOIN BookmarkTags bt ON t.Id = bt.TagId
              WHERE bt.BookmarkId = @BookmarkId",
            new { BookmarkId = bookmarkId });
        return new List<Tag>(list);
    }

    public static async Task SetTagsForBookmarkAsync(int bookmarkId, IEnumerable<int> tagIds)
    {
        using var conn = new SqliteConnection(ConnStr);
        await conn.ExecuteAsync("DELETE FROM BookmarkTags WHERE BookmarkId=@Id", new { Id = bookmarkId });
        foreach (var tagId in tagIds)
        {
            await conn.ExecuteAsync(
                "INSERT OR IGNORE INTO BookmarkTags (BookmarkId, TagId) VALUES (@BmId, @TagId)",
                new { BmId = bookmarkId, TagId = tagId });
        }
    }

    public static async Task<Dictionary<int, List<Tag>>> GetTagsForBookmarksAsync(List<int> bookmarkIds)
    {
        if (bookmarkIds.Count == 0) return new();
        using var conn = new SqliteConnection(ConnStr);
        var rows = await conn.QueryAsync<(int BookmarkId, int Id, string Name, string Color)>(
            @"SELECT bt.BookmarkId, t.Id, t.Name, t.Color FROM Tags t
              INNER JOIN BookmarkTags bt ON t.Id = bt.TagId
              WHERE bt.BookmarkId IN @Ids",
            new { Ids = bookmarkIds });
        var dict = bookmarkIds.ToDictionary(id => id, _ => new List<Tag>());
        foreach (var (BookmarkId, Id, Name, Color) in rows)
            dict[BookmarkId].Add(new Tag { Id = Id, Name = Name, Color = Color });
        return dict;
    }

    public static async Task<List<Bookmark>> GetBookmarksByTagAsync(int tagId)
    {
        using var conn = new SqliteConnection(ConnStr);
        var list = await conn.QueryAsync<Bookmark>(
            @"SELECT b.* FROM Bookmarks b
              INNER JOIN BookmarkTags bt ON b.Id = bt.BookmarkId
              WHERE bt.TagId = @TagId ORDER BY b.SortOrder, b.Id",
            new { TagId = tagId });
        return new List<Bookmark>(list);
    }

    // ========== Settings ==========
    public static async Task<string?> GetSettingAsync(string key)
    {
        using var conn = new SqliteConnection(ConnStr);
        return await conn.QueryFirstOrDefaultAsync<string>(
            "SELECT Value FROM Settings WHERE Key=@Key", new { Key = key });
    }

    public static async Task SetSettingAsync(string key, string value)
    {
        using var conn = new SqliteConnection(ConnStr);
        await conn.ExecuteAsync(
            "INSERT OR REPLACE INTO Settings (Key, Value) VALUES (@Key, @Value)",
            new { Key = key, Value = value });
    }

    // ========== Sort Order ==========
    public static async Task UpdateBookmarkSortOrderAsync(int id, int newOrder)
    {
        using var conn = new SqliteConnection(ConnStr);
        await conn.ExecuteAsync(
            "UPDATE Bookmarks SET SortOrder=@Ord WHERE Id=@Id",
            new { Ord = newOrder, Id = id });
    }

    public static async Task UpdateCategorySortOrderAsync(int id, int newOrder)
    {
        using var conn = new SqliteConnection(ConnStr);
        await conn.ExecuteAsync(
            "UPDATE Categories SET SortOrder=@Ord WHERE Id=@Id",
            new { Ord = newOrder, Id = id });
    }
}
