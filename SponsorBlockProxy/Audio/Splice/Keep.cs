using System;
using System.Threading.Tasks;

namespace SponsorBlockProxy.Audio.Splice;
public class Keep
{
    public TimeSpan Start { get; set; }
    public TimeSpan Stop { get; set; }
    public Task CutTask { get; set; }
    public string File { get; set; }
}