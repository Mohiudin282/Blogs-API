using System.Net;
using System.Reflection.Metadata;
using Conduit.Features.Users;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

public class Create
{
    public record UserData(string? Username, string? Email, string? Password);
    public record Command(UserData User) : IRequest<UserEnvelope>;

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.User).NotNull();
            When(
                x => x.User != null,
                () =>
                {
                    RuleFor(x => x.User.Username)
                        .NotEmpty()
                        .WithMessage(Constants.BLANK)
                        .MaximumLength(256);
                    RuleFor(x => x.User.Email)
                        .NotEmpty()
                        .WithMessage(Constants.BLANK)
                        .MaximumLength(320);
                    RuleFor(x => x.User.Password)
                        .NotEmpty()
                        .WithMessage(Constants.BLANK)
                        .MinimumLength(8)
                        .WithMessage(Constants.PASSWORD_TOO_SHORT);
                }
            );
        }
    }

    public class Handler(
        ConduitContext context,
        IPasswordHasher passwordHasher,
        ConduitMapper mapper,
        IJwtTokenGenerator jwtTokenGenerator
    ) : IRequestHandler<Command, UserEnvelope>
    {
        public async ValueTask<UserEnvelope> Handle(
            Command message, 
            CancellationToken cancellationToken)
        {
            if (
                await context.Persons
                .Where(x => x.Username == message.User.Username)
                .AnyAsync(cancellationToken)
            )
            {
                throw new RestException(HttpStatusCode.Conflict, "username", Constants.IN_USE);
            }
            var person = new Person
            {
                Username = message.User.Username,
                Email = message.User.Email,
                Hash = await passwordHasher.Hash(
                    message.User.Password ?? throw new InvalidOperationException()
                ),
                Salt = []
            };
            await context.Persons.AddAsync(person, cancellationToken);
            await context.SaveChangesAsync();

            var user = mapper.PersonToUser(person);
            user.Token = jwtTokenGenerator.CreateToken(person.PersonId);
            return new UserEnvelope(user);
        }
    }
}

