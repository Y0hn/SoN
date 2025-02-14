/// <summary>
/// Wontroluje nazov vytvaraneho
/// </summary>
public class WorldNameField : PlayerNameField
{
    protected override bool FieldCheck()
    {
        bool check = true;

        string tt = Text;

        if (tt == "")
        {
            ErrorMessage("Type world name");
            check = false;
        }
        else if (tt.Length < 2)
        {
            ErrorMessage("World name must be longer");
            check = false;
        }
        else if (tt.Contains('-'))
        {
            ErrorMessage("World name contains bad symbols");
            check = false;
        }
        else
            foreach (string world in FileManager.Worlds)
            {
                check &= FileManager.WorldPathToName(world) != tt;
                if (!check) 
                {
                    ErrorMessage("World already exists");
                    break;
                }
            }

        if (check)
            ErrorMessage("");
        return check;
    }
}