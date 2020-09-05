using Discord.Audio;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Game
{
    public interface IAudioGame
    {
        public AudioOutStream GetAudioOutStream();

        public void SetStreamTask(Task task);

        public bool CanStartNewAudio(); // Is the no audio currently playing?

        public Process GetNewProcess();
    }
}
