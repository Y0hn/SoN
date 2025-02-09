/// <summary>
/// Wontroluje nazov vytvaraneho
/// </summary>
public class WorldNameField : PlayerNameField
{
    protected override bool FieldCheck()
    {
        bool check =  base.FieldCheck();

        string tt = Text;
        
        if (!check)
            return check;
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

        return check;
    }
}