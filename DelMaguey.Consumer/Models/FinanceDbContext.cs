using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DelMaguey.Consumer.Models;

public partial class FinanceDbContext : DbContext
{
    public FinanceDbContext()
    {
    }

    public FinanceDbContext(DbContextOptions<FinanceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:FinanceConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Amt).HasColumnName("amt");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasColumnName("category");
            entity.Property(e => e.CcNum).HasColumnName("cc_num");
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .HasColumnName("city");
            entity.Property(e => e.CityPop).HasColumnName("city_pop");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.First)
                .HasMaxLength(50)
                .HasColumnName("first");
            entity.Property(e => e.Gender)
                .HasMaxLength(50)
                .HasColumnName("gender");
            entity.Property(e => e.IsFraud).HasColumnName("is_fraud");
            entity.Property(e => e.Job)
                .HasMaxLength(100)
                .HasColumnName("job");
            entity.Property(e => e.Last)
                .HasMaxLength(50)
                .HasColumnName("last");
            entity.Property(e => e.Lat).HasColumnName("lat");
            entity.Property(e => e.Long).HasColumnName("long");
            entity.Property(e => e.MerchLat).HasColumnName("merch_lat");
            entity.Property(e => e.MerchLong).HasColumnName("merch_long");
            entity.Property(e => e.Merchant)
                .HasMaxLength(50)
                .HasColumnName("merchant");
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .HasColumnName("state");
            entity.Property(e => e.Street)
                .HasMaxLength(50)
                .HasColumnName("street");
            entity.Property(e => e.TransDateTransTime).HasColumnName("trans_date_trans_time");
            entity.Property(e => e.TransNum)
                .HasMaxLength(100)
                .HasColumnName("trans_num");
            entity.Property(e => e.UnixTime).HasColumnName("unix_time");
            entity.Property(e => e.Zip).HasColumnName("zip");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
