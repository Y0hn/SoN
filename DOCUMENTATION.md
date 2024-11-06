# LORE
```
https://en.wikipedia.org/wiki/List_of_Slavic_deities
https://en.wikipedia.org/wiki/Nav_(Slavic_folklore)

```
## Inspirations
```
As an underworld
The phrase Nawia (Polish) or Nav (used across Slavic tongues) was also utilised as a name for the Slavonic underworld, ruled by the god Veles, enclosed away from the world either by a living sea or river, according to some beliefs located deep underground.[3] According to Ruthenian folklore, Veles lived on a swamp in the centre of Nav, where he sat on a golden throne at the base of the Cosmic Tree, wielding a sword.[3] Symbolically, the Nav has also been described as a huge green plain—pasture, onto which Veles guides souls.[3] The entrance to Nav was guarded by a Zmey.[3] It was believed the souls would later be reborn on earth.[7] It is highly likely that these folk beliefs were the inspiration behind the neopagan idea of Jav, Prav and Nav in the literary forgery known as the Book of Veles.

source: wiki
```
# SOURCES

## Text Grafics
Fonts
```
https://www.1001fonts.com/
```
Curveature:
```
https://github.com/TonyViT/CurvedTextMeshPro
```
## Multiplayer:
```        
Setup:
    https://youtu.be/3yuBOB3VrCk
    https://youtu.be/dUqLShvBIsM
Relay: 
    https://youtu.be/fRJlb4t_TXc
    https://stackoverflow.com/questions/51975799/how-to-get-ip-address-of-device-in-unity-2018
```
## UI:
```
https://youtu.be/tWUyEfD0kV0
https://youtu.be/RsgiYqLID-U
https://youtu.be/AyuQXfgVk3U
https://youtu.be/rAqyi85IAJ0
```
### Chat:
```
https://youtu.be/p-2QFmCMBt8
```
## Input:
```
https://youtu.be/VAM3Ve7ARwc
```
## Mechanics:
```
Attack:
        https://youtu.be/ktGJstDvEmU
```

# Problems encountred down the road

## Problems with C#
must be "(float)" couse hp & maxHp is INT so value would turn out 0
```
float value = (float)hp.Value / (float)maxHp.Value;
```
```
rows = (int)Mathf.Ceil((float)(size/cols));
// is not the same as
rows = (int)Mathf.Ceil(((float)size/(float)cols));
```
UnityEngine method to copy from variable
```
public void Copy()
{
    GUIUtility.systemCopyBuffer = connectionManager.codeText.text;
}
```

## Problems with Multiplayer
### NetworkVariables<'CustomStruck'> are pain 
They have to be value data types (cannot be null)
They have to have inplemented 'NetworkSerialize<T>(BufferSerializer<T> serializer)' serializer from 'INetworkSerializable'
U cant have Arrays or Dictionaries only as 'NetworkList<T>' and "T" must have 'Equals(T other)' from 'IEquatable<T>'
Perfect examle is struck 'Rezistance' in 'EntityStats'
### Items Cannot be send trough network - Server/Clinet RPSs
Line 446 in FastBufferReader.cs which take care of sending data between PCs (Serializer) states:
```
public void ReadNetworkSerializable<T>(out T value) where T : INetworkSerializable, new()
{
    value = new T();
    var bufferSerializer = new BufferSerializer<BufferSerializerReader>(new BufferSerializerReader(this));
    value.NetworkSerialize(bufferSerializer);
}
```
My Item Class "tree" looks like this:
```
public /*abstract*/ class Item : ScriptableObject, INetworkSerializable, IEquatable<Item>
public class Money : Item
public class Equipment : Item
public class Weapon : Equipment
public class Armor : Equipment
```
My Rpc Looks like this:
```
[Rpc(SendTo.SpecifiedInParams)] public void PickUpItemRpc(Item item, RpcParams rpcParams)
{
    Inventory.instance.AddItem(item);
}
```
so if I were to do this:
```
PickUpItemRpc((Item)weapon, RpcTarget.Single(id, RpcTargetUse.Temp)); 

// FOR EXAMPLE: PickUpItemRpc(((Item)({Scriptable object of weapon})), RpcTarget.Single(1, RpcTargetUse.Temp));
```
It would not only refuse to Create "Scriptable Object" (becouse they have to be created trough "CreateInstanve()").
It would also trow away all other atributes from inhereted classes.
And don't even talk about "abstract class Item".
What a joke :,) 

SULLUTION:
send only "string refItem"
```
pl.PickUpItemRpc(Item.GetReferency, RpcTarget.Single(id, RpcTargetUse.Temp));

// FOR EXAMPLE: PickUpItemRpc("Items/weapons/sword-1", RpcTarget.Single(1, RpcTargetUse.Temp));
```

## Problems with Animator & Prefabs
### Animator on Entity Body
When Destroyed animated Child 'Body' of gameobject 'Entity' (examp. Player) and subsequently replacing it with PreFab Animator is not registring new GameObjects as part of animation even if they have same names.

WANABEE SULLUTION:
```
private RuntimeAnimatorController RNA;

// Function called in 'EntityStats.OnNetworkSpawn().RaseSetUp()' from 'EntityController'
public Animator animate { set { animator = value; RNA = value.runtimeAnimatorController; animator.runtimeAnimatorController = null; } }

// Later in 'EntityController.FixedUpdate().AnimateMovement()'
protected virtual void AnimateMovement()
{
    if (animator != null)
        animator.runtimeAnimatorController = RNA;
    // ...
}
```

RESULT:
```
does not work for other Players trough Net
reverted back to having 'Animator' at Player (Entiry) gameobject instead of Player.Body
```
!!! IDEA SCRAPED !!!