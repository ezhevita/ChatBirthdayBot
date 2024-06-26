﻿using Microsoft.EntityFrameworkCore;

namespace ChatBirthdayBot.Database;

public class DataContext : DbContext
{
	public DataContext() { }

	public DataContext(DbContextOptions<DataContext> options)
		: base(options)
	{
	}

	public virtual DbSet<ChatRecord> Chats { get; set; }
	public virtual DbSet<UserRecord> Users { get; set; }

	public virtual DbSet<UserChat> UserChats { get; set; }
	public virtual DbSet<SentMessage> SentMessages { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ChatRecord>(
			entity =>
			{
				entity.ToTable("Chats");

				entity.Property(e => e.Id)
					.ValueGeneratedNever()
					.HasColumnName("ID");

				entity.Property(e => e.Name);
				entity.Property(e => e.Locale);
				entity.Property(e => e.TimeZoneHourOffset);
				entity.Property(e => e.CustomOffsetInHours);
				entity.Property(e => e.ShouldPinNotify);
			});

		modelBuilder.Entity<UserRecord>(
			entity =>
			{
				entity.ToTable("Users");

				entity.Property(e => e.Id)
					.ValueGeneratedNever()
					.HasColumnName("ID");

				entity.Property(e => e.BirthdayDay);
				entity.Property(e => e.BirthdayMonth);
				entity.Property(e => e.BirthdayYear);
				entity.Property(e => e.FirstName);
				entity.Property(e => e.LastName);
				entity.Property(e => e.Username);

				entity.HasMany(p => p.Chats)
					.WithMany(p => p.Users)
					.UsingEntity<UserChat>();
			});

		modelBuilder.Entity<UserChat>(
			entity =>
			{
				entity.HasKey(e => new {e.UserId, e.ChatId});

				entity.HasIndex(e => e.ChatId);
				entity.HasIndex(e => e.UserId);

				entity.Property(e => e.UserId);
				entity.Property(e => e.ChatId);
				entity.Property(e => e.IsPublic);
			});

		modelBuilder.Entity<SentMessage>(
			entity =>
			{
				entity.HasKey(e => new {e.ChatId, e.MessageId});

				entity.HasOne(e => e.Chat)
					.WithMany(e => e.SentMessages)
					.HasForeignKey(e => e.ChatId);

				entity.Property(e => e.ChatId);
				entity.Property(e => e.MessageId);
				entity.Property(e => e.SendDateUtc);
			});
	}
}
