using Microsoft.EntityFrameworkCore;

#nullable disable

namespace ChatBirthdayBot.Database {
	public class DataContext : DbContext {
		public DataContext() { }

		public DataContext(DbContextOptions<DataContext> options)
			: base(options) { }

		// ReSharper disable UnusedAutoPropertyAccessor.Global
		public virtual DbSet<ChatRecord> Chats { get; set; }
		public virtual DbSet<UserRecord> Users { get; set; }
		public virtual DbSet<UserChat> UserChats { get; set; }
		// ReSharper restore UnusedAutoPropertyAccessor.Global

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			if (!optionsBuilder.IsConfigured) {
				optionsBuilder.UseSqlite("Data Source=data.db");
				//optionsBuilder.LogTo(Console.WriteLine);
				//optionsBuilder.EnableSensitiveDataLogging();
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.Entity<ChatRecord>(
				entity => {
					entity.ToTable("Chat");

					entity.Property(e => e.Id)
						.HasColumnType("integer")
						.ValueGeneratedNever()
						.HasColumnName("ID");

					entity.Property(e => e.Name).HasColumnType("varchar");

					entity.Property(e => e.Locale).HasColumnType("varchar");
				}
			);

			modelBuilder.Entity<UserRecord>(
				entity => {
					entity.ToTable("User");

					entity.Property(e => e.Id)
						.HasColumnType("integer")
						.ValueGeneratedNever()
						.HasColumnName("ID");

					entity.Property(e => e.BirthdayDay).HasColumnType("integer");

					entity.Property(e => e.BirthdayMonth).HasColumnType("integer");

					entity.Property(e => e.BirthdayYear).HasColumnType("integer");

					entity.Property(e => e.FirstName).HasColumnType("varchar");

					entity.Property(e => e.LastName).HasColumnType("varchar");

					entity.Property(e => e.Username).HasColumnType("varchar");

					entity.HasMany(p => p.Chats)
						.WithMany(p => p.Users)
						.UsingEntity<UserChat>(
							p => p
								.HasOne(x => x.Chat)
								.WithMany(x => x.UserChats)
								.HasForeignKey(x => x.ChatId),
							p => p
								.HasOne(x => x.User)
								.WithMany(x => x.UserChats)
								.HasForeignKey(x => x.UserId)
						);
				}
			);

			modelBuilder.Entity<UserChat>(
				entity => {
					entity.HasKey(e => new { e.UserId, e.ChatId });

					entity.ToTable("UserChat");

					entity.HasIndex(e => e.ChatId, "UserChat_ChatID");

					entity.HasIndex(e => e.UserId, "UserChat_UserID");

					entity.Property(e => e.UserId)
						.HasColumnType("integer")
						.HasColumnName("UserID");

					entity.Property(e => e.ChatId)
						.HasColumnType("integer")
						.HasColumnName("ChatID");

					entity.Property(e => e.IsPublic).HasColumnType("integer");
				}
			);
		}
	}
}
