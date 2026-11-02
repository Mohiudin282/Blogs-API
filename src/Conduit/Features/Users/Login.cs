using System.Net;
using Conduit.Features.Users;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class Login
{
    public class UserData
    {
        public string? Email {get; set;}
        public string? Password {get; set;}
    }
    public record Command(UserData User) : IRequest<UserEnvelope>;
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator(){
            RuleFor(x => x.User).NotNull();
            When(
                x => x.User != null,
                () =>
                {
                    RuleFor(x => x.User.Email).NotEmpty().WithMessage(Constants.BLANK);
                    RuleFor(x => x.User.Password).NotEmpty().WithMessage(Constants.BLANK);
                }
            );
        }
    }
    public class Handler(
        ConduitContext context,
        ConduitMapper mapper,
        IJwtTokenGenerator jwtTokenGenerator,
        PasswordHasher passwordHasher) : IRequestHandler<Command, UserEnvelope>
    {
        public async ValueTask<UserEnvelope> Handle(
            Command message, CancellationToken cancellationToken)
        {
            var person = await context.Persons
                .Where(x => x.Email == message.User.Email)
                .SingleOrDefaultAsync(cancellationToken);

            if (person == null)
            {
                throw new RestException(HttpStatusCode.Unauthorized, "credentials", "invalid");
            }
            var verification = passwordHasher.Verify(
                message.User.Password ?? throw new InvalidOperationException(), 
                person.Hash);
            if (verification == PasswordVerificationResult.Failed)
            {
                throw new RestException(HttpStatusCode.Unauthorized, "credentials", "invalid");
            }
            var user = mapper.PersonToUser(person);
            user.Token = jwtTokenGenerator.CreateToken(person.PersonId);
            return new UserEnvelope(user);
        }
    }
}