namespace Castrimaris.Animations.Contracts {

    /// <summary>
    /// Generic interface for custom components that update <see cref="UnityEngine.Animator"/>
    /// </summary>
    public interface IAnimator {
        //TODO add other methods
        public void SetBool(string parameterName, bool value);
        public void SetFloat(string parameterName, float value);
    }
}
