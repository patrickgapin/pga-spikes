
namespace WinTail.Messages
{
    public class InputErrorMessage
    {
        public string Reason { get; private set; }
        public InputErrorMessage(string reason) { Reason = reason; }
    }
}
