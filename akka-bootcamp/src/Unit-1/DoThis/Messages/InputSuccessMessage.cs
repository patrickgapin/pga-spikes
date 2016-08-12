
namespace WinTail.Messages
{
    public class InputSuccessMessage
    {
        public string Reason { get; private set; }
        public InputSuccessMessage(string reason) { Reason = reason; }
    }
}
