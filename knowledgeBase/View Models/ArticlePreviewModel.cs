namespace knowledgeBase.View_Models;

public class ArticlePreviewModel
{
    public int Id { get; set; }
    public string Author { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public DateOnly PublishDate { get; set; }
    public int LikeCount { get; set; }
    public int ReadingTime { get; set; }
}