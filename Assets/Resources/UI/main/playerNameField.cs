/// <summary>
/// <inheritdoc/> mena hraca
/// </summary>
public class PlayerNameField : InputFieldCheck
{
    public override bool Check => base.Check || !gameObject.activeInHierarchy;
    /// <summary>
    /// <inheritdoc/> mena hraca
    /// </summary>
    /// <returns><inheritdoc/></returns>
    protected override bool FieldCheck()
    {
        bool check = false;

        string player = Text;

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
        {
            Menu.menu.playerName = player;
            FileManager.RegeneradeSettings();
        }
        return check;
    }
}
