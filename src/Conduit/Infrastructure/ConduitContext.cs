using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public class ConduitContext(DbContextOptions<ConduitContext> options) : DbContext(options)
{
    private IDbContextTransaction? _currentTransaction;

    public DbSet<Article> Articles { get; init; } = null!;
    public DbSet<Comment> Comments { get; init; } = null!;
    public DbSet<Person> Persons { get; init; } = null!;
    public DbSet<Tag> Tags { get; init; } = null!;
    public DbSet<ArticleTag> ArticleTags { get; init; } = null!;
    public DbSet<ArticleFavorite> ArticleFavorites { get; init; } = null!;
    public DbSet<FollowedPeople> FollowedPeople { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(b =>
        {
            b.Property(x => x.Slug).HasMaxLength(400);
            b.HasIndex(x => x.Slug).IsUnique();
            b.Property(x => x.CreatedAt)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            b.Property(x => x.UpdatedAt)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        });

        modelBuilder.Entity<Person>(b =>
        {
            b.Property(x => x.Username).HasMaxLength(256);
            b.Property(x => x.Email).HasMaxLength(320);
            b.HasIndex(x => x.Username).IsUnique();
            b.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Comment>(b =>
        {
            b.Property(x => x.CreatedAt)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            b.Property(x => x.UpdatedAt)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        });

        modelBuilder.Entity<ArticleTag>(b =>
        {
            b.HasKey(t => new {t.ArticleId, t.TagId});
            b.HasOne(pt => pt.Article)
                .WithMany(t => t.ArticleTags)
                .HasForeignKey(pt => pt.ArticleId);
            
            b.HasOne(pt => pt.Tag)
                .WithMany(t => t.ArticleTags)
                .HasForeignKey(pt => pt.TagId);
        });

        modelBuilder.Entity<ArticleFavorite>(b =>
        {
            b.HasKey(t => new {t.ArticleId, t.PersonId});
            b.HasOne(pt => pt.Article)
                .WithMany(p => p.ArticleFavorites)
                .HasForeignKey(pt => pt.ArticleId);

            b.HasOne(pt => pt.Person)
                .WithMany(p => p.ArticleFavorites)
                .HasForeignKey(pt => pt.PersonId);
        });

        modelBuilder.Entity<FollowedPeople>(b =>
        {
            b.HasKey(t => new {t.ObserverId, t.TargetId});

            b.HasOne(pt => pt.Observer)
                .WithMany(p => p.Followers)
                .HasForeignKey(pt => pt.ObserverId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(pt => pt.Target)
                .WithMany(p => p.Following)
                .HasForeignKey(pt => pt.TargetId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    #region Transaction handling
    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (_currentTransaction != null)
        {
            return;
        }
        _currentTransaction = await Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await Database.CommitTransactionAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
            }
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
            }
            _currentTransaction = null;
        }
    }
    #endregion
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

}