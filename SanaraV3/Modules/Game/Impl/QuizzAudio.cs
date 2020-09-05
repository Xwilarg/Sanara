using Discord;
using Discord.Audio;
using DiscordUtils;
using SanaraV3.Exceptions;
using SanaraV3.Modules.Game.Preload;
using System;
using System.Diagnostics;

namespace SanaraV3.Modules.Game.Impl
{
    public sealed class QuizzAudio : Quizz
    {
        public QuizzAudio(IMessageChannel textChan, IUser user, IPreload preload, GameSettings settings) : base(textChan, user, preload, settings, StaticObjects.ModeAudio)
        {
            var gUser = user as IGuildUser;
            if (gUser == null)
                throw new CommandFailed("This game must be played in a server.");
            _voiceChan = gUser.VoiceChannel;
            if (_voiceChan == null)
                throw new CommandFailed("You must be in a vocal channel to play this game.");
            _voiceSession = _voiceChan.ConnectAsync().GetAwaiter().GetResult();
            _process = null;
        }

        ~QuizzAudio()
        {
            try
            {
                _voiceChan.DisconnectAsync().GetAwaiter().GetResult();
            }
            catch (Exception e) // We makes sure that no exception is thrown in the dtor
            {
                Utils.LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
            }
        }

        protected override string GetRules()
            => "I'll play a vocal line of a character, you'll have to give his name.";

        public IAudioClient GetVoiceSession()
            => _voiceSession;

        /// <summary>
        /// Make sure to stop the last audio before starting a new one
        /// </summary>
        public Process GetNewProcess()
        {
            if (_process != null && !_process.HasExited)
                _process.Kill();
            return _process;
        }

        private IVoiceChannel _voiceChan;
        private IAudioClient _voiceSession;
        private Process _process;
    }
}
