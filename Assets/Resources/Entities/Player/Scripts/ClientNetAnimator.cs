using Unity.Netcode.Components;
/// <summary>
/// Povoluje animovanie pre vlasnika objektu hraca
/// </summary>
public class ClientNetworkAnimator : NetworkAnimator
{
    /// <summary>
    /// Prepisuje metodu overenia
    /// </summary>
    /// <returns></returns>
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}