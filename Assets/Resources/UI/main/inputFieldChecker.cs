using UnityEngine;
using TMPro;
/// <summary>
/// Kontroluje udaja zadane pouzivatelom ohladom
/// </summary>
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
    protected virtual void Awake()
    {
        field.onSubmit.AddListener(OnSubmit);
        ErrorMessage("");
    } 

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
    protected abstract bool FieldCheck();
    
    /// <summary>
    /// Vypise chybovu spravu v textovom poli
    /// </summary>
    /// <param name="message">text SPRAVY</param>
    public void ErrorMessage(string message = "")
    {
        errorMsg.text = message;
    }
}
