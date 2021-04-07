namespace CloudSync.Models
{
    public class Error
    {
        public bool ErrorOccurred { get; set; }

        public int ErrorCode { get; set; }

        public string ErrorText { get; set; }

        public Error InnerError { get; set; }

        public Error()
        {
            ErrorOccurred = false;
            ErrorCode = -1;
            ErrorText = "";
            InnerError = null;
        }

        public Error(bool occurred, int code, string text, Error innerError)
        {
            ErrorOccurred = occurred;
            ErrorCode = code;
            ErrorText = text;

            InnerError = null;
            if(InnerError != null)
                InnerError = new Error(innerError);
        }

        public Error(Error prevError)
        {
            ErrorOccurred = prevError.ErrorOccurred;
            ErrorCode = prevError.ErrorCode;
            ErrorText = prevError.ErrorText;
            
            InnerError = null;
            if(prevError.InnerError != null)
                InnerError = new Error(prevError.InnerError);
        }

        public override string ToString()
        {
            var result = string.Empty;
            for (;;)
            {
                if(ErrorOccurred)
                    result += (ErrorCode > 0 ? $"{ErrorCode} - " : string.Empty) + ErrorText + "\n";
                if (InnerError == null)
                    break;
                result += InnerError.ToString();
            }
            
            return result.TrimEnd('\n');
        }
    }
}
