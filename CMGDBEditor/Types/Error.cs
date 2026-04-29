using System;
using System.Collections.Generic;
using System.Text;

namespace CMGDBEditor.Types
{
    public class Error
    {
        public bool IsError { get; set; } = false;
        public string Message { get; set; } = "";
        public Error() { }
    }
}
