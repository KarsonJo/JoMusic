using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace MusicLibrary.Models
{
    public partial class MusicLibraryContext : DbContext
    {
        public MusicLibraryContext()
        {
        }

        public MusicLibraryContext(DbContextOptions<MusicLibraryContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ConnectPath> ConnectPaths { get; set; }
        public virtual DbSet<DirectSubFolder> DirectSubFolders { get; set; }
        public virtual DbSet<FileNode> FileNodes { get; set; }
        public virtual DbSet<FolderCopy> FolderCopies { get; set; }
        public virtual DbSet<FolderCreate> FolderCreates { get; set; }
        public virtual DbSet<FolderCut> FolderCuts { get; set; }
        public virtual DbSet<FolderIdMap> FolderIdMaps { get; set; }
        public virtual DbSet<FolderNode> FolderNodes { get; set; }
        public virtual DbSet<FolderPath> FolderPaths { get; set; }
        public virtual DbSet<NeteaseDatum> NeteaseData { get; set; }
        public virtual DbSet<PlayList> PlayLists { get; set; }
        public virtual DbSet<SongArtist> SongArtists { get; set; }
        public virtual DbSet<SongFileMetum> SongFileMeta { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseSqlite(@"Filename=.\Database\MusicLibrary.db"); <-- in build, database in .\Data\MusicLibrary.db
                optionsBuilder.UseSqlite(@"Filename=..\..\..\..\MusicLibrary\Database\MusicLibrary.db");

                optionsBuilder.LogTo(message => System.Diagnostics.Debug.WriteLine(message));
                optionsBuilder.EnableSensitiveDataLogging(true);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConnectPath>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("ConnectPath");
            });

            modelBuilder.Entity<DirectSubFolder>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("DirectSubFolder");
            });

            modelBuilder.Entity<FileNode>(entity =>
            {
                entity.HasKey(e => new { e.SongId, e.FolderId });

                entity.HasOne(d => d.Folder)
                    .WithMany(p => p.FileNodes)
                    .HasForeignKey(d => d.FolderId);

                entity.HasOne(d => d.Song)
                    .WithMany(p => p.FileNodes)
                    .HasForeignKey(d => d.SongId);
            });

            modelBuilder.Entity<FolderCopy>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("FolderCopy");
            });

            modelBuilder.Entity<FolderCreate>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("FolderCreate");
            });

            modelBuilder.Entity<FolderCut>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("FolderCut");
            });

            modelBuilder.Entity<FolderIdMap>(entity =>
            {
                entity.HasKey(e => new { e.OldId, e.NewId });

                entity.ToTable("FolderIdMap");

                entity.HasOne(d => d.New)
                    .WithMany(p => p.FolderIdMapNews)
                    .HasForeignKey(d => d.NewId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Old)
                    .WithMany(p => p.FolderIdMapOlds)
                    .HasForeignKey(d => d.OldId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<FolderNode>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<FolderPath>(entity =>
            {
                entity.HasKey(e => new { e.Ancestor, e.Descendant });

                entity.HasIndex(e => new { e.Ancestor, e.Descendant, e.Length }, "IX_FolderPaths_ADL");

                entity.HasIndex(e => new { e.Descendant, e.Length }, "IX_FolderPaths_DL");

                entity.HasOne(d => d.AncestorNavigation)
                    .WithMany(p => p.FolderPathAncestorNavigations)
                    .HasForeignKey(d => d.Ancestor);

                entity.HasOne(d => d.DescendantNavigation)
                    .WithMany(p => p.FolderPathDescendantNavigations)
                    .HasForeignKey(d => d.Descendant);
            });

            modelBuilder.Entity<NeteaseDatum>(entity =>
            {
                entity.HasKey(e => e.SongId);

                entity.HasIndex(e => e.NeteaseId, "IX_NeteaseData_NeteaseId");

                entity.Property(e => e.SongId).ValueGeneratedNever();

                entity.HasOne(d => d.Song)
                    .WithOne(p => p.NeteaseDatum)
                    .HasForeignKey<NeteaseDatum>(d => d.SongId);
            });

            modelBuilder.Entity<PlayList>(entity =>
            {
                entity.HasKey(e => new { e.NavType, e.NavValue });

                entity.ToTable("PlayList");
            });

            modelBuilder.Entity<SongArtist>(entity =>
            {
                entity.HasKey(e => new { e.SongId, e.ArtistName });

                entity.HasOne(d => d.Song)
                    .WithMany(p => p.SongArtists)
                    .HasForeignKey(d => d.SongId);
            });

            modelBuilder.Entity<SongFileMetum>(entity =>
            {
                entity.HasIndex(e => e.AlbumName, "IX_SongFileMeta_AlbumName");

                entity.HasIndex(e => e.SongName, "IX_SongFileMeta_SongName");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.SongName).IsRequired();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
