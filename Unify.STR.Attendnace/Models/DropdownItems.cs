namespace Unify.STR.Attendnace.Models
{ 
public class DropdownItems
    {
        public string Value { get; set; }
        public string Text { get; set; }

        public DropdownItems(string Value, string Text)
        {
            this.Value = Value;
            this.Text = Text;
        }
    }
}
