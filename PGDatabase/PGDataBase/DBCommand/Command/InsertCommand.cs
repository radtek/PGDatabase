using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PGCafe;
using PGCafe.Object;

namespace PGLibrary.Database {

    /// <summary> Object to generate Insert Script. </summary>
    public class InsertCommand : IDBCommand {

        #region Property & Constructor

        /// <summary> Table object </summary>
        private DBTable Table;
        
        /// <summary> Insert's Column and Value clause </summary>
        private IClause ValuesClause = null;

        /// <summary> Determine how to quote the keyword. </summary>
        private EQuoteType QuoteType = EQuoteType.Auto;

        /// <summary> Create InsertCommand object. </summary>
        /// <param name="QuoteType"> Determine how to quote the keyword </param>
        public InsertCommand( EQuoteType QuoteType = EQuoteType.Auto ) {
            this.QuoteType = QuoteType;
        }  // public InsertCommand( EQuoteType QuoteType = EQuoteType.Auto )

        /// <summary> Create InsertCommand object and generate script from source with Table and Values. </summary>
        /// <param name="source">get Table and Values from source.</param>
        /// <param name="QuoteType"> Determine how to quote the keyword </param>
        public InsertCommand( IDatabaseTable source, EQuoteType QuoteType = EQuoteType.Auto )
            : this( QuoteType ) {
            this.InsertInTo( source.GetType() ).Values( source.Columns( EConvertType.ToDB ) );
        }  // public InsertCommand( IDatabaseTable source, EQuoteType QuoteType = EQuoteType.Auto )

        #endregion

        #region InsertInTo

        /// <summary> Specify Insert into which table </summary>
        /// <param name="Table"> table to Insert </param>
        public InsertCommand InsertInTo( DBTable Table ) {
            this.Table = Table;
            return this;
        } // public InsertCommand InsertInTo( DBTable Table )

        /// <summary>
        /// Specify Insert into which table by Type of IDatabaseTable
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        /// </summary>
        /// <param name="DBTableType"> Type to get Table name </param>
        public InsertCommand InsertInTo( Type DBTableType ) {
            this.Table = IDatabaseTableExtension.Table( DBTableType );
            return this;
        } // public InsertCommand InsertInTo( Type DBTableType )
        
        /// <summary>
        /// Specify Insert into which table by Type of IDatabaseTable
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        /// </summary>
        /// <typeparam name="T">Type to get table name.</typeparam>
        public InsertCommand InsertInTo<T>()
            where T : IDatabaseTable {
            return this.InsertInTo( typeof ( T ) );
        } // public InsertCommand InsertInTo<T>()

        #endregion

        #region Values
        
        /// <summary> Use speficy Values script and Parameter list to generate Values clause. </summary>
        /// <param name="ColumnScript">The Values's column script.</param>
        /// <param name="ValueScript">The Values's value script.</param>
        /// <param name="Parameters">Parameter list which specify the value in ValuesScript.</param>
        /// <param name="throwExceptionIfParameterNotMatch">
        /// if any parameter in parameter list hasn't be used
        /// or any parameter in CommandText hasn't be replace, throw exception. </param>
        public InsertCommand ValuesScript( string ColumnScript, string ValueScript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true ) {
            this.ValuesClause = new InsertValuesScript( ColumnScript, ValueScript, Parameters, throwExceptionIfParameterNotMatch );
            return this;
        } // public InsertCommand ValuesScript( string ColumnScript, string ValueScript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true )
        
        /// <summary> Use speficy Values script and Parameter list to generate Values clause. </summary>
        /// <param name="ColumnScript">The Values's column script.</param>
        /// <param name="ValueScript">The Values's value script.</param>
        /// <param name="Parameters">Parameter list which specify the value in ValuesScript.</param>
        public InsertCommand ValuesScript( string ColumnScript, string ValueScript, params DBParameter[] Parameters ) {
            return this.ValuesScript( ColumnScript, ValueScript, Parameters.CastTo<IEnumerable<DBParameter>>() );
        } // public InsertCommand ValuesScript( string ColumnScript, string ValueScript, params DBParameter[] Parameters )
        
        /// <summary> Generate Values script by DBParameters. </summary>
        /// <param name="Parameters">Parameter list to generate to Values script.</param>
        public InsertCommand Values( IEnumerable<DBParameter> Parameters ) {
            this.ValuesClause = new InsertValuesParameters( Parameters );
            return this;
        } // public InsertCommand Values( IEnumerable<DBParameter> Parameters )
        
        /// <summary> Generate Values script by DBParameters. </summary>
        /// <param name="Parameters">Parameter list to generate to Values script.</param>
        public InsertCommand Values( params DBParameter[] Parameters )
            => this.Values( Parameters.CastTo<IEnumerable<DBParameter>>() );

        #endregion

        #region InsertIntoValues
        
        /// <summary> 
        /// Generate Values script by DBParameters and Specify Insert into which table by Type of IDatabaseTable
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        ///  </summary>
        /// <typeparam name="T">Type to get table name.</typeparam>
        /// <param name="Parameters">Parameter list to generate to Values script.</param>
        public InsertCommand InsertInToValues<T>( IEnumerable<DBParameter> Parameters )
            where T : IDatabaseTable {
            this.InsertInTo( typeof ( T ) ).ValuesClause = new InsertValuesParameters( Parameters );
            return this;
        } // public InsertCommand InsertInToValues<T>( IEnumerable<DBParameter> Parameters )
        
        /// <summary>
        /// Generate Values script by DBParameters and Specify Insert into which table by Type of IDatabaseTable
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        ///  </summary>
        /// <typeparam name="T">Type to get table name.</typeparam>
        /// <param name="Parameters">Parameter list to generate to Values script.</param>
        public InsertCommand InsertInToValues<T>( params DBParameter[] Parameters )
            where T : IDatabaseTable
            => this.InsertInTo( typeof ( T ) ).Values( Parameters.CastTo<IEnumerable<DBParameter>>() );

        #endregion

        #region ToScript, Clone

        /// <summary> Generate the Insert script. </summary>
        /// <param name="Provider"> Database Provider to get some personal format. </param>
        /// <param name="AddCommandSpliter">Add command spliter at the end or not.</param>
        public string ToScript( IDatabaseProvider Provider, bool AddCommandSpliter = false ) {
            // generate insert value clause
            string ValuesString = null;
            if ( this.ValuesClause?.AnyClause() ?? false )
                ValuesString = this.ValuesClause.ToScript( Provider, this.QuoteType );
            
            // generate whold command.
            var result = $"Insert Into {this.Table.ToScript( Provider, QuoteType )}{ValuesString}";
            if ( AddCommandSpliter )
                result += Provider.CommandSpliter;

            return result;
        } // public string ToScript( IDatabaseProvider Provider, bool AddCommandSpliter = false )
        
        /// <summary> Clone this object. </summary>
        public InsertCommand Clone() {
            var result = (InsertCommand)this.MemberwiseClone();
            result.ValuesClause = result.ValuesClause.Clone();
            return result;
        } // public InsertCommand Clone()

        #endregion

    } // public class InsertCommand : IDBCommand
    
} // namespace PGLibrary.Database
