using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using twiker_backend.Db.Models;

namespace twiker_backend.Models.DatabaseContext
{
    public partial class TwikerContext : DbContext
    {
        public TwikerContext()
        {
        }

        public TwikerContext(DbContextOptions<TwikerContext> options)
            : base(options)
        {
        }

        public virtual DbSet<LikeTable> LikeTables { get; set; }

        public virtual DbSet<PinnedTable> PinnedTables { get; set; }

        public virtual DbSet<PostTable> PostTables { get; set; }

        public virtual DbSet<RetweetTable> RetweetTables { get; set; }

        public virtual DbSet<UserTable> UserTables { get; set; }

        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
        // {
        //     DotNetEnv.Env.TraversePath().Load();
        //     optionsBuilder.UseNpgsql(DotNetEnv.Env.GetString("connection_string"));
        // }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp");

            modelBuilder.Entity<LikeTable>(entity =>
            {
                entity.HasKey(e => e.PostId).HasName("like_table_pkey");

                entity.ToTable("like_table");

                entity.Property(e => e.PostId)
                    .ValueGeneratedNever()
                    .HasColumnName("postId");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("createdAt");
                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .HasColumnName("username");

                entity.HasOne(d => d.Post).WithOne(p => p.LikeTable)
                    .HasForeignKey<LikeTable>(d => d.PostId)
                    .HasConstraintName("like_table_postId_fkey");

                entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.LikeTables)
                    .HasPrincipalKey(p => p.Username)
                    .HasForeignKey(d => d.Username)
                    .HasConstraintName("like_table_username_fkey");
            });

            modelBuilder.Entity<PinnedTable>(entity =>
            {
                entity.HasKey(e => e.PostId).HasName("pinned_table_pkey");

                entity.ToTable("pinned_table");

                entity.Property(e => e.PostId)
                    .ValueGeneratedNever()
                    .HasColumnName("postId");

                entity.HasOne(d => d.Post).WithOne(p => p.PinnedTable)
                    .HasForeignKey<PinnedTable>(d => d.PostId)
                    .HasConstraintName("pinned_table_postId_fkey");
            });

            modelBuilder.Entity<PostTable>(entity =>
            {
                entity.HasKey(e => e.PostId).HasName("post_table_pkey");

                entity.ToTable("post_table");

                entity.Property(e => e.PostId)
                    .HasDefaultValueSql("uuid_generate_v4()")
                    .HasColumnName("postId");
                entity.Property(e => e.Content).HasColumnName("content");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("createdAt");
                entity.Property(e => e.Postby)
                    .HasMaxLength(50)
                    .HasColumnName("postby");

                entity.HasOne(d => d.PostbyNavigation).WithMany(p => p.PostTables)
                    .HasPrincipalKey(p => p.Username)
                    .HasForeignKey(d => d.Postby)
                    .HasConstraintName("post_table_postby_fkey");
            });

            modelBuilder.Entity<RetweetTable>(entity =>
            {
                entity.HasKey(e => e.PostId).HasName("retweet_table_pkey");

                entity.ToTable("retweet_table");

                entity.Property(e => e.PostId)
                    .ValueGeneratedNever()
                    .HasColumnName("postId");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("createdAt");
                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .HasColumnName("username");

                entity.HasOne(d => d.Post).WithOne(p => p.RetweetTable)
                    .HasForeignKey<RetweetTable>(d => d.PostId)
                    .HasConstraintName("retweet_table_postId_fkey");

                entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.RetweetTables)
                    .HasPrincipalKey(p => p.Username)
                    .HasForeignKey(d => d.Username)
                    .HasConstraintName("retweet_table_username_fkey");
            });

            modelBuilder.Entity<UserTable>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("user_table_pkey");

                entity.ToTable("user_table");

                entity.HasIndex(e => e.Username, "user_table_username_key").IsUnique();

                entity.HasIndex(e => e.Username, "user_table_username_key1").IsUnique();

                entity.HasIndex(e => e.Username, "user_table_username_key2").IsUnique();

                entity.HasIndex(e => e.Username, "user_table_username_key3").IsUnique();

                entity.Property(e => e.UserId)
                    .HasDefaultValueSql("uuid_generate_v4()")
                    .HasColumnName("userId");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("createdAt");
                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .HasColumnName("email");
                entity.Property(e => e.Firstname)
                    .HasMaxLength(50)
                    .HasColumnName("firstname");
                entity.Property(e => e.Lastname)
                    .HasMaxLength(50)
                    .HasColumnName("lastname");
                entity.Property(e => e.Password).HasColumnName("password");
                entity.Property(e => e.Profilepic).HasColumnName("profilepic");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("updatedAt");
                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .HasColumnName("username");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}