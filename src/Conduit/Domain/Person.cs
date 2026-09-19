using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Person
{
    [JsonIgnore]
    public int PerosnId {get; init;}

    public string? Username {get; set;}

    [JsonIgnore]
    public string? Email {get; set;}

    public string? Bio {get; set;}

    public string? Image {get; set;}

    [NotMapped]
    [JsonPropertyName("following")]
    public bool IsFollowedByCurrentUser {get; set;}

    [JsonIgnore]
    public List<ArticleFavorite> ArticleFavorites {get; init;} = new();

    [JsonIgnore]
    public List<FollowedPeople> Followers {get; init;} = new();

    [JsonIgnore]
    public List<FollowedPeople> Following {get; init;} = new();

    [JsonIgnore]
    public byte[] Hash {get; set;} = [];

    [JsonIgnore]
    public byte[] Salt {get; set;} = [];
}