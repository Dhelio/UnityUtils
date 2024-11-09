using System.Threading.Tasks;

namespace Castrimaris.IO.Contracts {
    public interface IViewRequest {
        public string GetCoordinates();
        public Task<string> Build(string Key);
    }
}