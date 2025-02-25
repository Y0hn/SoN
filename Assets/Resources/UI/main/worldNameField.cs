/// <summary>
/// Kontroluje nazov vytvaraneho sveta
/// </summary>
public class WorldNameField : PlayerNameField
{
    protected override bool FieldCheck()
    {
        bool check = true;

        // získa názov sveta bez okrajových medzier
        string tt = Text;

        // ak názov nebolo zadané
        if (tt == "")
        {
            ErrorMessage("Write world name");
            check = false;
        }
        // ak je názov kratšie ako 2 znaky
        else if (tt.Length < 2)
        {
            ErrorMessage("World name must be longer");
            check = false;
        }
        // ak názov obsahuje '-'
        else if (tt.Contains('-'))
        {
            ErrorMessage("World name contains bad symbols");
            check = false;
        }
        // inak kontroluje existenciu sveta
        else
            // získa vsetky názvy svetov -> 
            foreach (string world in FileManager.Worlds)
            {
                // názov každého porovná s vstupným názvom
                check &= FileManager.WorldPathToName(world) != tt;

                // ak sa zhodujú preruší a vypíše chybu
                if (!check) 
                {
                    ErrorMessage("World already exists");
                    break;
                }
            }

        // Ak chyba nenastala 
        if (check)
            ErrorMessage("");
        return check;
    }
}