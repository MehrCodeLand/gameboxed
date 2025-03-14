using Domain.Leyer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Leyer.MyDbSetting;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GameRating> GameRatings { get; set; }
    public DbSet<FavoriteGame> FavoriteGames { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure many-to-many relationship
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        // Seed roles into the database
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "User" },
            new Role { Id = 2, Name = "Moderator" },
            new Role { Id = 3, Name = "Admin" }
        );

        // Configure many-to-many for User - FavoriteGames via FavoriteGame
        modelBuilder.Entity<FavoriteGame>()
            .HasKey(fg => new { fg.UserId, fg.GameId });
        modelBuilder.Entity<FavoriteGame>()
            .HasOne(fg => fg.User)
            .WithMany(u => u.FavoriteGames)
            .HasForeignKey(fg => fg.UserId);
        modelBuilder.Entity<FavoriteGame>()
            .HasOne(fg => fg.Game)
            .WithMany(g => g.FavoritedBy)
            .HasForeignKey(fg => fg.GameId);

        base.OnModelCreating(modelBuilder);
    }
}