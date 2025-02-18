using System.Linq;
public class IpCodeField : InputFieldCheck
{
    public override string Text 
    { 
        get => base.Text.ToUpper();
        set => base.Text = value.ToUpper(); 
    }
    protected override bool FieldCheck()
    {        
        bool check = false;

        if (field.text.Length == 6)
        {
            // vzdialene prenasane 
            check = true;
        }
        else if (field.text.Contains("."))
        {
            check = field.text.Count(c => c == '.') == 3;
        }
        else 
            ErrorMessage("Should be x.x.x.x or XXXXXX");

        if (check)
            FileManager.RegeneradeSettings();

        return check;
    }
}