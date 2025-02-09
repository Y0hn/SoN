public class WorldNameField : PlayerNameField
{
    protected override bool FieldCheck()
    {
        bool check =  base.FieldCheck();
        
        if (check)
            foreach (string world in FileManager.Worlds)
            {
                check &= FileManager.WorldPathToName(world) != Text;
                if (!check) 
                {
                    ErrorMessage("World already exists");
                    break;
                }
            }

        return check;
    }
}