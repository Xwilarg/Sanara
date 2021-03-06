﻿using System;

namespace SanaraV3
{
    public struct ErrorData
    {
        public ErrorData(DateTime dateTime, System.Exception exception)
        {
            DateTime = dateTime;
            Exception = exception;
        }

        public DateTime DateTime;
        public System.Exception Exception;
    }
}
