using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PGCafe;
using PGCafe.Object;

namespace PGLibrary.Database {

    /// <summary> Sub clause in SQL Command </summary>
    internal interface IClause {

        /// <summary> Has any clause in this instance. </summary>
        /// <returns> true if any clause in this instance, otherwise false. </returns>
        bool AnyClause();

        /// <summary> Generate instance to script </summary>
        /// <param name="Provider">The database provider.</param>
        /// <param name="QuoteType">Determin how to quote the word.</param>
        /// <returns> script </returns>
        string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType );

        /// <summary> Clones this instance. </summary>
        /// <returns> New <see cref="IClause"/> Clone from this instance. </returns>
        IClause Clone();

    } // internal interface IClause

} // namespace PGLibrary.Database
