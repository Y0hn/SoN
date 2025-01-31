using Unity.Netcode.Components;
/// <summary>
/// Povoluje pohyb pre vlasnika
/// </summary>
public class ClientNetwordTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
