using Riok.Mapperly.Abstractions;
using Conduit.Features.Users;

[Mapper]
public partial class ConduitMapper
{
    [MapperIgnoreSource(nameof(Person.PersonId))]
    [MapperIgnoreSource(nameof(Person.ArticleFavorites))]
    [MapperIgnoreSource(nameof(Person.Following))]
    [MapperIgnoreSource(nameof(Person.Followers))]
    [MapperIgnoreSource(nameof(Person.Hash))]
    [MapperIgnoreSource(nameof(Person.Salt))]
    [MapperIgnoreSource(nameof(Person.IsFollowedByCurrentUser))]
    [MapperIgnoreTarget(nameof(User.Token))]
    public partial User PersonToUser(Person person);
}