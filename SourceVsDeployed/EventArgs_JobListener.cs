using System;

namespace SourceVsDeployed
{
    /// <summary>
    /// Allows us to pass parameters back from job listeners
    /// </summary>
// ReSharper disable once InconsistentNaming
    internal class EventArgs_JobListener : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code">Code (often an integer) to be passed</param>
        /// <param name="message">Message</param>
        public EventArgs_JobListener(string code, string message)
        {
            _code = code;
            _message = message;
        }

        private readonly string _code;
        private readonly string _message;

        public string Code { get { return _code; } }
        public string Message { get { return _message; } }

        public override string ToString()
        {
            return "EventArgs_JobListener\n" +
                   "Code:" + Code + "\n" +
                   "Message:" + Message + "\n";
        }

    }

}
