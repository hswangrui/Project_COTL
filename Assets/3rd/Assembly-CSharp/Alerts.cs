using System;
using src.Alerts;

[Serializable]
public class Alerts
{
	public DoctrineAlerts Doctrine = new DoctrineAlerts();

	public FollowerInteractionAlerts FollowerInteractions = new FollowerInteractionAlerts();

	public RitualAlerts Rituals = new RitualAlerts();

	public StructureAlerts Structures = new StructureAlerts();

	public CharacterSkinAlerts CharacterSkinAlerts = new CharacterSkinAlerts();

	public InventoryAlerts Inventory = new InventoryAlerts();

	public WeaponAlerts Weapons = new WeaponAlerts();

	public CurseAlerts Curses = new CurseAlerts();

	public TarotCardAlerts TarotCardAlerts = new TarotCardAlerts();

	public UpgradeAlerts Upgrades = new UpgradeAlerts();

	public LocationAlerts Locations = new LocationAlerts();

	public TutorialAlerts Tutorial = new TutorialAlerts();

	public RecipeAlerts Recipes = new RecipeAlerts();

	public RelicAlerts RelicAlerts = new RelicAlerts();

	public PhotoGalleryAlerts GalleryAlerts = new PhotoGalleryAlerts();

	public RunTarotCardAlerts RunTarotCardAlerts = new RunTarotCardAlerts();
}
