using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PGCafe;

namespace PGLibrary.Database {

    /// <summary> Save Column's Name to use in DBCommand. </summary>
    public class DBColumn {

        #region Property & Constructor

        /// <summary> Column's name </summary>
        public string Name { get; }
        
        /// <summary> Column rule to use at this Column. </summary>
        public EColumnRule ColumnRule { get; }
        
        /// <summary> Create <see cref="DBColumn"/> with Name to use in DBCommand. </summary>
        /// <param name="Name"> Column's name </param>
        /// <param name="ColumnRule"> Column rule to use at this Column. </param>
        public DBColumn( string Name, EColumnRule ColumnRule = EColumnRule.None ) {
            this.Name = Name; this.ColumnRule = ColumnRule;
        } // public DBColumn( string Name, EColumnRule ColumnRule = EColumnRule.None )
        

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

    } // public class DBColumn

} // namespace PGLibrary.Database
