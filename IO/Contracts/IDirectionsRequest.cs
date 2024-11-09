using System.Threading.Tasks;

namespace Castrimaris.IO.Contracts {
    public interface IDirectionsRequest {
        public Task<string> Build(string Key);
    }
}