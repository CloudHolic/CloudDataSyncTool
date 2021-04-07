using System;
using CloudSync.Models;
using MySqlConnector;

namespace CloudSync.Utils
{
    public class LastErrorManager
    {
        private static volatile LastErrorManager _instance;
        private static readonly object Lock = new object();
        private Error _lastError;

        public static LastErrorManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Lock)
                    {
                        if (_instance == null)
                            _instance = new LastErrorManager();
                    }
                }

                return _instance;
            }
        }

        private LastErrorManager()
        {
            _lastError = new Error();
        }

        public bool CheckError()
        {
            return _lastError.ErrorOccurred;
        }

        public Error GetLastError()
        {
            return new Error(_lastError);
        }

        public void SetLastError()
        {
            _lastError = new Error();
        }

        public void SetLastError(Exception e)
        {
            _lastError = InternalSetLastError(e);
            if (_lastError == null)
                SetLastError();
        }

        private static Error InternalSetLastError(Exception e)
        {
            if (e == null)
                return null;

            return new Error
            {
                ErrorOccurred = true,
                ErrorCode = e is MySqlException exception ? exception.Number : -1,
                ErrorText = e.Message,
                InnerError = InternalSetLastError(e.InnerException)
            };
        }
    }
}
