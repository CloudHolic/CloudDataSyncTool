using System.Runtime.InteropServices;
using System.Security;

namespace CloudSync.Utils
{
    public static class SecureStringUtils
    {
        public static SecureString ConvertToSecureString(string password = "")
        {
            if (string.IsNullOrEmpty(password))
                return new SecureString();

            var secure = new SecureString();

            foreach (var c in password)
                secure.AppendChar(c);

            secure.MakeReadOnly();
            return secure;
        }

        public static string ConvertToString(SecureString password)
        {
            if (password == null)
                return "";

            var pStr = Marshal.SecureStringToGlobalAllocUnicode(password);
            var unsecuredPassword = Marshal.PtrToStringUni(pStr);
            Marshal.ZeroFreeCoTaskMemUnicode(pStr);

            return unsecuredPassword;
        }
    }
}
