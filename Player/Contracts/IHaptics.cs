namespace Castrimaris.Player {


    public interface IHaptics {
        void Vibrate(float Amplitude, float Duration);
        //TODO
        //void Vibrate(HapticsTargets Target, float Amplitude, float Duration);
        //void ShortSoftVibrate(HapticsTargets Target);
        //void ShortHardVibrate(HapticsTargets Target);
        //void LongSoftVibrate(HapticsTargets Target);
        //void LongHardVibrate(HapticsTargets Target);
    }

}