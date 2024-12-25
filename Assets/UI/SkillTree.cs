using UnityEngine;
using TMPro;
public class SkillTree : MonoBehaviour
{
    [SerializeField] TMP_Text skillCounterText;
    public void LevelUP (byte skillCounter)
    {
        skillCounterText.text = skillCounter.ToString();
    }
}
