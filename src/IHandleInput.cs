namespace kgrep
{
    public interface IHandleInput
    {
        string ReadLine();
        void Close();
    }
}