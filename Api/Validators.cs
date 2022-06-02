using System.Text.RegularExpressions;

namespace Api;

public class Validators
{
    public static bool IsValidSlug(string str)
    {
        return Regex.IsMatch(str, "^[a-z0-9-]+$");
    }
}
