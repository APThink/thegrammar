﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGrammar.Database.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20231217160543_init")]
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
partial class init
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
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
                    .HasColumnType("TEXT");

                b.Property<int>("RightKey")
                    .HasColumnType("INTEGER");

                b.Property<DateTime>("UpdatedAt")
                    .HasColumnType("TEXT");

                b.HasKey("Id");

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
