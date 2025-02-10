using Unity.Netcode.Components;
/// <summary>
/// Povoluje animovanie pre vlasnika objektu hraca
/// </summary>
public class ClientNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}