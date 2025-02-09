using UnityEngine;
using TMPro;

public abstract class InputFieldCheck : MonoBehaviour
{
    [SerializeField] protected TMP_InputField field;
    [SerializeField] protected TMP_Text placeHolder;
    [SerializeField] protected TMP_Text errorMsg;

    public string Text 
    {
        get => (field.text == "") ? placeHolder.text : field.text;
        set => placeHolder.text = value;
    } 

    /// <summary>
    /// Kontrola Vstupu z vonka
    /// </summary>
    public virtual bool Check => FieldCheck();

    /// <summary>
    /// Spustane len raz
    /// </summary>
    protected virtual void Awake() => field.onSubmit.AddListener(OnSubmit);

    /// <summary>
    /// Zavolane po potvrdeni vstupu
    /// </summary>
    /// <param name="text">vstupny TEXT</param>
    protected virtual void OnSubmit(string text)
    {
        field.text = text.Trim();
        FieldCheck();
    }
    /// <summary>
    /// Overi spravnost vstupu
    /// </summary>
    /// <returns>PRAVDA ak je spravne</returns>
    protected virtual bool FieldCheck()
    {
        bool check = false;
        string player = Text.Trim();

        if (player == "")
        {
            ErrorMessage("Type your name");
        }
        else if (player.Length < 2)
        {
            ErrorMessage("Name must be longer");
        }
        else
        {
            ErrorMessage("");
            check = true;
        }

        if (check)
            FileManager.RegeneradeSettings();
        return check;
    }
    /// <summary>
    /// Vypise chybovu spravu v textovom poli
    /// </summary>
    /// <param name="message">text SPRAVY</param>
    public void ErrorMessage(string message = "")
    {
        errorMsg.text = message;
    }
}
