using System;
using Unity.Netcode;
/// <summary>
/// Urcuje aktuanu zbran a jej utok
/// </summary>
[Serializable] public struct WeaponIndex : INetworkSerializable
{
    public byte eIndex;
    public byte aIndex;

    /// <summary>
    /// Vytvara ukazovatel pouzivanej zbrane
    /// </summary>
    /// <param name="e"></param>
    /// <param name="a"></param>
    public WeaponIndex(byte e, byte a = 0)
    {
        eIndex = e;
        aIndex = a;
    }
    /// <summary>
    /// Pouzivane na prenos hodnot po sieti
    /// </summary>
    /// <param name="serializer"></param>
    /// <typeparam name="T"></typeparam>
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref eIndex);
        serializer.SerializeValue(ref aIndex);
    }
    /// <summary>
    /// Pouzivane na ziskanie kratkeho vypisu hodnot
    /// </summary>
    /// <returns>VYPIS hodnot</returns>
    public override readonly string ToString()
    {
        return $"equipIndex= {eIndex} attackIndex= {aIndex}";
    }
}
