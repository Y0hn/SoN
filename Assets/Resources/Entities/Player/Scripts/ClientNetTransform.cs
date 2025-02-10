using Unity.Netcode.Components;
/// <summary>
/// Povoluje pohyb pre vlasnika objektu hraca
/// </summary>
public class ClientNetwordTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
