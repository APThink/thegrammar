﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TheGrammar.Database;

#nullable disable

namespace TheGrammar.Database.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20231217162847_setKeyAsIndexAndShoulUnique")]
partial class setKeyAsIndexAndShoulUnique
{
    /// <inheritdoc />
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "7.0.14");

        modelBuilder.Entity("TheGrammar.Data.Prompt", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("TEXT");

                b.Property<int>("LeftKey")
                    .HasColumnType("INTEGER");

                b.Property<string>("Promt")
                    .IsRequired()
                    .HasMaxLength(5000)
                    .HasColumnType("TEXT");

                b.Property<int>("RightKey")
                    .HasColumnType("INTEGER");

                b.Property<DateTime>("UpdatedAt")
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("LeftKey", "RightKey")
                    .IsUnique();

                b.ToTable("Prompts");
            });

        modelBuilder.Entity("TheGrammar.Data.Requests", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("TEXT");

                b.Property<string>("Request")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("Response")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<DateTime>("UpdatedAt")
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.ToTable("Requests");
            });
#pragma warning restore 612, 618
    }
}
