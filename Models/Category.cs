using System;

namespace BookmarkManager.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "📁";
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
