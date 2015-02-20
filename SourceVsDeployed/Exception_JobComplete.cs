using System;

namespace SourceVsDeployed
{
    /// <summary>
    /// Honestly, this is a a hack.
    /// I created this custom exception because I couldn't seem to get JobWasExecuted
    /// to fire unless I did this. 
    /// EventArgs_JobListener is quite similar...because I'm essentially using this 
    /// exception as an event.   
    /// </summary>
// ReSharper disable once InconsistentNaming
    internal class Exception_JobComplete : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code">optional code (often an integer)</param>
        /// <param name="message">optional message</param>
        public Exception_JobComplete(string code, string message)
        {
            _code = code;
            _message = message;
        }

        private readonly string _code;
        private readonly string _message;

        public string Code { get { return _code; } }
        public string CustomMessage { get { return _message; } }

        public override string ToString()
        {
            return "EventArgs_JobListener\n" +
                   "Code:" + Code + "\n" +
                   "CustomMessage:" + CustomMessage + "\n";
        }

    }

}
