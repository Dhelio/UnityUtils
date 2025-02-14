#if DISSONANCE

namespace Castrimaris.IO {

    public static class  IOExtensions
    {
        public static Dissonance.CommTriggerTarget ToDissonanceCommTriggerTarget(this ChannelTypes channelType) {
            return (channelType == ChannelTypes.LOCAL) ? Dissonance.CommTriggerTarget.Self : Dissonance.CommTriggerTarget.Room;
        }
    }

}

#endif