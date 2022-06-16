namespace SponsorBlockProxy.Models;

public class SkipPair
{
    public SkipPair()
    {
        IdSeed++;
        this.Id = IdSeed.ToString();
    }

    static int IdSeed = 1;

    public string Id { get; private set; }
    public string Start_Filename { get; set; }
    public string End_Filename { get; set; }
    public int MaxTimeSeconds { get; set; } = 5 * 60;
    public int MinTimeSeconds { get; set; } = 30;
}