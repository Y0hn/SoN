using Unity.Netcode;
using UnityEngine;

public class ItemDrop : NetworkBehaviour
{
    [SerializeField] SpriteRenderer texture;
    [SerializeField] CircleCollider2D colli;
    [SerializeField] Item item;
    private const string defaultTexturePath = "Items/textures/";
    public Item Item
    {
        get { return item; }
        set 
        {  
            item = value;
            Sprite[] sprites= Resources.LoadAll<Sprite>(defaultTexturePath);
            foreach (Sprite sprite in sprites)
            {
                if (sprite.name != item.iconRef)
                {
                    texture.sprite = sprite;
                    break;
                }
            }
            // if class Sprite had implemented 'IEnumerable<Customer>'
            // sprites.Where(s => s.name == item.iconRef);
        }
    }
    void Start()
    {
        if (item == null) return;
        Item = item;
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        if (collision.transform.TryGetComponent(out PlayerStats pl))
        {
            Debug.Log($"Item {name} picked up by {pl.name}");
            pl.AddItemClientRpc(item);
            Destroy(gameObject);
        }
    }
}
