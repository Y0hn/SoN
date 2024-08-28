using Unity.Netcode;
using UnityEngine;

public class netReference : MonoBehaviour, INetworkSerializable
{
    [SerializeField] int id;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
    }
}
