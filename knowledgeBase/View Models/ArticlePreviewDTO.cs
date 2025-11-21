namespace knowledgeBase.View_Models;

public class ArticlePreviewDTO
{
    public int Id { get; set; }
    public string Author { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateOnly PublishDate { get; set; }
    public int LikesCount { get; set; }
    public int ReadingTime { get; set; }
    public string Category { get; set; }
    public bool IsLikedByUser { get; set; }
    public string Icon { get; set; }
}