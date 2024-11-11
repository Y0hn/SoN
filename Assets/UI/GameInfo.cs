using TMPro;
using UnityEngine;

public class GameInfo : MonoBehaviour
{
    [SerializeField] new TMP_Text name;
    [SerializeField] TMP_Text version;
    [SerializeField] TMP_Text company;
    void Awake()
    {
        if (name != null)
            name.text = Application.productName;
        if (version != null)
            version.text = "Version: " + Application.version;
        if (company != null)
            company.text = Application.companyName;
    }
}
