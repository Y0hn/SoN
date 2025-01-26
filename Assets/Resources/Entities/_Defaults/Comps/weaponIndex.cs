using System;
using Unity.Netcode;
[Serializable] public struct WeaponIndex : INetworkSerializable
{
    public sbyte eIndex;
    public sbyte aIndex;
    public bool Holding { get => eIndex >= 0 && 0 <= aIndex;}
    public WeaponIndex(sbyte e, sbyte a = 0)
    {
        eIndex = e;
        aIndex = a;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref eIndex);
        serializer.SerializeValue(ref aIndex);
    }
    public override string ToString()
    {
        return $"equipIndex= {eIndex} attackIndex= {aIndex}";
    }
}
