using System.Threading.Tasks;
using UnityEngine.Events;

namespace Castrimaris.IO.Contracts {

    /// <summary>
    /// Contract for any component that implements Voice Chat functionality.
    /// </summary>
    public interface IVoiceComms {

        public bool IsInitialized { get; }

        /// <summary>
        /// Player id
        /// </summary>
        public string PlayerId { get; }

        /// <summary>
        /// Event called when a character is speaking.
        /// </summary>
        public UnityEvent OnSpeakingEvent { get; }

        /// <summary>
        /// Initializes the Voice Chat component. Should be async.
        /// </summary>
        public Task Initialize();

        /// <summary>
        /// Self destruct
        /// </summary>
        public void Destroy();

        /// <summary>
        /// Logs in the Voice Chat as an Anonymous User. Should be async.
        /// </summary>
        public Task Login();

        /// <summary>
        /// Logs out the Voice Chat. Should be async.
        /// </summary>
        public Task Logout();

        /// <summary>
        /// Joins a specific channel. Should be async.
        /// </summary>
        /// <param name="channelName">Name of the channel to join</param>
        /// <param name="channelType">Either a Global (non-positional) chat or a local (positional) chat</param>
        /// <returns></returns>
        public Task Join(string channelName, ChannelTypes channelType);

        /// <summary>
        /// Leaves a previously joined channel. Should be async.
        /// </summary>
        /// <param name="ChannelName">The name of the joined channel</param>
        /// <param name="ChannelType">The type of the joined channel</param>
        public Task Leave(string ChannelName, ChannelTypes ChannelType);

        /// <summary>
        /// Restarts Service
        /// </summary>
        public Task Restart();
    }
}
