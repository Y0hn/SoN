using Unity.Netcode.Components;
public class ClientNetwordTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
