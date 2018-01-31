using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PGCafe;

namespace PGLibrary.Database {

    /// <summary> DBCommand's interface </summary>
    public interface IDBCommand {

        /// <summary> Generate command to script. </summary>
        /// <param name="Provider"> Database provider to get personal setting. </param>
        /// <param name="AddCommandSpliter">Add command spliter at the end or not. </param>
        string ToScript( IDatabaseProvider Provider, bool AddCommandSpliter = false );

    } // public interface IDBCommand

} // namespace PGLibrary.Database
