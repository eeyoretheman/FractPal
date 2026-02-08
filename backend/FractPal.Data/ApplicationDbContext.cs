namespace FractPal.Data;

using System.Text.Json;
using FractPal.Model.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

public class ApplicationDbContext : IdentityDbContext
{

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    /// <summary>
    /// Users in the application (Identity user extended as <see cref="FractPalUser"/>).
    /// Exposed here for queries and EF mapping.
    /// </summary>
    public DbSet<FractPalUser> FractPalUsers => this.Set<FractPalUser>();

    /// <summary>
    /// Posts created by users.
    /// </summary>
    public DbSet<Post> Posts => this.Set<Post>();

    /// <summary>
    /// Comments made on posts.
    /// </summary>
    public DbSet<Comment> Comments => this.Set<Comment>();

    /// <summary>
    /// Join entity representing a "like" (many-to-many relationship between users and posts).
    /// Composite key and relationships are configured in <see cref="OnModelCreating(ModelBuilder)"/>
    /// </summary>
    public DbSet<Like> Likes => this.Set<Like>();

    /// <summary>
    /// Fractal entities which include JSON-serializable dictionaries for rules and instructions.
    /// </summary>
    public DbSet<Fractal> Fractals => this.Set<Fractal>();

    /// <summary>
    /// Configure EF Core model mappings, relationships and conversions.
    /// - Configures composite key and relationships for <see cref="Like"/>.
    /// - Creates a <see cref="ValueComparer{T}"/> used to compare dictionary properties on <see cref="Fractal"/>.
    /// - Configures JSON serialization and storage for <see cref="Fractal.Rules"/> and <see cref="Fractal.Instructions"/>.
    /// </summary>
    /// <param name="builder">The model builder used to configure EF Core mappings.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Likes for Many-to-Many relationship

        builder.Entity<Like>()
            .HasKey(l => new { l.UserId, l.PostId });

        builder.Entity<Like>()
            .HasOne(l => l.User)
            .WithMany(u => u.Likes)
            .HasForeignKey(l => l.UserId);

        builder.Entity<Like>()
            .HasOne(l => l.Post)
            .WithMany(p => p.Likes)
            .HasForeignKey(l => l.PostId);

        // Setup a ValueComparer

        var dictionaryComparer = CreateDictValueComparer();

        // Configure Fractal Rules and Instructions serialization
        // Stored as nvarchar(max) JSON. On read, empty or whitespace column produces an empty dictionary.
        // A ValueComparer is provided so EF can detect changes to the dictionary and its inner lists.

        builder.Entity<Fractal>()
            .Property(f => f.Rules)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => v == null ? "{}" : JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => string.IsNullOrWhiteSpace(v)
                    ? new Dictionary<string, List<string>>()
                    : JsonSerializer.Deserialize<Dictionary<string, List<string>>>(v, JsonSerializerOptions.Default)
                                                                              ?? new Dictionary<string, List<string>>()
                )
            .Metadata.SetValueComparer(dictionaryComparer);

        builder.Entity<Fractal>()
            .Property(f => f.Instructions)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => v == null ? "{}" : JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => string.IsNullOrWhiteSpace(v)
                    ? new Dictionary<string, List<string>>()
                    : JsonSerializer.Deserialize<Dictionary<string, List<string>>>(v, JsonSerializerOptions.Default)
                                                                              ?? new Dictionary<string, List<string>>()
                )
            .Metadata.SetValueComparer(dictionaryComparer);
    }

    /// <summary>
    /// Creates a <see cref="ValueComparer{Dictionary}"/> suitable for comparing
    /// <see cref="Dictionary{String,List{String}}"/> instances used by EF Core value conversions.
    /// The comparer uses deep equality, a deterministic hash and a snapshot function.
    /// </summary>
    /// <returns>A configured <see cref="ValueComparer{Dictionary}"/> instance.</returns>
    private static ValueComparer<Dictionary<string, List<string>>> CreateDictValueComparer()
    {
        return new ValueComparer<Dictionary<string, List<string>>>(
            (a, b) => DictEquality(a, b),
            v => DictHash(v),
            v => DictSnapShot(v)
        );
    }

    /// <summary>
    /// Deep equality check for two dictionaries mapping string keys to ordered lists of strings.
    /// Comparison is order-sensitive for list values and uses <see cref="StringComparer.Ordinal"/> for key ordering in hashing.
    /// </summary>
    /// <param name="a">First dictionary (may be null).</param>
    /// <param name="b">Second dictionary (may be null).</param>
    /// <returns>True if both dictionaries are equal by structure and element values; otherwise false.</returns>
    private static bool DictEquality(Dictionary<string, List<string>>? a, Dictionary<string, List<string>>? b)
    {
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            if (a.Count != b.Count)
                return false;
            foreach (var kvp in a)
            {
                if (!b.TryGetValue(kvp.Key, out var bList))
                    return false;
                var aList = kvp.Value;
                if (aList is null && bList is null)
                    continue;
                if (aList is null || bList is null)
                    return false;
                if (aList.Count != bList.Count)
                    return false;
                for (int i = 0; i < aList.Count; i++)
                {
                    if (aList[i] != bList[i])
                        return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Produces a deterministic hash code for a dictionary of lists.
    /// Keys are processed in ordinal order to ensure consistent hash independent of insertion order.
    /// Each key, each list item and the list length contribute to the hash. Null dictionary returns 0.
    /// </summary>
    /// <param name="v">Dictionary to hash (may be null).</param>
    /// <returns>An integer hash code.</returns>
    private static int DictHash(Dictionary<string, List<string>>? v)
    {
        if (v is null)
            return 0;
        var hash = new HashCode();
        foreach (var key in v.Keys.OrderBy(k => k, StringComparer.Ordinal))
        {
            hash.Add(key, StringComparer.Ordinal);
            var list = v[key];
            if (list != null)
            {
                foreach (var item in list)
                {
                    hash.Add(item);
                }
                hash.Add(list.Count);
            }
            else
            {
                // Marker to distinguish null list vs empty list (empty lists add count 0 above)
                hash.Add(-1);
            }
        }
        return hash.ToHashCode();
    }

    /// <summary>
    /// Creates a deep snapshot copy of the provided dictionary and its inner lists.
    /// Used by EF Core to take a snapshot of a value object so changes can be detected.
    /// </summary>
    /// <param name="v">The dictionary to snapshot.</param>
    /// <returns>A new dictionary containing copies of the inner lists.</returns>
    private static Dictionary<string, List<string>> DictSnapShot(Dictionary<string, List<string>> v)
    {
        if (v == null)
            return new Dictionary<string, List<string>>();
        var copy = new Dictionary<string, List<string>>(v.Count);
        foreach (var kvp in v)
        {
            copy[kvp.Key] = kvp.Value == null ? new List<string>() : new List<string>(kvp.Value);
        }
        return copy;
    }
}
