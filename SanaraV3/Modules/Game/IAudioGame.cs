using Discord.Audio;
using System.Diagnostics;

namespace SanaraV3.Modules.Game
{
    public interface IAudioGame
    {
        public AudioOutStream GetAudioOutStream();

        public bool CanStartNewAudio(); // Is the no audio currently playing?

        public void GetNewProcess();

        public void SetCurrentProcess(Process p);
    }
}
