using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableTopSheets.Persistence.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpellSlotRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TableKey = table.Column<string>(type: "TEXT", nullable: false),
                    CasterLevel = table.Column<byte>(type: "INTEGER", nullable: false),
                    SpellLevel = table.Column<byte>(type: "INTEGER", nullable: false),
                    Count = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpellSlotRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HitDie = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    PrimaryAbilities = table.Column<string>(type: "TEXT", nullable: false),
                    PrimaryAbilityMode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SubclassLevel = table.Column<byte>(type: "INTEGER", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DescriptionHtml = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Feats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Repeatable = table.Column<bool>(type: "INTEGER", nullable: false),
                    PrerequisiteExpr = table.Column<string>(type: "TEXT", nullable: true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DescriptionHtml = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feats_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Rare = table.Column<bool>(type: "INTEGER", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DescriptionHtml = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Languages_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Species",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatureType = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DescriptionHtml = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Species", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Species_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Spells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Level = table.Column<byte>(type: "INTEGER", nullable: false),
                    School = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CastingTime = table.Column<string>(type: "TEXT", nullable: false),
                    Ritual = table.Column<bool>(type: "INTEGER", nullable: false),
                    Range = table.Column<string>(type: "TEXT", nullable: false),
                    Components = table.Column<string>(type: "TEXT", nullable: false),
                    Material = table.Column<string>(type: "TEXT", nullable: true),
                    Concentration = table.Column<bool>(type: "INTEGER", nullable: false),
                    Duration = table.Column<string>(type: "TEXT", nullable: false),
                    Lists = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DescriptionHtml = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spells_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WeaponMasteries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DescriptionHtml = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeaponMasteries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeaponMasteries_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ScaleValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Level = table.Column<byte>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    ClassId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScaleValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScaleValues_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subclasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClassId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DescriptionHtml = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subclasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subclasses_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subclasses_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Backgrounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AbilityOptions = table.Column<string>(type: "TEXT", nullable: false),
                    OriginFeatId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DescriptionHtml = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Backgrounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Backgrounds_Feats_OriginFeatId",
                        column: x => x.OriginFeatId,
                        principalTable: "Feats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Backgrounds_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CostCp = table.Column<int>(type: "INTEGER", nullable: false),
                    Weight = table.Column<double>(type: "REAL", nullable: false),
                    Magic = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresAttunement = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArmorCategory = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    BaseAc = table.Column<byte>(type: "INTEGER", nullable: true),
                    DexCap = table.Column<byte>(type: "INTEGER", nullable: true),
                    StrengthRequirement = table.Column<byte>(type: "INTEGER", nullable: true),
                    StealthDisadvantage = table.Column<bool>(type: "INTEGER", nullable: true),
                    Group = table.Column<string>(type: "TEXT", nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Ranged = table.Column<bool>(type: "INTEGER", nullable: true),
                    Damage = table.Column<string>(type: "TEXT", nullable: true),
                    DamageType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Properties = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    VersatileDamage = table.Column<string>(type: "TEXT", nullable: true),
                    RangeNormal = table.Column<short>(type: "INTEGER", nullable: true),
                    RangeLong = table.Column<short>(type: "INTEGER", nullable: true),
                    MasteryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DescriptionHtml = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Items_WeaponMasteries_MasteryId",
                        column: x => x.MasteryId,
                        principalTable: "WeaponMasteries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    SpeciesId = table.Column<Guid>(type: "TEXT", nullable: true),
                    BackgroundId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Alignment = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Experience = table.Column<int>(type: "INTEGER", nullable: false),
                    Appearance = table.Column<string>(type: "TEXT", nullable: false),
                    Backstory = table.Column<string>(type: "TEXT", nullable: false),
                    AbilityMethod = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Strength = table.Column<byte>(type: "INTEGER", nullable: false),
                    Dexterity = table.Column<byte>(type: "INTEGER", nullable: false),
                    Constitution = table.Column<byte>(type: "INTEGER", nullable: false),
                    Intelligence = table.Column<byte>(type: "INTEGER", nullable: false),
                    Wisdom = table.Column<byte>(type: "INTEGER", nullable: false),
                    Charisma = table.Column<byte>(type: "INTEGER", nullable: false),
                    ActiveToggles = table.Column<string>(type: "TEXT", nullable: false),
                    ArmorClassChoice = table.Column<string>(type: "TEXT", nullable: true),
                    PlayState_CurrentHitPoints = table.Column<short>(type: "INTEGER", nullable: true),
                    PlayState_TemporaryHitPoints = table.Column<short>(type: "INTEGER", nullable: false),
                    PlayState_Exhaustion = table.Column<byte>(type: "INTEGER", nullable: false),
                    PlayState_HeroicInspiration = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlayState_Conditions = table.Column<string>(type: "TEXT", nullable: false),
                    PlayState_CopperPieces = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayState_SilverPieces = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayState_ElectrumPieces = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayState_GoldPieces = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayState_PlatinumPieces = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_Backgrounds_BackgroundId",
                        column: x => x.BackgroundId,
                        principalTable: "Backgrounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Characters_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DescriptionHtml = table.Column<string>(type: "TEXT", nullable: false),
                    OwnerType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Level = table.Column<byte>(type: "INTEGER", nullable: true),
                    GrantMode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: true),
                    SheetSection = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    PrerequisiteExpr = table.Column<string>(type: "TEXT", nullable: true),
                    Toggleable = table.Column<bool>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SubclassId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SpeciesId = table.Column<Guid>(type: "TEXT", nullable: true),
                    BackgroundId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FeatId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SpellId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Features_Backgrounds_BackgroundId",
                        column: x => x.BackgroundId,
                        principalTable: "Backgrounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Features_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Features_Feats_FeatId",
                        column: x => x.FeatId,
                        principalTable: "Feats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Features_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Features_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Features_Spells_SpellId",
                        column: x => x.SpellId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Features_Subclasses_SubclassId",
                        column: x => x.SubclassId,
                        principalTable: "Subclasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipSlot = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Attuned = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterItems_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CharacterLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Number = table.Column<byte>(type: "INTEGER", nullable: false),
                    ClassId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubclassId = table.Column<Guid>(type: "TEXT", nullable: true),
                    HitPointRoll = table.Column<byte>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterLevels_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterLevels_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CharacterLevels_Subclasses_SubclassId",
                        column: x => x.SubclassId,
                        principalTable: "Subclasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CharacterPreparedSpells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClassId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpellId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterPreparedSpells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterPreparedSpells_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterPreparedSpells_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CharacterPreparedSpells_Spells_SpellId",
                        column: x => x.SpellId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CharacterResourceUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Used = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterResourceUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterResourceUsages_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterSpellbookEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpellId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterSpellbookEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterSpellbookEntries_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterSpellbookEntries_Spells_SpellId",
                        column: x => x.SpellId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Choices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    FeatureId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CountExpr = table.Column<string>(type: "TEXT", nullable: false),
                    OptionSource = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: true),
                    FilterExpr = table.Column<string>(type: "TEXT", nullable: true),
                    ReplacePolicy = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    AllowDuplicates = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxPerOption = table.Column<byte>(type: "INTEGER", nullable: false),
                    Optional = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Choices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Choices_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    FeatureId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxExpr = table.Column<string>(type: "TEXT", nullable: false),
                    DieExpr = table.Column<string>(type: "TEXT", nullable: true),
                    Recharge = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpellcastingProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FeatureId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClassId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Ability = table.Column<string>(type: "TEXT", nullable: false),
                    Progression = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SlotTableKey = table.Column<string>(type: "TEXT", nullable: true),
                    SpellListKey = table.Column<string>(type: "TEXT", nullable: false),
                    PreparedCountKey = table.Column<string>(type: "TEXT", nullable: true),
                    CantripCountKey = table.Column<string>(type: "TEXT", nullable: true),
                    Pool = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    PrepareChange = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    RitualMode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Focus = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpellcastingProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpellcastingProfiles_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SpellcastingProfiles_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterChoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChoiceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OptionKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    LevelId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SupersededByLevelId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterChoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterChoices_CharacterChoices_ParentId",
                        column: x => x.ParentId,
                        principalTable: "CharacterChoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterChoices_CharacterLevels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "CharacterLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterChoices_CharacterLevels_SupersededByLevelId",
                        column: x => x.SupersededByLevelId,
                        principalTable: "CharacterLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CharacterChoices_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterChoices_Choices_ChoiceId",
                        column: x => x.ChoiceId,
                        principalTable: "Choices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChoiceOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChoiceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    GrantsFeatureId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PrerequisiteExpr = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChoiceOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChoiceOptions_Choices_ChoiceId",
                        column: x => x.ChoiceId,
                        principalTable: "Choices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChoiceOptions_Features_GrantsFeatureId",
                        column: x => x.GrantsFeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Effects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FeatureId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ChoiceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Target = table.Column<string>(type: "TEXT", nullable: false),
                    Operation = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ValueExpr = table.Column<string>(type: "TEXT", nullable: false),
                    ConditionExpr = table.Column<string>(type: "TEXT", nullable: true),
                    StackGroup = table.Column<string>(type: "TEXT", nullable: true),
                    Label = table.Column<string>(type: "TEXT", nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Effects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Effects_Choices_ChoiceId",
                        column: x => x.ChoiceId,
                        principalTable: "Choices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Effects_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Backgrounds_Key",
                table: "Backgrounds",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Backgrounds_OriginFeatId",
                table: "Backgrounds",
                column: "OriginFeatId");

            migrationBuilder.CreateIndex(
                name: "IX_Backgrounds_SourceId",
                table: "Backgrounds",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterChoices_CharacterId_ChoiceId",
                table: "CharacterChoices",
                columns: new[] { "CharacterId", "ChoiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterChoices_ChoiceId",
                table: "CharacterChoices",
                column: "ChoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterChoices_LevelId",
                table: "CharacterChoices",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterChoices_ParentId",
                table: "CharacterChoices",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterChoices_SupersededByLevelId",
                table: "CharacterChoices",
                column: "SupersededByLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterItems_CharacterId",
                table: "CharacterItems",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterItems_ItemId",
                table: "CharacterItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterLevels_CharacterId_Number",
                table: "CharacterLevels",
                columns: new[] { "CharacterId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterLevels_ClassId",
                table: "CharacterLevels",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterLevels_SubclassId",
                table: "CharacterLevels",
                column: "SubclassId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterPreparedSpells_CharacterId_ClassId_SpellId",
                table: "CharacterPreparedSpells",
                columns: new[] { "CharacterId", "ClassId", "SpellId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterPreparedSpells_ClassId",
                table: "CharacterPreparedSpells",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterPreparedSpells_SpellId",
                table: "CharacterPreparedSpells",
                column: "SpellId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterResourceUsages_CharacterId_Key",
                table: "CharacterResourceUsages",
                columns: new[] { "CharacterId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Characters_BackgroundId",
                table: "Characters",
                column: "BackgroundId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_SpeciesId",
                table: "Characters",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSpellbookEntries_CharacterId",
                table: "CharacterSpellbookEntries",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSpellbookEntries_SpellId",
                table: "CharacterSpellbookEntries",
                column: "SpellId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoiceOptions_ChoiceId_Key",
                table: "ChoiceOptions",
                columns: new[] { "ChoiceId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChoiceOptions_GrantsFeatureId",
                table: "ChoiceOptions",
                column: "GrantsFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_Choices_FeatureId",
                table: "Choices",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_Choices_Key",
                table: "Choices",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_Key",
                table: "Classes",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_SourceId",
                table: "Classes",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Effects_ChoiceId",
                table: "Effects",
                column: "ChoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Effects_FeatureId",
                table: "Effects",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_Feats_Key",
                table: "Feats",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feats_SourceId",
                table: "Feats",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Features_BackgroundId",
                table: "Features",
                column: "BackgroundId");

            migrationBuilder.CreateIndex(
                name: "IX_Features_Category",
                table: "Features",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Features_ClassId",
                table: "Features",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Features_FeatId",
                table: "Features",
                column: "FeatId");

            migrationBuilder.CreateIndex(
                name: "IX_Features_ItemId",
                table: "Features",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Features_Key",
                table: "Features",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Features_SpeciesId",
                table: "Features",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_Features_SpellId",
                table: "Features",
                column: "SpellId");

            migrationBuilder.CreateIndex(
                name: "IX_Features_SubclassId",
                table: "Features",
                column: "SubclassId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Key",
                table: "Items",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_MasteryId",
                table: "Items",
                column: "MasteryId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_SourceId",
                table: "Items",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Languages_Key",
                table: "Languages",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Languages_SourceId",
                table: "Languages",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_FeatureId",
                table: "Resources",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Key",
                table: "Resources",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScaleValues_ClassId",
                table: "ScaleValues",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ScaleValues_Key_Level",
                table: "ScaleValues",
                columns: new[] { "Key", "Level" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sources_Key",
                table: "Sources",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Species_Key",
                table: "Species",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Species_SourceId",
                table: "Species",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SpellcastingProfiles_ClassId",
                table: "SpellcastingProfiles",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_SpellcastingProfiles_FeatureId",
                table: "SpellcastingProfiles",
                column: "FeatureId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spells_Key",
                table: "Spells",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spells_SourceId",
                table: "Spells",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SpellSlotRows_TableKey_CasterLevel_SpellLevel",
                table: "SpellSlotRows",
                columns: new[] { "TableKey", "CasterLevel", "SpellLevel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subclasses_ClassId",
                table: "Subclasses",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Subclasses_Key",
                table: "Subclasses",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subclasses_SourceId",
                table: "Subclasses",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_WeaponMasteries_Key",
                table: "WeaponMasteries",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeaponMasteries_SourceId",
                table: "WeaponMasteries",
                column: "SourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterChoices");

            migrationBuilder.DropTable(
                name: "CharacterItems");

            migrationBuilder.DropTable(
                name: "CharacterPreparedSpells");

            migrationBuilder.DropTable(
                name: "CharacterResourceUsages");

            migrationBuilder.DropTable(
                name: "CharacterSpellbookEntries");

            migrationBuilder.DropTable(
                name: "ChoiceOptions");

            migrationBuilder.DropTable(
                name: "Effects");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "ScaleValues");

            migrationBuilder.DropTable(
                name: "SpellcastingProfiles");

            migrationBuilder.DropTable(
                name: "SpellSlotRows");

            migrationBuilder.DropTable(
                name: "CharacterLevels");

            migrationBuilder.DropTable(
                name: "Choices");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "Backgrounds");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Species");

            migrationBuilder.DropTable(
                name: "Spells");

            migrationBuilder.DropTable(
                name: "Subclasses");

            migrationBuilder.DropTable(
                name: "Feats");

            migrationBuilder.DropTable(
                name: "WeaponMasteries");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Sources");
        }
    }
}
