using Discord;
using Discord.Audio;
using Sanara.Exception;
using Sanara.Game.Preload;
using System.Diagnostics;

namespace Sanara.Game.Impl
{
    /// <summary>
    /// Quizz game with audio files
    /// </summary>
    public class QuizzAudio : Quizz, IAudioGame
    {
        public QuizzAudio(IMessageChannel textChan, IUser user, IPreload preload, GameSettings settings) : base(textChan, user, preload, settings, StaticObjects.ModeAudio, true)
        {
            var gUser = user as IGuildUser;
            if (gUser == null)
                throw new CommandFailed("This game must be played in a server.");
            _voiceChan = gUser.VoiceChannel;
            if (_voiceChan == null)
                throw new CommandFailed("You must be in a vocal channel to play this game.");
            _voiceSession = _voiceChan.ConnectAsync().GetAwaiter().GetResult();
            _process = null;

            while (_voiceSession.ConnectionState != ConnectionState.Connected) // We wait to be connected before starting vocal stuffs
            { }
            _outStream = _voiceSession.CreatePCMStream(AudioApplication.Voice);
        }

        protected override void DisposeInternal()
        {
            if (_process != null && !_process.HasExited)
                _process.Kill();
            if (_voiceChan != null && _voiceSession.ConnectionState == ConnectionState.Connected)
            {
                _voiceChan.DisconnectAsync().GetAwaiter().GetResult();
                _voiceSession.Dispose();
            }
        }

        public IAudioClient GetVoiceSession()
            => _voiceSession;

        public AudioOutStream GetAudioOutStream()
            => _outStream;

        /// <summary>
        /// Make sure to stop the last audio before starting a new one
        /// </summary>
        public void GetNewProcess()
        {
            if (_process != null && !_process.HasExited)
                _process.Kill();
        }

        public void SetCurrentProcess(Process p)
        {
            _process = p;
        }

        public bool CanStartNewAudio()
        {
            if (_process == null) return true;
            return _process.HasExited;
        }

        private IVoiceChannel _voiceChan;
        private IAudioClient _voiceSession;
        private Process? _process;
        private AudioOutStream _outStream;
    }
}
