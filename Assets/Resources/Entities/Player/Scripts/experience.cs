using System;
using Unity.Netcode;
[Serializable] public struct Experience : INetworkSerializable
{
    public uint minValue;
    public uint value;
    public uint maxValue;
    public byte gainedLevel;

    public readonly float XP => (float)(value-minValue)/(float)(maxValue-minValue);
    public readonly bool LevelUP => 0 < gainedLevel;

    public Experience(uint min, uint v, uint max)
    {
        minValue = min;
        value = v;
        maxValue = max;
        gainedLevel = 0;
    }
    public Experience(int min, int v, int max)
    {
        minValue = (uint)min;
        value = (uint)v;
        maxValue = (uint)max;
        gainedLevel = 0;
    }
    public Experience(Experience xp, uint add)
    {
        uint tVal = xp.value + add;
        uint tMin = xp.minValue;
        uint tMax = xp.maxValue;
        byte gLevel = 0;

        for (; tMax <= tVal && gLevel < 250; gLevel++)
        {
            tMin= tMax;
            tMax += PlayerStats.NEEDED_XP_TO_NEXT_LEVEL;
        }

        value = tVal;
        minValue = tMin;
        maxValue = tMax;
        gainedLevel = gLevel;

        FileManager.Log($"Created {this}",FileLogType.RECORD);
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref minValue);
        serializer.SerializeValue(ref value);
        serializer.SerializeValue(ref maxValue);
        serializer.SerializeValue(ref gainedLevel);
    }
    public override readonly string ToString()
    {
        return $"[{minValue}<{value}>{maxValue}] {(0 < gainedLevel ? $"gained {gainedLevel} levels" : "")}";
    }

}