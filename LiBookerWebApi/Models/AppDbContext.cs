using Microsoft.EntityFrameworkCore;
using LiBookerWebApi.Models.Entities;

namespace LiBookerWebApi.Model
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Person> Persons => Set<Person>();
        public DbSet<Book> Books => Set<Book>();
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
        public DbSet<Genre> Genres => Set<Genre>();
        public DbSet<BookGenre> BookGenres => Set<BookGenre>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<BookCategory> BookCategories => Set<BookCategory>();
        public DbSet<Publisher> Publishers => Set<Publisher>();
        public DbSet<Language> Languages => Set<Language>();
        public DbSet<CoverImage> CoverImages => Set<CoverImage>();
        public DbSet<Publication> Publications => Set<Publication>();
        public DbSet<Copy> Copies => Set<Copy>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<Loan> Loans => Set<Loan>();
        public DbSet<Fine> Fines => Set<Fine>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // table 'osoba'
            modelBuilder.Entity<Person>(b =>
            {
                b.ToTable("osoba".ToUpper());
                b.HasKey(p => p.Id).HasName("pk_osoba".ToUpper());
                b.Property(p => p.Id).HasColumnName("id_osoby".ToUpper());
                b.Property(p => p.FirstName).HasColumnName("meno".ToUpper()).HasMaxLength(30);
                b.Property(p => p.LastName).HasColumnName("priezvisko".ToUpper()).HasMaxLength(35);
                b.Property(p => p.BirthDate).HasColumnName("dat_narodenia".ToUpper());
                b.Property(p => p.RegisteredAt).HasColumnName("dat_registracie".ToUpper());
                b.Property(p => p.Email).HasColumnName("email".ToUpper()).HasMaxLength(50);
                b.Property(p => p.Gender).HasColumnName("pohlavie".ToUpper()).HasMaxLength(1).HasColumnType("CHAR(1)");
                b.Property(p => p.Phone).HasColumnName("tel_cislo".ToUpper()).HasMaxLength(20);
            });

            // table 'kniha'
            modelBuilder.Entity<Book>(b =>
            {
                b.ToTable("kniha".ToUpper());
                b.HasKey(x => x.Id).HasName("pk_kniha".ToUpper());
                b.Property(x => x.Id).HasColumnName("id_knihy".ToUpper());
                b.Property(x => x.Title).HasColumnName("nazov".ToUpper()).HasMaxLength(50);
                b.Property(x => x.Description).HasColumnName("popis".ToUpper()).HasColumnType("CLOB");
            });

            // table 'autor'
            modelBuilder.Entity<Author>(b =>
            {
                b.ToTable("autor".ToUpper());
                b.HasKey(x => x.Id).HasName("pk_autor".ToUpper());
                b.Property(x => x.Id).HasColumnName("id_autora".ToUpper());
                b.Property(x => x.FirstName).HasColumnName("meno".ToUpper()).HasMaxLength(30);
                b.Property(x => x.LastName).HasColumnName("priezvisko".ToUpper()).HasMaxLength(35);
                b.Property(x => x.Nationality).HasColumnName("narodnost".ToUpper());
            });

            // table 'knihy' (join author-book)
            modelBuilder.Entity<BookAuthor>(b =>
            {
                b.ToTable("knihy".ToUpper());
                b.HasKey(x => new { x.AuthorId, x.BookId }).HasName("pk_knihy".ToUpper());
                b.Property(x => x.AuthorId).HasColumnName("id_autora".ToUpper());
                b.Property(x => x.BookId).HasColumnName("id_knihy".ToUpper());

                b.HasOne(x => x.Author).WithMany(a => a.BookAuthors).HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(x => x.Book).WithMany(bk => bk.BookAuthors).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
            });

            // table 'zaner'
            modelBuilder.Entity<Genre>(b =>
            {
                b.ToTable("zaner".ToUpper());
                b.HasKey(x => x.Id).HasName("pk_zaner".ToUpper());
                b.Property(x => x.Id).HasColumnName("id_zanra".ToUpper());
                b.Property(x => x.Name).HasColumnName("nazov".ToUpper()).HasMaxLength(20);
            });

            // table 'zanre_knih' (join)
            modelBuilder.Entity<BookGenre>(b =>
            {
                b.ToTable("zanre_knih".ToUpper());
                b.HasKey(x => new { x.GenreId, x.BookId }).HasName("pk_zanre_knih".ToUpper());
                b.Property(x => x.GenreId).HasColumnName("id_zanra".ToUpper());
                b.Property(x => x.BookId).HasColumnName("id_knihy".ToUpper());

                b.HasOne(x => x.Genre).WithMany(g => g.BookGenres).HasForeignKey(x => x.GenreId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(x => x.Book).WithMany(bk => bk.BookGenres).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
            });

            // table 'kategoria'
            modelBuilder.Entity<Category>(b =>
            {
                b.ToTable("kategoria".ToUpper());
                b.HasKey(x => x.Id).HasName("pk_kategoria".ToUpper());
                b.Property(x => x.Id).HasColumnName("id_kategorie".ToUpper());
                b.Property(x => x.Name).HasColumnName("nazov".ToUpper()).HasMaxLength(30);
            });

            // table 'kategorie_knih' (join)
            modelBuilder.Entity<BookCategory>(b =>
            {
                b.ToTable("kategorie_knih".ToUpper());
                b.HasKey(x => new { x.CategoryId, x.BookId }).HasName("pk_kategorie_knih".ToUpper());
                b.Property(x => x.CategoryId).HasColumnName("id_kategorie".ToUpper());
                b.Property(x => x.BookId).HasColumnName("id_knihy".ToUpper());

                b.HasOne(x => x.Category).WithMany(c => c.BookCategories).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(x => x.Book).WithMany(bk => bk.BookCategories).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
            });

            // table 'vydavatelstvo'
            modelBuilder.Entity<Publisher>(b =>
            {
                b.ToTable("vydavatelstvo".ToUpper());
                b.HasKey(x => x.Id).HasName("pk_vydavatelstvo".ToUpper());
                b.Property(x => x.Id).HasColumnName("id_vydavatelstva".ToUpper());
                b.Property(x => x.Name).HasColumnName("nazov".ToUpper()).HasMaxLength(30);
                b.Property(x => x.Description).HasColumnName("popis".ToUpper()).HasColumnType("CLOB");
            });

            // table 'jazyk'
            modelBuilder.Entity<Language>(b =>
            {
                b.ToTable("jazyk".ToUpper());
                b.HasKey(x => x.Id).HasName("pk_jazyk".ToUpper());
                b.Property(x => x.Id).HasColumnName("id_jazyka".ToUpper());
                b.Property(x => x.Name).HasColumnName("nazov".ToUpper()).HasMaxLength(30);
                b.Property(x => x.Code).HasColumnName("skratka".ToUpper()).HasMaxLength(3);
            });

            // table 'titulny_obrazok'
            modelBuilder.Entity<CoverImage>(b =>
            {
                b.ToTable("titulny_obrazok".ToUpper());
                b.HasKey(x => x.Id).HasName("pk_titulny_obrazok".ToUpper());
                b.Property(x => x.Id).HasColumnName("id_obrazku".ToUpper());
                b.Property(x => x.Image).HasColumnName("obrazok".ToUpper()).HasColumnType("BLOB");
            });

            // table 'vydanie'
            modelBuilder.Entity<Publication>(b =>
            {
                b.ToTable("vydanie".ToUpper());
                b.HasKey(x => x.Id).HasName("pk_vydanie".ToUpper());
                b.Property(x => x.Id).HasColumnName("id_vydania".ToUpper());
                b.Property(x => x.BookId).HasColumnName("id_knihy".ToUpper());
                b.Property(x => x.PublisherId).HasColumnName("id_vydavatelstva".ToUpper());
                b.Property(x => x.LanguageId).HasColumnName("id_jazyka".ToUpper());
                b.Property(x => x.CoverImageId).HasColumnName("id_obrazku".ToUpper());
                b.Property(x => x.ISBN).HasColumnName("ISBN".ToUpper()).HasMaxLength(17);
                b.Property(x => x.Year).HasColumnName("rok_vydania".ToUpper());
                b.Property(x => x.EditionNumber).HasColumnName("cislo_vydania".ToUpper());
                b.Property(x => x.PageCount).HasColumnName("pocet_stran".ToUpper());
                b.Property(x => x.XmlProperties).HasColumnName("vlastnosti_xml".ToUpper()).HasColumnType("XMLTYPE");

                b.HasOne(x => x.Book).WithMany(bk => bk.Publications).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Publisher).WithMany(p => p.Publications).HasForeignKey(x => x.PublisherId).OnDelete(DeleteBehavior.SetNull);
                b.HasOne(x => x.Language).WithMany(l => l.Publications).HasForeignKey(x => x.LanguageId).OnDelete(DeleteBehavior.SetNull);
                b.HasOne(x => x.CoverImage).WithMany(ci => ci.Publications).HasForeignKey(x => x.CoverImageId).OnDelete(DeleteBehavior.SetNull);
            });

            // table 'vytlacok' (copy)
            modelBuilder.Entity<Copy>(b =>
            {
                b.ToTable("vytlacok".ToUpper());
                b.HasKey(x => x.Id).HasName("pk_vytlacok".ToUpper());
                b.Property(x => x.Id).HasColumnName("id_vytlacka".ToUpper());
                b.Property(x => x.PublicationId).HasColumnName("id_vydania".ToUpper());
                b.Property(x => x.Status).HasColumnName("stav".ToUpper()).HasMaxLength(15);

                b.HasOne(x => x.Publication).WithMany(p => p.Copies).HasForeignKey(x => x.PublicationId).OnDelete(DeleteBehavior.Cascade);
            });

            // table 'rezervacia'
            modelBuilder.Entity<Reservation>(b =>
            {
                b.ToTable("rezervacia".ToUpper());
                b.HasKey(x => x.Id).HasName("pk_rezervacia".ToUpper());
                b.Property(x => x.Id).HasColumnName("id_rezervacie".ToUpper());
                b.Property(x => x.PublicationId).HasColumnName("vydanie_id_vydania".ToUpper());
                b.Property(x => x.PersonId).HasColumnName("osoba_id_osoby".ToUpper());
                b.Property(x => x.ReservedAt).HasColumnName("dat_rezervacie".ToUpper());

                b.HasOne(x => x.Publication).WithMany(p => p.Reservations).HasForeignKey(x => x.PublicationId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Person).WithMany(pr => pr.Reservations).HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.Cascade);
            });

            // table 'vypozicanie' (loan)
            modelBuilder.Entity<Loan>(b =>
            {
                b.ToTable("vypozicanie".ToUpper());
                b.HasKey(x => x.Id).HasName("pk_vypozicanie".ToUpper());
                b.Property(x => x.Id).HasColumnName("id_vypozicania".ToUpper());
                b.Property(x => x.PersonId).HasColumnName("osoba_id_osoby".ToUpper());
                b.Property(x => x.CopyId).HasColumnName("vytlacok_id_vytlacka".ToUpper());
                b.Property(x => x.LoanedAt).HasColumnName("dat_vypozicania".ToUpper());
                b.Property(x => x.DueAt).HasColumnName("dat_konca_vypoz".ToUpper());
                b.Property(x => x.ReturnedAt).HasColumnName("dat_vratenia".ToUpper());
                b.Property(x => x.FineId).HasColumnName("o_pokuta".ToUpper());

                b.HasOne(x => x.Person).WithMany(p => p.Loans).HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(x => x.Copy).WithMany(c => c.Loans).HasForeignKey(x => x.CopyId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Fine).WithMany(f => f.Loans).HasForeignKey(x => x.FineId).OnDelete(DeleteBehavior.SetNull);
            });

            // table 'pokuta'
            modelBuilder.Entity<Fine>(b =>
            {
                b.ToTable("pokuta".ToUpper());
                b.HasKey(x => x.Id).HasName("pk_pokuta".ToUpper());
                b.Property(x => x.Id).HasColumnName("id_pokuty".ToUpper());
                b.Property(x => x.PaidAt).HasColumnName("dat_zaplatenia".ToUpper());
                b.Property(x => x.Amount).HasColumnName("cena".ToUpper()).HasColumnType("NUMBER");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}