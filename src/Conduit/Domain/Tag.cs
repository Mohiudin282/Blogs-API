public class Tag
{
    public string? TagId {get; init;}
    public List<ArticleTag> ArticleTags {get; init;} = new();
}