namespace LettersApp.Models;

public sealed class Addressee
{
    public string ContactPerson { get; set; } = "";
    public string StreetAddress { get; set; } = "";
    public string Suburb { get; set; } = "";
    public string State { get; set; } = "";
    public string PostCode { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string CardNumber { get; set; } = "";
    public string ExpireDate { get; set; } = "";
}
public class CustomerRecord
{
    public string ContactPerson { get; set; }
    public string StreetAddress { get; set; }
    public string Suburb { get; set; }
    public string State { get; set; }
    public string PostCode { get; set; }
    public string FirstName { get; set; }
    public string CardNumber { get; set; }
    public string ExpireDate { get; set; }
}
