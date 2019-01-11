using System;

namespace SanaraV2.Modules.Base
{
    public class NotAvailable :  Exception
    {
        public NotAvailable() : base("This feature isn't available")
        { }
    }
}
