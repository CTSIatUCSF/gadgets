using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using UCSF.Data;
using log4net;

namespace UCSF.Business.DataImporter
{
    public class LinqEntityDataReader<T> :IDataReader where T : class
    {
        private DataTable _sourceTable;
        private readonly IEnumerator<T> _enumerator;
        private DataRow _current;
        private ILog log = LogManager.GetLogger("LinqEntityDataReader");

        public LinqEntityDataReader(IEnumerable<T> source)
        {
            MapTableName();
            MapColumns();
            _enumerator = source.GetEnumerator();
        }

        public string DestenationTable
        {
            get { return _sourceTable.TableName; }
        }

        public IEnumerable<string> Columns
        {
            get
            {
                foreach (DataColumn column in _sourceTable.Columns)
                {
                    yield return column.ColumnName;
                }
            }
        }

        public object GetValue(int columnIndex)
        {
            return _current[columnIndex];
        }

        public int GetOrdinal(string name)
        {
            return _sourceTable.Columns[name].Ordinal;
        }

        public bool Read()
        {
            bool next = _enumerator.MoveNext();

            if (next)
            {
                _current = _sourceTable.NewRow();

                foreach (DataColumn column in _sourceTable.Columns)
                {
//                    if (_enumerator.Current == null)
//                    {
//                        
//                    }
                    _current[column] = typeof(T).GetProperty(column.ColumnName).GetValue(_enumerator.Current, null) ?? DBNull.Value;
                }
            }

            return next;
        }

        public int FieldCount
        {
            get { return _sourceTable.Columns.Count; }
        }

        private void MapTableName()
        {
            Type entityType = typeof(T);

            TableAttribute destenationTable = entityType
                .GetCustomAttributes(typeof(TableAttribute), false)
                .Cast<TableAttribute>()
                .SingleOrDefault();

            if (destenationTable == null)
            {
                throw new ArgumentNullException("destenationTable");
            }

            _sourceTable = new DataTable(destenationTable.Name);
        }

        private void MapColumns()
        {
            Type entityType = typeof(T);
            int columnIndex = 0;

            foreach (PropertyInfo property in entityType.GetProperties())
            {
                ColumnAttribute column = property
                    .GetCustomAttributes(typeof(ColumnAttribute), false)
                    .Cast<ColumnAttribute>()
                    .SingleOrDefault();

                if (column == null)
                {
                    return;
                }

                if (!column.IsVersion && !column.DbType.Contains("IDENTITY") && !column.IsDbGenerated)
                {
                    DataColumn col = new DataColumn(column.Name ?? property.Name);
                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        col.AllowDBNull = true;
                        col.DataType = property.PropertyType.GetGenericArguments()[0];
                    }
                    else
                    {
                        col.DataType = property.PropertyType;
                        //_sourceTable.Columns.Add(column.Name ?? property.Name, property.PropertyType);
                    }
                    _sourceTable.Columns.Add(col);
                }
            }

            columnIndex++;
        }

        #region stub
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        object IDataRecord.this[int i]
        {
            get { throw new NotImplementedException(); }
        }

        object IDataRecord.this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

    }

    public static class TableExtension
    {
        public static void BulkInsert<T>(this Table<T> entity, IEnumerable<T> data, int batchSize)where T : class
        {
            LinqEntityDataReader<T> reader = new LinqEntityDataReader<T>(entity.ToList());

            using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(entity.Context.Connection.ConnectionString))
            {
                foreach (string columnName in reader.Columns)
                {
                    sqlBulkCopy.ColumnMappings.Add(columnName, columnName);
                }

                sqlBulkCopy.BatchSize = data.Count();
                sqlBulkCopy.DestinationTableName = reader.DestenationTable;
                sqlBulkCopy.WriteToServer(new LinqEntityDataReader<T>(data));
                sqlBulkCopy.Close();
            }
        }
    }
}