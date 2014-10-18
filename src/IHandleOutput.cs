
namespace kgrep {
    public interface IHandleOutput {
        void Write(string line);
        string Close();
    }
}
