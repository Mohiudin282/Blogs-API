using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Article
{
    [JsonIgnore]
    public int ArticleId {get; init;}

    public string? Slug {get; init;}
    public string? Title {get; set;}
    public string? Description {get; set;}

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Body {get; set;}
    public Person? Author {get; init;}
    public List<Comment> Comments {get; init;} = new();

    [NotMapped]
    public bool Favorited => ArticleFavorites.Count != 0;

    [NotMapped]
    public int FavoritesCount => ArticleFavorites?.Count ?? 0;

    [NotMapped]
    public List<string> TagList =>
        [.. ArticleTags.Where(x => x.TagId is not null).Select(x => x.TagId!)];

    [JsonIgnore]
    public List<ArticleFavorite> ArticleFavorites {get; init;} = new();

    [JsonIgnore]
    public List<ArticleTag> ArticleTags {get; init;} = new();

    public DateTime CreatedAt {get; init;}

    public DateTime UpdatedAt {get; init;}
}