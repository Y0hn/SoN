using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
public class EquipmentPanel : MonoBehaviour
{
    [SerializeField] InputActionReference input;
    [SerializeField] Animator animator;
    [SerializeField] Button button;
    [SerializeField] TMP_Text btn;
    bool equip = false;
    void Start()
    {
        button.onClick.AddListener(OC_Equipment);
        input.action.started += OC_Equipment;        
    }
    void OC_Equipment(InputAction.CallbackContext context) { OC_Equipment(); }
    void OC_Equipment()
    {
        if (!GameManager.instance.playerLives) return;
        equip = !equip;
        animator.SetBool("open", equip);
        
        if (equip) btn.text = "<";
        else btn.text = ">";
    }
}
