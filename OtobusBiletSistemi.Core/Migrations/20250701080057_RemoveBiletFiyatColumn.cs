using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OtobusBiletSistemi.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBiletFiyatColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ASPNET_ROLES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NAME = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    NORMALIZEDNAME = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    CONCURRENCYSTAMP = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASPNET_ROLES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GUZERGAH",
                columns: table => new
                {
                    GUZERGAHID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NEREDEN = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    NEREYE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MESAFE = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GUZERGAH", x => x.GUZERGAHID);
                });

            migrationBuilder.CreateTable(
                name: "KOLTUK",
                columns: table => new
                {
                    KOLTUKID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    OTOBUSID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    KOLTUKNO = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KOLTUK", x => x.KOLTUKID);
                });

            migrationBuilder.CreateTable(
                name: "ODEME",
                columns: table => new
                {
                    ODEMEID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    YOLCUID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ODEMETARIHI = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    TOPLAMTUTAR = table.Column<decimal>(type: "NUMBER(10,2)", nullable: false),
                    ODEMETIPI = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ONAYDURUMU = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    KARTSAHIBIADI = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    KARTNUMARASI = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    BILETSAYISI = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ODEME", x => x.ODEMEID);
                });

            migrationBuilder.CreateTable(
                name: "OTOBUS",
                columns: table => new
                {
                    OTOBUSID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PLAKA = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    OTOBUSTIPI = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    KOLTUKSAYISI = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTOBUS", x => x.OTOBUSID);
                });

            migrationBuilder.CreateTable(
                name: "Yolcu",
                columns: table => new
                {
                    YolcuID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TCNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Ad = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Soyad = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TelNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Yolcu", x => x.YolcuID);
                });

            migrationBuilder.CreateTable(
                name: "YOLCU",
                columns: table => new
                {
                    YOLCUID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TCNO = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    AD = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    SOYAD = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TELNO = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false),
                    USERNAME = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    NORMALIZEDUSERNAME = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    NORMALIZEDEMAIL = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    EMAILCONFIRMED = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PASSWORDHASH = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SECURITYSTAMP = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CONCURRENCYSTAMP = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TWOFACTORENABLED = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LOCKOUTEND = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    LOCKOUTENABLED = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ACCESSFAILEDCOUNT = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YOLCU", x => x.YOLCUID);
                });

            migrationBuilder.CreateTable(
                name: "ASPNET_ROLE_CLAIMS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ROLEID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CLAIMTYPE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CLAIMVALUE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASPNET_ROLE_CLAIMS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ASPNET_ROLE_CLAIMS_ASPNET_ROLES_ROLEID",
                        column: x => x.ROLEID,
                        principalTable: "ASPNET_ROLES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SEFER",
                columns: table => new
                {
                    SEFERID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    OTOBUSID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GUZERGAHID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TARIH = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    SAAT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    KALKMATERMINALI = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    VARISTERMINALI = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    FIYAT = table.Column<decimal>(type: "NUMBER(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SEFER", x => x.SEFERID);
                    table.ForeignKey(
                        name: "FK_SEFER_GUZERGAH_GUZERGAHID",
                        column: x => x.GUZERGAHID,
                        principalTable: "GUZERGAH",
                        principalColumn: "GUZERGAHID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SEFER_OTOBUS_OTOBUSID",
                        column: x => x.OTOBUSID,
                        principalTable: "OTOBUS",
                        principalColumn: "OTOBUSID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASPNET_USER_CLAIMS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    USERID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CLAIMTYPE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CLAIMVALUE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASPNET_USER_CLAIMS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ASPNET_USER_CLAIMS_YOLCU_USERID",
                        column: x => x.USERID,
                        principalTable: "YOLCU",
                        principalColumn: "YOLCUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASPNET_USER_LOGINS",
                columns: table => new
                {
                    LOGINPROVIDER = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    PROVIDERKEY = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    PROVIDERDISPLAYNAME = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    USERID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASPNET_USER_LOGINS", x => new { x.LOGINPROVIDER, x.PROVIDERKEY });
                    table.ForeignKey(
                        name: "FK_ASPNET_USER_LOGINS_YOLCU_USERID",
                        column: x => x.USERID,
                        principalTable: "YOLCU",
                        principalColumn: "YOLCUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASPNET_USER_ROLES",
                columns: table => new
                {
                    USERID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ROLEID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASPNET_USER_ROLES", x => new { x.USERID, x.ROLEID });
                    table.ForeignKey(
                        name: "FK_ASPNET_USER_ROLES_ASPNET_ROLES_ROLEID",
                        column: x => x.ROLEID,
                        principalTable: "ASPNET_ROLES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ASPNET_USER_ROLES_YOLCU_USERID",
                        column: x => x.USERID,
                        principalTable: "YOLCU",
                        principalColumn: "YOLCUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASPNET_USER_TOKENS",
                columns: table => new
                {
                    USERID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LOGINPROVIDER = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    VALUE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASPNET_USER_TOKENS", x => new { x.USERID, x.LOGINPROVIDER, x.NAME });
                    table.ForeignKey(
                        name: "FK_ASPNET_USER_TOKENS_YOLCU_USERID",
                        column: x => x.USERID,
                        principalTable: "YOLCU",
                        principalColumn: "YOLCUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BILET",
                columns: table => new
                {
                    BILETID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    SEFERID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    KOLTUKID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    YOLCUID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ODEMEID = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    BILETTARIHI = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    BILETDURUMU = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BILET", x => x.BILETID);
                    table.ForeignKey(
                        name: "FK_BILET_KOLTUK_KOLTUKID",
                        column: x => x.KOLTUKID,
                        principalTable: "KOLTUK",
                        principalColumn: "KOLTUKID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BILET_ODEME_ODEMEID",
                        column: x => x.ODEMEID,
                        principalTable: "ODEME",
                        principalColumn: "ODEMEID");
                    table.ForeignKey(
                        name: "FK_BILET_SEFER_SEFERID",
                        column: x => x.SEFERID,
                        principalTable: "SEFER",
                        principalColumn: "SEFERID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BILET_Yolcu_YOLCUID",
                        column: x => x.YOLCUID,
                        principalTable: "Yolcu",
                        principalColumn: "YolcuID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ASPNET_ROLE_CLAIMS_ROLEID",
                table: "ASPNET_ROLE_CLAIMS",
                column: "ROLEID");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "ASPNET_ROLES",
                column: "NORMALIZEDNAME",
                unique: true,
                filter: "\"NORMALIZEDNAME\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ASPNET_USER_CLAIMS_USERID",
                table: "ASPNET_USER_CLAIMS",
                column: "USERID");

            migrationBuilder.CreateIndex(
                name: "IX_ASPNET_USER_LOGINS_USERID",
                table: "ASPNET_USER_LOGINS",
                column: "USERID");

            migrationBuilder.CreateIndex(
                name: "IX_ASPNET_USER_ROLES_ROLEID",
                table: "ASPNET_USER_ROLES",
                column: "ROLEID");

            migrationBuilder.CreateIndex(
                name: "IX_BILET_KOLTUKID",
                table: "BILET",
                column: "KOLTUKID");

            migrationBuilder.CreateIndex(
                name: "IX_BILET_ODEMEID",
                table: "BILET",
                column: "ODEMEID");

            migrationBuilder.CreateIndex(
                name: "IX_BILET_SEFERID",
                table: "BILET",
                column: "SEFERID");

            migrationBuilder.CreateIndex(
                name: "IX_BILET_YOLCUID",
                table: "BILET",
                column: "YOLCUID");

            migrationBuilder.CreateIndex(
                name: "IX_SEFER_GUZERGAHID",
                table: "SEFER",
                column: "GUZERGAHID");

            migrationBuilder.CreateIndex(
                name: "IX_SEFER_OTOBUSID",
                table: "SEFER",
                column: "OTOBUSID");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "YOLCU",
                column: "NORMALIZEDEMAIL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "YOLCU",
                column: "NORMALIZEDUSERNAME",
                unique: true,
                filter: "\"NORMALIZEDUSERNAME\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ASPNET_ROLE_CLAIMS");

            migrationBuilder.DropTable(
                name: "ASPNET_USER_CLAIMS");

            migrationBuilder.DropTable(
                name: "ASPNET_USER_LOGINS");

            migrationBuilder.DropTable(
                name: "ASPNET_USER_ROLES");

            migrationBuilder.DropTable(
                name: "ASPNET_USER_TOKENS");

            migrationBuilder.DropTable(
                name: "BILET");

            migrationBuilder.DropTable(
                name: "ASPNET_ROLES");

            migrationBuilder.DropTable(
                name: "YOLCU");

            migrationBuilder.DropTable(
                name: "KOLTUK");

            migrationBuilder.DropTable(
                name: "ODEME");

            migrationBuilder.DropTable(
                name: "SEFER");

            migrationBuilder.DropTable(
                name: "Yolcu");

            migrationBuilder.DropTable(
                name: "GUZERGAH");

            migrationBuilder.DropTable(
                name: "OTOBUS");
        }
    }
}
