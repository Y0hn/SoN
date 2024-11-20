using System.Collections.Generic;
using System.Numerics;

public static class FileManager
{
    public const string TEXTURE_DEFAULT_PATH = "Items/textures";
    public const string ITEM_DEFAULT_PATH = "Items";
    public const string WEAPONS_DEFAULT_PATH = "Items/weapons";
    public const string ARMORS_DEFAULT_PATH = "Items/armors";


    // Ulozene iba na servery
    public static World world;

    public static World LoadWorldData(string path)
    {
        return new();
    }
}
public class World
{
    List<ItemDrop> items;
    List<EntitySave> entities;
    Dictionary<string, InventorySave> inventories;

    public World()
    {
        items = new List<ItemDrop>();
    }
    public class EntitySave
    {
        Defence defence;
        Vector2 position;
        string etName;
        bool isPlayer;
        float hp;

        public EntitySave(EntityStats entity)
        {
            position = new(entity.transform.position.x,entity.transform.position.y);
            isPlayer = entity is PlayerStats;
            etName = entity.transform.name;
            hp = entity.HP;
        }
    }
    public class InventorySave
    {

    }
}