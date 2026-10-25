using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TableTopSheets.Persistence.Entity;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence;

public class TableTopSheetsContext: DbContext
{
  #region Constructor

  public TableTopSheetsContext(DbContextOptions options)
    : base(options)
  {
  }

  #endregion

  #region Rules

  public DbSet<Source> Sources => this.Set<Source>();

  public DbSet<CharacterClass> Classes => this.Set<CharacterClass>();

  public DbSet<Subclass> Subclasses => this.Set<Subclass>();

  public DbSet<Species> Species => this.Set<Species>();

  public DbSet<Background> Backgrounds => this.Set<Background>();

  public DbSet<Feat> Feats => this.Set<Feat>();

  public DbSet<Feature> Features => this.Set<Feature>();

  public DbSet<Effect> Effects => this.Set<Effect>();

  public DbSet<Choice> Choices => this.Set<Choice>();

  public DbSet<ChoiceOption> ChoiceOptions => this.Set<ChoiceOption>();

  public DbSet<ScaleValue> ScaleValues => this.Set<ScaleValue>();

  public DbSet<ResourceDefinition> Resources => this.Set<ResourceDefinition>();

  public DbSet<SpellcastingProfile> SpellcastingProfiles => this.Set<SpellcastingProfile>();

  public DbSet<SpellSlotRow> SpellSlotRows => this.Set<SpellSlotRow>();

  public DbSet<Spell> Spells => this.Set<Spell>();

  public DbSet<Language> Languages => this.Set<Language>();

  public DbSet<WeaponMastery> WeaponMasteries => this.Set<WeaponMastery>();

  public DbSet<Item> Items => this.Set<Item>();

  #endregion

  #region Characters

  public DbSet<Character> Characters => this.Set<Character>();

  public DbSet<CharacterLevel> CharacterLevels => this.Set<CharacterLevel>();

  public DbSet<CharacterChoice> CharacterChoices => this.Set<CharacterChoice>();

  public DbSet<CharacterItem> CharacterItems => this.Set<CharacterItem>();

  public DbSet<CharacterPreparedSpell> CharacterPreparedSpells => this.Set<CharacterPreparedSpell>();

  public DbSet<CharacterSpellbookEntry> CharacterSpellbookEntries => this.Set<CharacterSpellbookEntry>();

  public DbSet<CharacterResourceUsage> CharacterResourceUsages => this.Set<CharacterResourceUsage>();

  #endregion

  #region Override

  protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
  {
    base.ConfigureConventions(configurationBuilder);

    // enums are stored as text so the database stays readable and seed data survives enum reordering
    configurationBuilder.Properties<System.Enum>().HaveConversion<string>().HaveMaxLength(64);
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(typeof(TableTopSheetsContext).Assembly);

    // ids are always assigned in code (deterministic for seed data), so entities added through a navigation are inserted
    foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes().Where(entry => typeof(BaseEntity).IsAssignableFrom(entry.ClrType) && entry.BaseType == null))
    {
      entityType.FindProperty(nameof(BaseEntity.Id))!.ValueGenerated = ValueGenerated.Never;
    }
  }

  #endregion
}
