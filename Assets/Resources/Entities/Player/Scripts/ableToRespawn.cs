using UnityEngine;
/// <summary>
/// Set if player is able to be respawned
/// </summary>
public class AbleToRespawn : MonoBehaviour
{
    public bool Respawnable
    {
        get         
        {
            if (ableToBe)
            {
                ableToBe = false;
                return true;
            }
            return false;
        }
        private set { ableToBe = value; }
    }
    private bool ableToBe;
    public void SetAble()
    {
        Respawnable = true;
    }
}
