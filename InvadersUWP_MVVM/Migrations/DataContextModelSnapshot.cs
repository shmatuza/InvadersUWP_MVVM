using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using InvadersUWP_MVVM.DAL;

namespace InvadersUWP_MVVM.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("InvadersUWP_MVVM.Model.Highscore", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("PlayerName")
                        .IsRequired();

                    b.Property<int>("Score");

                    b.HasKey("Id");

                    b.ToTable("Highscore");
                });
        }
    }
}
