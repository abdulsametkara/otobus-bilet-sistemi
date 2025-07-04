using OtobusBiletSistemi.Mobile.Models;
using OtobusBiletSistemi.Core.Interfaces;
using CoreEntities = OtobusBiletSistemi.Core.Entities;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OtobusBiletSistemi.Core.Data;
using OtobusBiletSistemi.Core.Repositories;

namespace OtobusBiletSistemi.Mobile.Services
{
    public class ApiService : IApiService
    {
        private readonly string _connectionString;

        public ApiService()
        {
            // Web projesi ile aynı kullanıcı bilgileri kullan
            // User Id: OTOBUS_USER (büyük harf), Password: otobus123
            _connectionString = "Data Source=10.0.2.2:1521/XEPDB1;User Id=OTOBUS_USER;Password=otobus123;";
            Debug.WriteLine("ApiService - Web projesi ile aynı kullanıcı bilgileri kullanılıyor.");
            Debug.WriteLine("User: OTOBUS_USER, Database: XEPDB1, Host: 10.0.2.2:1521");
        }

        // Her işlem için yeni DbContext oluştur
        private AppDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseOracle(_connectionString);
            return new AppDbContext(optionsBuilder.Options);
        }

        #region Güzergah İşlemleri

        public async Task<List<Models.Guzergah>> GetGuzergahlarAsync()
        {
            try
            {
                Debug.WriteLine("GetGuzergahlarAsync - Veritabanından güzergahlar alınıyor...");
                
                var directGuzergahlar = await CreateDbContext().Guzergahlar.AsNoTracking().ToListAsync();
                Debug.WriteLine($"DbContext üzerinden direkt erişim: {directGuzergahlar.Count} güzergah bulundu");
                
                var mobileGuzergahlar = directGuzergahlar.Select(g => new Models.Guzergah
                {
                    GuzergahID = g.GuzergahID,
                    KalkisYeri = g.Nereden,
                    VarisYeri = g.Nereye,
                    Mesafe = g.Mesafe
                }).ToList();
                
                Debug.WriteLine($"✅ Veritabanından {mobileGuzergahlar.Count} güzergah başarıyla alındı");
                foreach (var g in mobileGuzergahlar)
                {
                    Debug.WriteLine($"Güzergah: {g.GuzergahID} | {g.KalkisYeri} -> {g.VarisYeri} ({g.Mesafe} km)");
                }
                
                return mobileGuzergahlar;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ GetGuzergahlarAsync HATA: {ex.Message}");
                Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        #endregion

        #region Sefer İşlemleri

        public async Task<List<Models.Sefer>> GetSeferlerAsync()
        {
            try
            {
                Debug.WriteLine("GetSeferlerAsync başlatıldı...");
                
                // Seferler ve Güzergahları ayrı ayrı çek
                var coreSeferler = await CreateDbContext().Seferler.AsNoTracking().ToListAsync();
                var coreGuzergahlar = await CreateDbContext().Guzergahlar.AsNoTracking().ToListAsync();
                
                // Core entity'lerini Mobile model'lerine dönüştür
                var mobileSeferler = new List<Models.Sefer>();
                
                foreach (var sefer in coreSeferler)
                {
                    // Sefer'in bağlı olduğu Güzergah'ı bul
                    var guzergah = coreGuzergahlar.FirstOrDefault(g => g.GuzergahID == sefer.GuzergahID);
                    
                    var mobileSefer = new Models.Sefer
                    {
                        SeferID = sefer.SeferID,
                        OtobusID = sefer.OtobusID,
                        GuzergahID = sefer.GuzergahID,
                        Tarih = sefer.Tarih,
                        Saat = sefer.Saat,
                        Kalkis = sefer.Kalkis,
                        Varis = sefer.Varis,
                        Fiyat = sefer.Fiyat,
                        BosKoltukSayisi = 40 // Varsayılan değer, ileride hesaplanabilir
                    };
                    
                    // Güzergah bilgilerini ekle
                    if (guzergah != null)
                    {
                        mobileSefer.Guzergah = new Models.Guzergah
                        {
                            GuzergahID = guzergah.GuzergahID,
                            KalkisYeri = guzergah.Nereden,
                            VarisYeri = guzergah.Nereye,
                            Mesafe = guzergah.Mesafe
                        };
                        
                        Debug.WriteLine($"Sefer {sefer.SeferID}: Güzergah ilişkisi kuruldu - {guzergah.Nereden} - {guzergah.Nereye}");
                    }
                    else
                    {
                        Debug.WriteLine($"UYARI: Sefer {sefer.SeferID} için Güzergah bulunamadı (ID: {sefer.GuzergahID})");
                    }
                    
                    mobileSeferler.Add(mobileSefer);
                }

                Debug.WriteLine($"✅ Veritabanından {mobileSeferler.Count} sefer alındı");
                return mobileSeferler;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ GetSeferlerAsync Hatası: {ex.Message}");
                Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                throw; // Exception'ı yukarı fırlat ki gerçek hatayı görelim
            }
        }

        public async Task<Models.Sefer?> GetSeferAsync(int seferID)
        {
            try
            {
                var coreSefer = await CreateDbContext().Seferler.AsNoTracking().FirstOrDefaultAsync(s => s.SeferID == seferID);
                if (coreSefer == null) return null;

                return new Models.Sefer
                {
                    SeferID = coreSefer.SeferID,
                    OtobusID = coreSefer.OtobusID,
                    GuzergahID = coreSefer.GuzergahID,
                    Tarih = coreSefer.Tarih,
                    Saat = coreSefer.Saat,
                    Kalkis = coreSefer.Kalkis,
                    Varis = coreSefer.Varis,
                    Fiyat = coreSefer.Fiyat,
                    BosKoltukSayisi = 40
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetSeferAsync Hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Models.Sefer>> SearchSeferlerAsync(string kalkis, string varis, DateTime tarih)
        {
            try
            {
                var tumSeferler = await GetSeferlerAsync();
                
                return tumSeferler.Where(s => 
                    s.Kalkis.Contains(kalkis, StringComparison.OrdinalIgnoreCase) &&
                    s.Varis.Contains(varis, StringComparison.OrdinalIgnoreCase) &&
                    s.Tarih.Date == tarih.Date
                ).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ SearchSeferlerAsync Hatası: {ex.Message}");
                return new List<Models.Sefer>();
            }
        }

        #endregion

        #region Koltuk İşlemleri

        public async Task<List<Models.Koltuk>> GetKoltuklerAsync()
        {
            try
            {
                Debug.WriteLine("GetKoltuklerAsync - Tüm koltuklar alınıyor...");
                
                var coreKoltuklar = await CreateDbContext().Koltuklar.AsNoTracking().ToListAsync();
                var mobileKoltuklar = coreKoltuklar.Select(k => new Models.Koltuk
                {
                    KoltukID = k.KoltukID,
                    OtobusID = k.OtobusID,
                    KoltukNo = k.KoltukNo
                }).ToList();
                
                Debug.WriteLine($"✅ {mobileKoltuklar.Count} koltuk alındı");
                return mobileKoltuklar;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ GetKoltuklerAsync Hatası: {ex.Message}");
                Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<List<Models.Koltuk>> GetKoltuklarAsync(int seferID)
        {
            try
            {
                var coreKoltuklar = await CreateDbContext().Koltuklar.AsNoTracking().ToListAsync();
                
                var mobileKoltuklar = coreKoltuklar.Select(k => new Models.Koltuk
                {
                    KoltukID = k.KoltukID,
                    OtobusID = k.OtobusID,
                    KoltukNo = k.KoltukNo
                }).ToList();

                return mobileKoltuklar;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetKoltuklarAsync Hatası: {ex.Message}");
                return new List<Models.Koltuk>();
            }
        }

        public async Task<Models.Koltuk?> GetKoltukAsync(int koltukID)
        {
            try
            {
                var coreKoltuk = await CreateDbContext().Koltuklar.AsNoTracking().FirstOrDefaultAsync(k => k.KoltukID == koltukID);
                if (coreKoltuk == null) return null;

                return new Models.Koltuk
                {
                    KoltukID = coreKoltuk.KoltukID,
                    OtobusID = coreKoltuk.OtobusID,
                    KoltukNo = coreKoltuk.KoltukNo
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetKoltukAsync Hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<Models.Koltuk?> CreateKoltukAsync(Models.Koltuk koltuk)
        {
            try
            {
                var coreKoltuk = new CoreEntities.Koltuk
                {
                    OtobusID = koltuk.OtobusID,
                    KoltukNo = koltuk.KoltukNo
                };

                using var context = CreateDbContext();
                var createdKoltuk = await context.Koltuklar.AddAsync(coreKoltuk);
                await context.SaveChangesAsync();
                
                return new Models.Koltuk
                {
                    KoltukID = createdKoltuk.Entity.KoltukID,
                    OtobusID = createdKoltuk.Entity.OtobusID,
                    KoltukNo = createdKoltuk.Entity.KoltukNo
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CreateKoltukAsync Hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> EnsureKoltuklarExistAsync(int otobusID, int koltukSayisi = 50)
        {
            try
            {
                Debug.WriteLine($"🔧 EnsureKoltuklarExistAsync başlıyor - OtobusID: {otobusID}, KoltukSayisi: {koltukSayisi}");
                
                // Mevcut koltukları kontrol et
                using var context = CreateDbContext();
                var otobusKoltuklari = await context.Koltuklar.AsNoTracking()
                    .Where(k => k.OtobusID == otobusID).ToListAsync();
                
                Debug.WriteLine($"📋 OtobusID {otobusID} için {otobusKoltuklari.Count} mevcut koltuk bulundu");
                
                if (otobusKoltuklari.Count >= koltukSayisi)
                {
                    Debug.WriteLine($"✅ Yeterli koltuk mevcut ({otobusKoltuklari.Count}/{koltukSayisi})");
                    return true;
                }
                
                // Eksik koltukları oluştur
                var yeniKoltuklar = new List<CoreEntities.Koltuk>();
                for (int sira = 1; sira <= 17; sira++)
                {
                    var positions = sira == 17 ? new[] { "A", "B" } : new[] { "A", "B", "C" };
                    
                    foreach (var pos in positions)
                    {
                        var koltukNo = $"{sira}{pos}";
                        if (!otobusKoltuklari.Any(k => k.KoltukNo == koltukNo))
                        {
                            yeniKoltuklar.Add(new CoreEntities.Koltuk
                            {
                                OtobusID = otobusID,
                                KoltukNo = koltukNo
                            });
                            Debug.WriteLine($"   ✅ Koltuk oluşturulacak: {koltukNo}");
                        }
                    }
                }
                
                if (yeniKoltuklar.Any())
                {
                    using var newContext = CreateDbContext();
                    await newContext.Koltuklar.AddRangeAsync(yeniKoltuklar);
                    await newContext.SaveChangesAsync();
                    Debug.WriteLine($"💾 {yeniKoltuklar.Count} yeni koltuk veritabanına kaydedildi");
                }
                
                Debug.WriteLine("✅ Koltuklar hazır!");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ EnsureKoltuklarExistAsync Hatası: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Bilet İşlemleri

        public async Task<List<Models.Bilet>> GetBiletlerAsync()
        {
            try
            {
                var coreBiletler = await CreateDbContext().Biletler.AsNoTracking().ToListAsync();
                
                var mobileBiletler = coreBiletler.Select(b => new Models.Bilet
                {
                    BiletID = b.BiletID,
                    SeferID = b.SeferID,
                    KoltukID = b.KoltukID,
                    YolcuID = b.YolcuID,
                    OdemeID = b.OdemeID,
                    BiletTarihi = b.BiletTarihi,
                    BiletDurumu = b.BiletDurumu
                }).ToList();

                return mobileBiletler;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetBiletlerAsync Hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<List<Models.Bilet>> GetBiletlerByYolcuAsync(int yolcuID)
        {
            try
            {
                var tumBiletler = await GetBiletlerAsync();
                return tumBiletler.Where(b => b.YolcuID == yolcuID).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetBiletlerByYolcuAsync Hatası: {ex.Message}");
                return new List<Models.Bilet>();
            }
        }

        public async Task<Models.Bilet?> GetBiletAsync(int biletID)
        {
            try
            {
                var coreBilet = await CreateDbContext().Biletler.AsNoTracking().FirstOrDefaultAsync(b => b.BiletID == biletID);
                if (coreBilet == null) return null;

                return new Models.Bilet
                {
                    BiletID = coreBilet.BiletID,
                    SeferID = coreBilet.SeferID,
                    KoltukID = coreBilet.KoltukID,
                    YolcuID = coreBilet.YolcuID,
                    OdemeID = coreBilet.OdemeID,
                    BiletTarihi = coreBilet.BiletTarihi,
                    BiletDurumu = coreBilet.BiletDurumu
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetBiletAsync Hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<Models.Bilet?> CreateBiletAsync(Models.Bilet bilet)
        {
            try
            {
                var coreBilet = new CoreEntities.Bilet
                {
                    SeferID = bilet.SeferID,
                    KoltukID = bilet.KoltukID,
                    YolcuID = bilet.YolcuID,
                    OdemeID = bilet.OdemeID,
                    BiletTarihi = bilet.BiletTarihi,
                    BiletDurumu = bilet.BiletDurumu
                };

                using var context = CreateDbContext();
                var createdBilet = await context.Biletler.AddAsync(coreBilet);
                await context.SaveChangesAsync();
                
                return new Models.Bilet
                {
                    BiletID = createdBilet.Entity.BiletID,
                    SeferID = createdBilet.Entity.SeferID,
                    KoltukID = createdBilet.Entity.KoltukID,
                    YolcuID = createdBilet.Entity.YolcuID,
                    OdemeID = createdBilet.Entity.OdemeID,
                    BiletTarihi = createdBilet.Entity.BiletTarihi,
                    BiletDurumu = createdBilet.Entity.BiletDurumu
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CreateBiletAsync Hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateBiletAsync(Models.Bilet bilet)
        {
            try
            {
                using var context = CreateDbContext();
                var coreBilet = await context.Biletler.FirstOrDefaultAsync(b => b.BiletID == bilet.BiletID);
                if (coreBilet == null) return false;

                coreBilet.SeferID = bilet.SeferID;
                coreBilet.KoltukID = bilet.KoltukID;
                coreBilet.YolcuID = bilet.YolcuID;
                coreBilet.OdemeID = bilet.OdemeID;
                coreBilet.BiletTarihi = bilet.BiletTarihi;
                coreBilet.BiletDurumu = bilet.BiletDurumu;

                context.Biletler.Update(coreBilet);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ UpdateBiletAsync Hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteBiletAsync(int biletID)
        {
            try
            {
                using var context = CreateDbContext();
                var coreBilet = await context.Biletler.FirstOrDefaultAsync(b => b.BiletID == biletID);
                if (coreBilet == null) return false;

                context.Biletler.Remove(coreBilet);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ DeleteBiletAsync Hatası: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Yolcu İşlemleri (Stub implementations)

        public async Task<List<Models.YolcuUser>> GetYolcularAsync()
        {
            // Identity User'ları için ayrı repository gerekli
            return new List<Models.YolcuUser>();
        }

        public async Task<Models.YolcuUser?> GetYolcuAsync(int yolcuID)
        {
            // Identity User'ları için ayrı repository gerekli
            return null;
        }

        public async Task<Models.YolcuUser?> CreateYolcuAsync(Models.YolcuUser yolcu)
        {
            // Identity User'ları için ayrı repository gerekli
            return null;
        }

        #endregion

        #region Ödeme İşlemleri

        public async Task<Models.Odeme?> CreateOdemeAsync(Models.Odeme odeme)
        {
            try
            {
                var coreOdeme = new CoreEntities.Odeme
                {
                    YolcuID = odeme.YolcuID,
                    OdemeTarihi = odeme.OdemeTarihi,
                    OdemeTutari = odeme.OdemeTutari,
                    OdemeYontemi = odeme.OdemeYontemi,
                    OdemeDurumu = odeme.OdemeDurumu,
                    KartSahibiAdi = odeme.KartSahibiAdi,
                    KartNumarasi = odeme.KartNumarasi,
                    BiletSayisi = odeme.BiletSayisi
                };

                using var context = CreateDbContext();
                var createdOdeme = await context.Odemeler.AddAsync(coreOdeme);
                await context.SaveChangesAsync();
                
                return new Models.Odeme
                {
                    OdemeID = createdOdeme.Entity.OdemeID,
                    YolcuID = createdOdeme.Entity.YolcuID,
                    OdemeTarihi = createdOdeme.Entity.OdemeTarihi,
                    OdemeTutari = createdOdeme.Entity.OdemeTutari,
                    OdemeYontemi = createdOdeme.Entity.OdemeYontemi,
                    OdemeDurumu = createdOdeme.Entity.OdemeDurumu,
                    KartSahibiAdi = createdOdeme.Entity.KartSahibiAdi,
                    KartNumarasi = createdOdeme.Entity.KartNumarasi,
                    BiletSayisi = createdOdeme.Entity.BiletSayisi
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CreateOdemeAsync Hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<Models.Odeme?> GetOdemeAsync(int odemeID)
        {
            try
            {
                var coreOdeme = await CreateDbContext().Odemeler.AsNoTracking().FirstOrDefaultAsync(o => o.OdemeID == odemeID);
                if (coreOdeme == null) return null;

                return new Models.Odeme
                {
                    OdemeID = coreOdeme.OdemeID,
                    YolcuID = coreOdeme.YolcuID,
                    OdemeTarihi = coreOdeme.OdemeTarihi,
                    OdemeTutari = coreOdeme.OdemeTutari,
                    OdemeYontemi = coreOdeme.OdemeYontemi,
                    OdemeDurumu = coreOdeme.OdemeDurumu,
                    KartSahibiAdi = coreOdeme.KartSahibiAdi,
                    KartNumarasi = coreOdeme.KartNumarasi,
                    BiletSayisi = coreOdeme.BiletSayisi
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetOdemeAsync Hatası: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Otobüs İşlemleri

        public async Task<List<Models.Otobus>> GetOtobuslerAsync()
        {
            try
            {
                var coreOtobusler = await CreateDbContext().Otobusler.AsNoTracking().ToListAsync();
                
                var mobileOtobusler = coreOtobusler.Select(o => new Models.Otobus
                {
                    OtobusID = o.OtobusID,
                    Plaka = o.Plaka,
                    OtobusTipi = o.OtobusTipi,
                    KoltukSayısı = o.KoltukSayısı
                }).ToList();

                return mobileOtobusler;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetOtobuslerAsync Hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Models.Otobus?> GetOtobusAsync(int otobusID)
        {
            try
            {
                var coreOtobus = await CreateDbContext().Otobusler.AsNoTracking().FirstOrDefaultAsync(o => o.OtobusID == otobusID);
                if (coreOtobus == null) return null;

                return new Models.Otobus
                {
                    OtobusID = coreOtobus.OtobusID,
                    Plaka = coreOtobus.Plaka,
                    OtobusTipi = coreOtobus.OtobusTipi,
                    KoltukSayısı = coreOtobus.KoltukSayısı
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetOtobusAsync Hatası: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Generic API Methods

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            // Bu method'lar placeholder - gerçek HTTP API çağrıları için
            // şimdilik repository'lerden veri döndürüyoruz
            
            try
            {
                if (typeof(T) == typeof(Models.Sefer))
                {
                    var parts = endpoint.Split('/');
                    if (parts.Length >= 3 && int.TryParse(parts[2], out int seferId))
                    {
                        var sefer = await GetSeferAsync(seferId);
                        return (T?)(object?)sefer;
                    }
                }
                else if (typeof(T) == typeof(Models.Otobus))
                {
                    var parts = endpoint.Split('/');
                    if (parts.Length >= 3 && int.TryParse(parts[2], out int otobusId))
                    {
                        var otobus = await GetOtobusAsync(otobusId);
                        return (T?)(object?)otobus;
                    }
                }
                else if (typeof(T) == typeof(Models.Guzergah))
                {
                    var parts = endpoint.Split('/');
                    if (parts.Length >= 3 && int.TryParse(parts[2], out int guzergahId))
                    {
                        var guzergahlar = await GetGuzergahlarAsync();
                        var guzergah = guzergahlar.FirstOrDefault(g => g.GuzergahID == guzergahId);
                        return (T?)(object?)guzergah;
                    }
                }
                
                return default(T);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetAsync<{typeof(T).Name}> Hatası: {ex.Message}");
                return default(T);
            }
        }

        public async Task<List<T>?> GetListAsync<T>(string endpoint)
        {
            try
            {
                if (typeof(T) == typeof(Models.Koltuk))
                {
                    // api/koltuklar/otobus/{otobusId} pattern'i için
                    if (endpoint.Contains("/koltuklar/otobus/"))
                    {
                        var parts = endpoint.Split('/');
                        if (parts.Length >= 4 && int.TryParse(parts[3], out int otobusId))
                        {
                            var coreKoltuklar = await CreateDbContext().Koltuklar.AsNoTracking().ToListAsync();
                            var otobusKoltuklari = coreKoltuklar.Where(k => k.OtobusID == otobusId)
                                .Select(k => new Models.Koltuk
                                {
                                    KoltukID = k.KoltukID,
                                    OtobusID = k.OtobusID,
                                    KoltukNo = k.KoltukNo
                                }).ToList();
                            return (List<T>?)(object?)otobusKoltuklari;
                        }
                    }
                }
                else if (typeof(T) == typeof(int))
                {
                    // api/koltuklar/dolu/{seferId} pattern'i için
                    if (endpoint.Contains("/koltuklar/dolu/"))
                    {
                        var parts = endpoint.Split('/');
                        if (parts.Length >= 4 && int.TryParse(parts[3], out int seferId))
                        {
                            var allBiletler = await CreateDbContext().Biletler.AsNoTracking().ToListAsync();
                            var doluKoltukIdleri = allBiletler
                                .Where(b => b.SeferID == seferId && b.BiletDurumu != "İptal")
                                .Select(b => b.KoltukID)
                                .ToList();
                            return (List<T>?)(object?)doluKoltukIdleri;
                        }
                    }
                }
                
                return new List<T>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetListAsync<{typeof(T).Name}> Hatası: {ex.Message}");
                return new List<T>();
            }
        }

        #endregion
    }
}
