using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PGCafe;

namespace PGLibrary.Database {

    /// <summary> Save Table's Name to use in DBCommand. </summary>
    public class DBTable {

        #region Property & Constructor

        /// <summary> Table's name </summary>
        public string Name { get; }
        
        /// <summary> Create <see cref="DBTable"/> with other DBTable to use in DBCommand. </summary>
        /// <param name="Other"> other DBTable object </param>
        public DBTable( DBTable Other ) {
            this.Name = Other.Name;
        } // public DBTable( DBTable Other )
        
        /// <summary> Create <see cref="DBTable"/> with Name to use in DBCommand. </summary>
        /// <param name="Name"> Table's name </param>
        public DBTable( string Name ) {
            this.Name = Name;
        } // public DBTable( string Name )
        

        /// <summary> Convert this object to sql script by Provider and QuoteType. </summary>
        /// <returns> A sql script of this instance. </returns>
        public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType ) {
            return Provider.Quote( this.Name, QuoteType );
        } // public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType )


        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return Name;
        } // public override string ToString()

        #endregion

    } // public class DBTable

} // namespace PGLibrary.Database
