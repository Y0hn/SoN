using Unity.Netcode.Components;
public class ClientNetwordAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}