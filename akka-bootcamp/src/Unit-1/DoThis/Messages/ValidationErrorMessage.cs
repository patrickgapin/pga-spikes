
namespace WinTail.Messages
{
    public class ValidationErrorMessage : InputErrorMessage
    {
        public ValidationErrorMessage(string reason) : base(reason) { }
    }
}
