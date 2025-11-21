namespace knowledgeBase.Entities;

public class Article
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
    public string Author { get; set; }
    public string Category { get; set; }
    public DateOnly PublishDate { get; set; }
    public int ReadingTime { get; set; }
    public int LikesCount { get; set; }
    public bool IsLikedByUser { get; set; }
    public string Icon { get; set; }
}