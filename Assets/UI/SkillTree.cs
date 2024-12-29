using UnityEngine;
using TMPro;
public class SkillTree : MonoBehaviour
{
    [SerializeField] TMP_Text skillCounterText;
    [SerializeField] Transform skills;
    [SerializeField] HoldButton button;
    [SerializeField] GameManager game;
    [SerializeField] Vector2 limitPosition;
    [SerializeField] Vector2 limitOffset;
    [SerializeField] bool ReloadLimimts = false;

    Vector2[] limits = new Vector2[2];
    Vector2 startMouse;
    void Awake()
    {
        button.onEnterHold += delegate
        {
            startMouse = game.MousePos;
        };
        CalculateLimits();
    }
    void FixedUpdate()
    {
        if (button.isHolding)
        {
            Vector2 v = game.MousePos - startMouse;
            startMouse = game.MousePos;
            MoveSkills(v);
        }
        if (ReloadLimimts)
            CalculateLimits();
    }
    private void CalculateLimits()
    {
        limits[0] = new (
            Mathf.Round(transform.position.x-limitPosition.x-limitOffset.x), 
            Mathf.Round(transform.position.y-limitPosition.y-limitOffset.y));
        limits[1] = new (
            Mathf.Round(transform.position.y+limitPosition.x-limitOffset.x), 
            Mathf.Round(transform.position.y+limitPosition.y-limitOffset.y));
    }
    public void LevelUP (byte skillCounter)
    {
        skillCounterText.text = skillCounter.ToString();
    }
    public void MoveSkills (Vector2 moveBy)
    {
        Vector2 newPosition = new (skills.position.x + moveBy.x, skills.position.y + moveBy.y);

        if  (limits[0].x < newPosition.x && newPosition.x < limits[1].x
                &&
             limits[0].y < newPosition.y && newPosition.y < limits[1].y)
        {
            skills.position = newPosition;
            /*
            Debug.Log(  $"Moving skillTree to ({newPosition.x},{newPosition.y})\n" + 
                        $"Base ({transform.position.x},{transform.position.y})\n" +
                        $"Limits: \n0 => ({limits[0].x},{limits[0].y}) \n1 => ({limits[1].x},{limits[1].y})");*/
        }
    }
}
