using Google.Protobuf;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
    public enum DBError
    {
        Success,
        IsExisted,
        ObjectIsEmpty,
        UnKnowError
    }
    public abstract class Table<TProto, TKey> where TProto: IMessage<TProto>, new()
    {
        public TProto Value { get; private set; }
        public TKey Key { get; private set; }

        public abstract string TableName {  get; }

        int version = 0;
        protected abstract string GetKey();
        protected abstract string GetKey(TKey key);


        public async Task<DBError> SaveAync()
        {
            var bitData = Value.ToByteArray();
            var keyString = GetKey();
            // 版本号是0即新增
            if (version == 0)
            {
                var sql = string.Format("insert into {0}(key, value, version) values(@key,@value,@version);", TableName);

                var conn = MySqlDBContext.Instance.Database.GetDbConnection();
                VAR cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@key", keyString);
                cmd.Parameters.AddWithValue("@value", bitData);
                cmd.Parameters.AddWithValue("@version", 1);
                try { 
                    var line = await cmd.ExecuteNonQueryAsync();
                    if (line == 1)
                    {
                        version = 1;
                        return DBError.Success;
                    }
                    else
                        return DBError.IsExisted;
                }
                catch (MySqlException ex) {
                    if (ex.Code == 1602)
                    {
                        return DBError.IsExisted;
                    }
                    return DBError.UnKnowError;
                }
                catch
                {
                    return DBError.UnKnowError;
                }
                finally
                {
                    MysqlConn.Instance.Close();
                }

            }
            else
            {
                var sql = string.Format("update {0} set value=@value, version=@newVersion where key=@key and version=@oldVersion;", TableName);
                MysqlConn.Instance.Open();
                MySqlCommand cmd = new MySqlCommand(sql, MysqlConn.Instance);
                cmd.Parameters.AddWithValue("@key", keyString);
                cmd.Parameters.AddWithValue("@value", bitData);
                cmd.Parameters.AddWithValue("@oldVersion", version);
                cmd.Parameters.AddWithValue("@newVersion", version + 1);
                try
                {
                    var line = await cmd.ExecuteNonQueryAsync();
                    if (line == 1)
                    {
                        version = version + 1;
                        return DBError.Success;
                    }
                    else
                        return DBError.IsExisted;
                }
                catch
                {
                    return DBError.UnKnowError;
                }
                finally
                {
                    MysqlConn.Instance.Close();
                }
            }
        }

        public async Task<DBError> DeleteAync()
        {
            if (version == 0)
                return DBError.ObjectIsEmpty;

            return DBError.Success;
        }

        static async Task<Table<TProto, TKey>> QuertAync(TKey key)
        {
            return null;
        }

        static async Task<DBError> DeleteAync(TKey key)
        {
            return DBError.Success;
        }

    }
}
