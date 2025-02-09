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

        string player = field.text.Trim();

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
            FileManager.RegeneradeSettings();
            Menu.menu.playerName = player;
        }
        return check;
    }
}
