using Unity.VisualScripting;
using UnityEngine;

public class ColorChain : MonoBehaviour
{
    [SerializeField] ColorChainReference reference;
    void Awake()
    {
        if (reference != null)
            GetComponent<SpriteRenderer>().color = reference.Color;
    }
    void OnDrawGizmos()
    {
        if (reference != null)
            GetComponent<SpriteRenderer>().color = reference.Color;
    }
}
