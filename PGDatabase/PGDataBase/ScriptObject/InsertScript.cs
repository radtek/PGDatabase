using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PGCafe;
using PGCafe.Object;
using PGLibrary.Data;

namespace PGLibrary.Data {
    /// <summary> Object to generate Insert Script. </summary>
    public class InsertScript {

        #region Property

        /// <summary> Table name to insert into. </summary>
        protected string TableName;

        /// <summary> Column name and values to insert. </summary>
        protected ListS<string,object> InsertValues = new ListS<string, object>();

        /// <summary> Determine how to quote the keyword. </summary>
        protected EQuoteType QuoteType = EQuoteType.Auto;

        /// <summary> DataBase provider to provide specific infomation </summary>
        public IDataBaseProvider Provider { get; private set; }

        #endregion

        #region Constructor and assign data method

        /// <summary> Create InsertScript object. </summary>
        /// <param name="Provider"> DataBase provider to provide specific infomation. </param>
        /// <param name="QuoteType"> Determine how to quote the keyword </param>
        public InsertScript( IDataBaseProvider Provider, EQuoteType QuoteType = EQuoteType.Auto ) {
            this.Provider = Provider;
            this.QuoteType = QuoteType;
        }  // public InsertScript( IDataBaseProvider Provider, EQuoteType QuoteType = EQuoteType.Auto )
        
        /// <summary> Set table name to insert into. </summary>
        /// <param name="TableName"> Table name to insert into. </param>
        public virtual InsertScript InsertInto( string TableName ) {
            this.TableName = TableName;
            return this;
        } // public virtual InsertScript InsertInto( string TableName )

        /// <summary> add the insert value with column name. </summary>
        /// <param name="Column"> Column name </param>
        /// <param name="Value"> Value </param>
        public virtual InsertScript Value( string Column, object Value ) {
            InsertValues.Add( Column, Value );
            return this;
        } // public override InsertScript Value( string Column, object Value )

        /// <summary> Add column name and value by [ColumnAttribute] of entity. </summary>
        /// <typeparam name="T"> type of Entity </typeparam>
        /// <param name="Entity"> the Entity of column name and value. </param>
        public virtual InsertScript Values<T>( T Entity ) {
            
            // get mapping member from target type.
            var membersHasColumnAttribute = _DBColumnAttribute_MemberInfo.GetCorrespondMember( typeof( T ) );

            // get member with column name and first member with column name.
            var members = membersHasColumnAttribute
                .Select( item => new { ColumnName = item.Key, MemberInfo = item.Value.First() } )
                .Where( item => item.MemberInfo.ConvertType.HasFlag( EConvertType.ToDB ) );
            
            // for each member, try get value from datarow.
            foreach ( var member in members ) {
                var valueObj = member.MemberInfo.GetValue( Entity );
                InsertValues.Add( member.ColumnName, valueObj );
            } // foreach

            return this;
        } // public override InsertScript Values<T>( T Entity )

        #endregion

        #region ToString, Clone
        
        /// <summary> generate the Insert script. </summary>
        public override string ToString() {
            string columnString = string.Join( ", ", InsertValues.Select( item => $"{Provider.Quote( item.Item1, QuoteType )}" ) );
            string valueString = string.Join( ", ", InsertValues.Select( item => $"{Provider.ToDBValueString( item.Item2 )}" ) );

            return $"Insert Into {Provider.Quote( TableName, QuoteType )} ( {columnString} ) Values ( {valueString} )";
        } // public override string ToString()
        
        /// <summary> Clone this object. </summary>
        public virtual InsertScript Clone() {
            var result = this.MemberwiseClone().CastTo<InsertScript>();
            result.InsertValues = new ListS<string, object>( this.InsertValues );
            return result;
        } // public virtual InsertScript Clone()

        #endregion

    } // public class InsertScript
} // namespace PGLibrary.Data
