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