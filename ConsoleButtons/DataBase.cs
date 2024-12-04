using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class DrawingDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=YOUR_SERVER_NAME;Database=DrawingDB;Trusted_Connection=True;");
    }

    public DbSet<Drawing> Drawings { get; set; }
    public DbSet<Point> Points { get; set; }
}

public class Drawing
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    public ICollection<Point> Points { get; set; }

    public Drawing()
    {
        Name = string.Empty;
        Points = new List<Point>();
    }
}

public class Point
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int X { get; set; }

    [Required]
    public int Y { get; set; }

    [Required]
    [StringLength(10)]
    public string Color { get; set; }

    [Required]
    [StringLength(1)]
    public char Character { get; set; }

    public int DrawingId { get; set; }
    public Drawing Drawing { get; set; }

    public Point()
    {
        Color = string.Empty;
        Drawing = new Drawing();
    }
}

