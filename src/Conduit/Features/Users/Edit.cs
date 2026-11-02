using System.Net;
using System.Text.Json.Serialization;
using Conduit.Features.Users;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

public class Edit
{
    public class UserData
    {
        private string? _username;
        private string? _email;
        private string? _password;
        private string? _bio;
        private string? _image;

        public string? Username
        {
            get => _username;
            set
            {
                _username = value;
                UsernameSet = true;
            }
        }
        public string? Email
        {
            get => _email;
            set
            {
                _email = value;
                EmailSet = true;
            }
        }
        public string? Password
        {
            get => _password;
            set
            {
                _password = value;
                PasswordSet = true;
            }
        }
        public string? Bio
        {
            get => _bio;
            set
            {
                _bio = value;
                BioSet = true;
            }
        }
        public string? Image
        {
            get => _image;
            set
            {
                _image = value;
                ImageSet = true;
            }
        }
        [JsonIgnore]
        public bool UsernameSet {get; private set;}
        [JsonIgnore]
        public bool EmailSet {get; private set;}
        [JsonIgnore]
        public bool PasswordSet {get; private set;}
        [JsonIgnore]
        public bool BioSet {get; private set;}
        [JsonIgnore]
        public bool ImageSet {get; private set;}
    }

    public record Command(UserData User) : IRequest<UserEnvelope>;

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.User).NotNull();
            RuleFor(x => x.User.Username)
                .NotEmpty()
                .WithMessage(Constants.BLANK)
                .MaximumLength(256)
                .When(x => x.User is {UsernameSet: true});
            RuleFor(x => x.User.Email)
                .NotEmpty()
                .WithMessage(Constants.BLANK)
                .MaximumLength(320)
                .When(x => x.User is { EmailSet: true });
            RuleFor(x => x.User.Password)
                .NotEmpty()
                .WithMessage(Constants.BLANK)
                .MinimumLength(8)
                .WithMessage(Constants.PASSWORD_TOO_SHORT)
                .When(x => x.User is { PasswordSet: true });
        }
    }
    public class Handler(
        ICurrentUserAccessor currentUserAccessor, 
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
            var currentPersonId = currentUserAccessor.GetCurrentPersonId();
            var person = await context.Persons
                .Where(x => x.PersonId == currentPersonId).
                FirstOrDefaultAsync();
            if (person is null)
            {
                throw new RestException(HttpStatusCode.NotFound, "user", Constants.NOT_FOUND);
            }
            if (message.User.UsernameSet && message.User.Username != person.Username)
            {
                if (
                    await context.Persons
                    .Where(x => x.Username == message.User.Username)
                    .AnyAsync(cancellationToken)
                )
                {
                    throw new RestException(HttpStatusCode.Conflict, "username", Constants.IN_USE);
                }
                person.Username = message.User.Username;
            }
            if (message.User.EmailSet && message.User.Email != person.Email)
            {
                if (
                    await context
                        .Persons.Where(x => x.Email == message.User.Email)
                        .AnyAsync(cancellationToken)
                )
                {
                    throw new RestException(HttpStatusCode.Conflict, "email", Constants.IN_USE);
                }

                person.Email = message.User.Email;
            }
            if (message.User.BioSet)
            {
                person.Bio = string.IsNullOrEmpty(message.User.Bio) ? null : message.User.Bio;
            }
            if (message.User.ImageSet)
            {
                person.Image = string.IsNullOrEmpty(message.User.Image) ? null : message.User.Image;
            }
            if (message.User.PasswordSet && !string.IsNullOrEmpty(message.User.Password))
            {
                person.Hash = await passwordHasher.Hash(message.User.Password);
            }
            await context.SaveChangesAsync(cancellationToken);

            var user = mapper.PersonToUser(person);
            user.Token = jwtTokenGenerator.CreateToken(person.PersonId);
            return new UserEnvelope(user);
        }
    }
}