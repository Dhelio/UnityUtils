using System.Threading.Tasks;

namespace Castrimaris.ScriptableObjects {
    public interface IAddressableObject<T> {
        public void Initialize();
        public Task InitializeAsync();
        public T Get(int Index);
        public Task<T> GetAsync(int Index);
        public void Dispose();
    }
}