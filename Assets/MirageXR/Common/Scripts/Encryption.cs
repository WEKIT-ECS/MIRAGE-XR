using System.Text;

public static class Encryption
{
    private const int key = 1364;

    public static string EncriptDecrypt(string txt)
    {
        StringBuilder sb = new StringBuilder(txt.Length);
        foreach (var ch in txt)
        {
            char newChar = (char) (ch ^ key);
            sb.Append(newChar);
        }

        return sb.ToString();
    }
}
