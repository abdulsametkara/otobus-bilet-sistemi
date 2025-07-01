using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OtobusBiletSistemi.Core.Entities;

namespace OtobusBiletSistemi.Core.Data
{
    public class YolcuUser : IdentityUser<int>
    {
        public override int Id { get; set; }
        
        public string TCNo { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string TelNo { get; set; }
        public override string Email { get; set; }
        
        public string FullName => $"{Ad} {Soyad}";
    }
    
    public class AppDbContext : IdentityDbContext<YolcuUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Bilet> Biletler { get; set; }
        public DbSet<Guzergah> Guzergahlar { get; set; }
        public DbSet<Koltuk> Koltuklar { get; set; }
        public DbSet<Odeme> Odemeler { get; set; }
        public DbSet<Otobus> Otobusler { get; set; }
        public DbSet<Sefer> Seferler { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 
            
            modelBuilder.Entity<YolcuUser>().ToTable("YOLCU");
            modelBuilder.Entity<YolcuUser>().Property(x => x.Id)
                .HasColumnName("YOLCUID")
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<YolcuUser>().Property(x => x.TCNo).HasColumnName("TCNO");
            modelBuilder.Entity<YolcuUser>().Property(x => x.Ad).HasColumnName("AD");
            modelBuilder.Entity<YolcuUser>().Property(x => x.Soyad).HasColumnName("SOYAD");
            modelBuilder.Entity<YolcuUser>().Property(x => x.TelNo).HasColumnName("TELNO");
            modelBuilder.Entity<YolcuUser>().Property(x => x.Email).HasColumnName("EMAIL");
            modelBuilder.Entity<YolcuUser>().Property(x => x.UserName).HasColumnName("USERNAME");
            
            modelBuilder.Entity<YolcuUser>().Ignore(x => x.PhoneNumber);
            modelBuilder.Entity<YolcuUser>().Ignore(x => x.PhoneNumberConfirmed);
            
            modelBuilder.Entity<YolcuUser>().Property(x => x.NormalizedUserName).HasColumnName("NORMALIZEDUSERNAME");
            modelBuilder.Entity<YolcuUser>().Property(x => x.NormalizedEmail).HasColumnName("NORMALIZEDEMAIL");
            modelBuilder.Entity<YolcuUser>().Property(x => x.PasswordHash).HasColumnName("PASSWORDHASH");
            modelBuilder.Entity<YolcuUser>().Property(x => x.SecurityStamp).HasColumnName("SECURITYSTAMP");
            modelBuilder.Entity<YolcuUser>().Property(x => x.ConcurrencyStamp).HasColumnName("CONCURRENCYSTAMP");
            modelBuilder.Entity<YolcuUser>().Property(x => x.LockoutEnd).HasColumnName("LOCKOUTEND");
            modelBuilder.Entity<YolcuUser>().Property(x => x.AccessFailedCount).HasColumnName("ACCESSFAILEDCOUNT");
            
            modelBuilder.Entity<YolcuUser>().Property(x => x.EmailConfirmed)
                .HasColumnName("EMAILCONFIRMED")
                .HasConversion<int>();
                
            modelBuilder.Entity<YolcuUser>().Property(x => x.TwoFactorEnabled)
                .HasColumnName("TWOFACTORENABLED")
                .HasConversion<int>();
                
            modelBuilder.Entity<YolcuUser>().Property(x => x.LockoutEnabled)
                .HasColumnName("LOCKOUTENABLED")
                .HasConversion<int>();
            
            // Identity tablolarının mapping'i
            modelBuilder.Entity<IdentityRole<int>>().ToTable("ASPNET_ROLES");
            modelBuilder.Entity<IdentityRole<int>>().Property(x => x.Id).HasColumnName("ID");
            modelBuilder.Entity<IdentityRole<int>>().Property(x => x.Name).HasColumnName("NAME");
            modelBuilder.Entity<IdentityRole<int>>().Property(x => x.NormalizedName).HasColumnName("NORMALIZEDNAME");
            modelBuilder.Entity<IdentityRole<int>>().Property(x => x.ConcurrencyStamp).HasColumnName("CONCURRENCYSTAMP");

            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("ASPNET_USER_ROLES");
            modelBuilder.Entity<IdentityUserRole<int>>().Property(x => x.UserId).HasColumnName("USERID");
            modelBuilder.Entity<IdentityUserRole<int>>().Property(x => x.RoleId).HasColumnName("ROLEID");

            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("ASPNET_USER_CLAIMS");
            modelBuilder.Entity<IdentityUserClaim<int>>().Property(x => x.Id).HasColumnName("ID");
            modelBuilder.Entity<IdentityUserClaim<int>>().Property(x => x.UserId).HasColumnName("USERID");
            modelBuilder.Entity<IdentityUserClaim<int>>().Property(x => x.ClaimType).HasColumnName("CLAIMTYPE");
            modelBuilder.Entity<IdentityUserClaim<int>>().Property(x => x.ClaimValue).HasColumnName("CLAIMVALUE");

            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("ASPNET_USER_LOGINS");
            modelBuilder.Entity<IdentityUserLogin<int>>().Property(x => x.LoginProvider).HasColumnName("LOGINPROVIDER");
            modelBuilder.Entity<IdentityUserLogin<int>>().Property(x => x.ProviderKey).HasColumnName("PROVIDERKEY");
            modelBuilder.Entity<IdentityUserLogin<int>>().Property(x => x.ProviderDisplayName).HasColumnName("PROVIDERDISPLAYNAME");
            modelBuilder.Entity<IdentityUserLogin<int>>().Property(x => x.UserId).HasColumnName("USERID");

            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("ASPNET_USER_TOKENS");
            modelBuilder.Entity<IdentityUserToken<int>>().Property(x => x.UserId).HasColumnName("USERID");
            modelBuilder.Entity<IdentityUserToken<int>>().Property(x => x.LoginProvider).HasColumnName("LOGINPROVIDER");
            modelBuilder.Entity<IdentityUserToken<int>>().Property(x => x.Name).HasColumnName("NAME");
            modelBuilder.Entity<IdentityUserToken<int>>().Property(x => x.Value).HasColumnName("VALUE");

            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("ASPNET_ROLE_CLAIMS");
            modelBuilder.Entity<IdentityRoleClaim<int>>().Property(x => x.Id).HasColumnName("ID");
            modelBuilder.Entity<IdentityRoleClaim<int>>().Property(x => x.RoleId).HasColumnName("ROLEID");
            modelBuilder.Entity<IdentityRoleClaim<int>>().Property(x => x.ClaimType).HasColumnName("CLAIMTYPE");
            modelBuilder.Entity<IdentityRoleClaim<int>>().Property(x => x.ClaimValue).HasColumnName("CLAIMVALUE");

            modelBuilder.Entity<Bilet>().ToTable("BILET");
            modelBuilder.Entity<Guzergah>().ToTable("GUZERGAH");
            modelBuilder.Entity<Koltuk>().ToTable("KOLTUK");
            modelBuilder.Entity<Odeme>().ToTable("ODEME");
            modelBuilder.Entity<Otobus>().ToTable("OTOBUS");
            modelBuilder.Entity<Sefer>().ToTable("SEFER");

            modelBuilder.Entity<Bilet>().HasKey(x => x.BiletID);
            modelBuilder.Entity<Guzergah>().HasKey(x => x.GuzergahID);
            modelBuilder.Entity<Koltuk>().HasKey(x => x.KoltukID);
            modelBuilder.Entity<Odeme>().HasKey(x => x.OdemeID);
            modelBuilder.Entity<Otobus>().HasKey(x => x.OtobusID);
            modelBuilder.Entity<Sefer>().HasKey(x => x.SeferID);

            modelBuilder.Entity<Bilet>().Property(x => x.BiletID).HasColumnName("BILETID").ValueGeneratedOnAdd();
            modelBuilder.Entity<Bilet>().Property(x => x.SeferID).HasColumnName("SEFERID");
            modelBuilder.Entity<Bilet>().Property(x => x.KoltukID).HasColumnName("KOLTUKID");
            modelBuilder.Entity<Bilet>().Property(x => x.YolcuID).HasColumnName("YOLCUID");
            modelBuilder.Entity<Bilet>().Property(x => x.OdemeID).HasColumnName("ODEMEID");
            modelBuilder.Entity<Bilet>().Property(x => x.BiletTarihi).HasColumnName("BILETTARIHI");
            modelBuilder.Entity<Bilet>().Property(x => x.BiletDurumu).HasColumnName("BILETDURUMU");
            
            // BiletNo computed property'sini ignore et
            modelBuilder.Entity<Bilet>().Ignore(x => x.BiletNo);

            modelBuilder.Entity<Guzergah>().Property(x => x.GuzergahID).HasColumnName("GUZERGAHID").ValueGeneratedOnAdd();
            modelBuilder.Entity<Guzergah>().Property(x => x.Nereden).HasColumnName("NEREDEN");
            modelBuilder.Entity<Guzergah>().Property(x => x.Nereye).HasColumnName("NEREYE");
            modelBuilder.Entity<Guzergah>().Property(x => x.Mesafe).HasColumnName("MESAFE");

            modelBuilder.Entity<Koltuk>().Property(x => x.KoltukID).HasColumnName("KOLTUKID").ValueGeneratedOnAdd();
            modelBuilder.Entity<Koltuk>().Property(x => x.OtobusID).HasColumnName("OTOBUSID");
            modelBuilder.Entity<Koltuk>().Property(x => x.KoltukNo).HasColumnName("KOLTUKNO");

            modelBuilder.Entity<Odeme>().Property(x => x.OdemeID).HasColumnName("ODEMEID").ValueGeneratedOnAdd();
            modelBuilder.Entity<Odeme>().Property(x => x.YolcuID).HasColumnName("YOLCUID");
            modelBuilder.Entity<Odeme>().Property(x => x.OdemeTarihi).HasColumnName("ODEMETARIHI");
            modelBuilder.Entity<Odeme>().Property(x => x.OdemeTutari).HasColumnName("TOPLAMTUTAR").HasColumnType("NUMBER(10,2)");
            modelBuilder.Entity<Odeme>().Property(x => x.OdemeYontemi).HasColumnName("ODEMETIPI");
            modelBuilder.Entity<Odeme>().Property(x => x.OdemeDurumu).HasColumnName("ONAYDURUMU");
            modelBuilder.Entity<Odeme>().Property(x => x.KartSahibiAdi).HasColumnName("KARTSAHIBIADI");
            modelBuilder.Entity<Odeme>().Property(x => x.KartNumarasi).HasColumnName("KARTNUMARASI");
            modelBuilder.Entity<Odeme>().Property(x => x.BiletSayisi).HasColumnName("BILETSAYISI");
            
            // Backward compatibility - ignore the wrapper properties
            modelBuilder.Entity<Odeme>().Ignore(x => x.ToplamTutar);
            modelBuilder.Entity<Odeme>().Ignore(x => x.OdemeTipi);
            modelBuilder.Entity<Odeme>().Ignore(x => x.OnayDurumu);

            modelBuilder.Entity<Otobus>().Property(x => x.OtobusID).HasColumnName("OTOBUSID").ValueGeneratedOnAdd();
            modelBuilder.Entity<Otobus>().Property(x => x.Plaka).HasColumnName("PLAKA");
            modelBuilder.Entity<Otobus>().Property(x => x.OtobusTipi).HasColumnName("OTOBUSTIPI");
            modelBuilder.Entity<Otobus>().Property(x => x.KoltukSayısı).HasColumnName("KOLTUKSAYISI");

            modelBuilder.Entity<Sefer>().Property(x => x.SeferID).HasColumnName("SEFERID").ValueGeneratedOnAdd();
            modelBuilder.Entity<Sefer>().Property(x => x.OtobusID).HasColumnName("OTOBUSID");
            modelBuilder.Entity<Sefer>().Property(x => x.GuzergahID).HasColumnName("GUZERGAHID");
            modelBuilder.Entity<Sefer>().Property(x => x.Tarih).HasColumnName("TARIH");
            modelBuilder.Entity<Sefer>().Property(x => x.Saat).HasColumnName("SAAT");
            modelBuilder.Entity<Sefer>().Property(x => x.Kalkis).HasColumnName("KALKMATERMINALI");
            modelBuilder.Entity<Sefer>().Property(x => x.Varis).HasColumnName("VARISTERMINALI");
            modelBuilder.Entity<Sefer>().Property(x => x.Fiyat).HasColumnName("FIYAT").HasColumnType("NUMBER(10,2)");
        }
    }
}