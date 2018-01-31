using System;
using System.Data;
using PGCafe.Object;

namespace PGLibrary.Database {
    
    /// <summary>
    /// Create object to manager sequence No with tables.
    /// * use enum type to enumerate all tables would be used, to avoid input error table name by string type.
    /// </summary>
    /// <typeparam name="TEntity"> type of entity to correspond table. </typeparam>
    /// <typeparam name="TKeyEnum"> enum type contains all keys, enum's field name should exact with key. </typeparam>
    public class DBSequenceManager<TEntity,TKeyEnum>
        where TEntity : SequenceManagerTable<TKeyEnum>, new()
        where TKeyEnum : struct, IConvertible {

        private PGDatabase mDBObj; // db spool object.
        private object mGetSequenceLocker = new object(); // locker when get Seq No
        
        /// <summary> Initial DBOrderManage with PGDatabase object and table name. </summary>
        /// <param name="PGDatabase"> PGDatabase object to access table. </param>
        public DBSequenceManager( PGDatabase PGDatabase ) {
            // Initial
            this.mDBObj = PGDatabase;
            //mLog?.Info( $"Initial DBSequenceManage by SequenceTableName : {SequenceTableName}" );
        } // public DBSequenceManager( PGDatabase PGDatabase )
                
        /// <summary> Get next sequence by key. </summary>
        /// <param name="Key"> which key need to generate. </param>
        public SingleResult<int> Generate( TKeyEnum Key ) {
            var tableName = Key.ToString();
            //mLog?.Info( $"Start get Sequence, Table Name : {tableName}" );

            // though the table should lock by transaction, but lock again in program to avoid begin twice transaction.
            lock ( mGetSequenceLocker ) {
                var result = mDBObj.TransactionExecute( () => {
                    
                    var entity = new TEntity();
                    entity.Key = Key;
                    
                    // get Sequence by table name.
                    var queryResult = mDBObj.ExecuteScalar<int?>( new SelectCommand()
                        .SelectFrom<TEntity>( IDatabaseTableExtension.Columns<TEntity>( nameof( entity.Sequence ) ) )
                        .WhereAnd( entity.Columns( nameof( entity.Key ) ) ) );

                    if ( !queryResult.Success ) throw queryResult.Exception;
                    
                    // if no result, means table name not exist, insert new row by table name with Sequence = 1.
                    if ( queryResult.Value == null ) {
                        //mLog?.Info( $"no TableName exist, insert new one : {tableName}" );
                        entity.Sequence = entity.InitialSequence( mDBObj );
                        entity.Insert( mDBObj );
                    } // if

                    // if has result, increace one by result and update it to table.
                    else {
                        entity.Sequence = queryResult.Value.Value + 1;
                        entity.Update( mDBObj );
                    } // else

                    return entity.Sequence;
                }, IsolationLevel.RepeatableRead );

                return result;
            } // lock
        } // public SingleResult<int> Generate( TKeyEnum Key )
        
    } // public class DBSequenceManager

} // namespace MayaFramework
