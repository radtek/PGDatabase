using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PGLibrary.Database {
    /// <summary> Determin how to quote the word. </summary>
    public enum EQuoteType {
        /// <summary> Always don't quote the word. </summary>
        None,
        /// <summary> Quote only when the word is reserved word. </summary>
        Auto,
        /// <summary> Always quote whether the word is reserved word or not. </summary>
        Always,
    } // public enum EQuoteType
} // namespace PGLibrary.Database
