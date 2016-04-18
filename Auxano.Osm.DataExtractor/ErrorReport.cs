using System;

namespace Auxano.Osm.DataExtractor
{
    public class ErrorReport
    {
        public ErrorReport(Exception exception)
        {
            this.Message = exception.Message;
            if (exception.InnerException != null) this.Children = new[] { new ErrorReport(exception.InnerException) };
        }

        public ErrorReport[] Children { get; set; }
        public string Message { get; set; }
    }
}