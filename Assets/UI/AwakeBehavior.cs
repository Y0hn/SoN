using UnityEngine;
/// <summary>
/// Niekedy sa stane, ze sa behavior ktory nieje povoleny ("enable"-nuty) ne"Awakene". <br />
/// Tato trieda je vytvorena presne aby sa takymto pripadom zabranilo. <br />
/// </summary>
public abstract class AwakeBehavior : MonoBehaviour
{
    /// <summary>
    /// Spusti sa pred prvim snimkom obrazovky
    /// </summary>
    public abstract void Awake();
}