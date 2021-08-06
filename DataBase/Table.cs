using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace DataBase
{
    public enum DBError
    {
        Success,
        IsExisted,
        IsNotExisted,
        ObjectIsEmpty,
        VersionError,
        UnKnowError
    }

    public abstract class Table<TProto, TKey, TTable> where TProto : IMessage<TProto>, new() where TTable : Table<TProto, TKey, TTable>, new()
    {
        public static TTable New() {
            var table = new TTable();
            table.Value = new TProto();
            return table;
        }

#pragma warning disable CS8618
        public Table()
        {

        }
#pragma warning restore CS8618 

        public TProto Value { get; private set; }

        public abstract string TableName {  get; }

        long version = 0;
        protected abstract string GetKey();
        protected abstract string GetKey(TKey key);
        public async Task<DBError> SaveAync()
        {
            byte[] bitData = null;
            bitData = Value.ToByteArray();
            var keyString = GetKey();
            // 版本号是0即新增
            if (version == 0)
            {
                using (var conn = Database.GetConnection())
                {
                    try
                    {
                        await conn.OpenAsync();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = string.Format("insert into {0} (c_key, c_value, c_version) values(@key,@value,@version);", TableName);
                        cmd.Parameters.AddWithValue("@key", keyString);
                        cmd.Parameters.AddWithValue("@value", bitData);
                        cmd.Parameters.AddWithValue("@version", 1);
                        var line = await cmd.ExecuteNonQueryAsync();
                        if (line == 1)
                        {
                            version = 1;
                            return DBError.Success;
                        }
                        else
                            return DBError.IsExisted;
                    }
                    catch (MySqlException ex)
                    {
                        if (ex.Number == 1062)
                        {
                            return DBError.IsExisted;
                        }
                        return DBError.UnKnowError;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                        return DBError.UnKnowError;
                    }

                }

            }
            else
            {
                using (var conn = Database.GetConnection())
                {
                    try
                    {
                        await conn.OpenAsync();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = string.Format("update {0} set c_value=@value, c_version=@newVersion where c_key=@key and c_version=@oldVersion;", TableName);
                        cmd.Parameters.AddWithValue("@key", keyString);
                        cmd.Parameters.AddWithValue("@value", bitData);
                        cmd.Parameters.AddWithValue("@oldVersion", version);
                        cmd.Parameters.AddWithValue("@newVersion", version + 1);
                        var line = await cmd.ExecuteNonQueryAsync();
                        if (line == 1)
                        {
                            version = version + 1;
                            return DBError.Success;
                        }
                        else
                            return DBError.VersionError;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                        return DBError.UnKnowError;
                    }
                    
                }
            }
        }

        public async Task<DBError> DeleteAync()
        {
            if (version == 0)
                return DBError.ObjectIsEmpty;
            var keyString = GetKey();
            using (var conn = Database.GetConnection())
            {
                try
                {
                    await conn.OpenAsync();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = string.Format("delete from {0} where c_key=@key;", TableName);
                    cmd.Parameters.AddWithValue("@key", keyString);
                    var line = await cmd.ExecuteNonQueryAsync();
                    if (line == 1)
                    {
                        version = 0;
                        return DBError.Success;
                    }
                    else
                        return DBError.IsExisted;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    return DBError.UnKnowError;
                }
            }

        }

        public static async Task<(TTable? Row, DBError Error)> QueryAync(TKey key) 
        {
            TTable table = new TTable();
            var tableName = table.TableName;
            var keyString = table.GetKey(key);
            MessageParser<TProto> parser = new MessageParser<TProto>(() => new TProto());
            using (var conn = Database.GetConnection())
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("select c_key, c_value, c_version from {0} where c_key=@key;", tableName);
                cmd.Parameters.AddWithValue("@key", keyString);
                try
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            await reader.GetFieldValueAsync<string>("c_key");
                            var buffer = await reader.GetFieldValueAsync<byte[]>("c_value");
                            var version = await reader.GetFieldValueAsync<long>("c_version");
                            table.Value = parser.ParseFrom(buffer);
                            table.version = version;
                            return (table, DBError.Success);
                        }
                        else
                        {
                            return (null, DBError.IsNotExisted);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    return (null, DBError.UnKnowError);
                }
            }
            
        }

        public static async Task<DBError> DeleteAync(TKey key)
        {
            TTable table = new TTable();
            var keyString = table.GetKey(key);
            var tableName = table.TableName;
            using (var conn = Database.GetConnection())
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("delete from {0} where c_key=@key;", tableName);
                cmd.Parameters.AddWithValue("@key", keyString);
                try
                {
                    var line = await cmd.ExecuteNonQueryAsync();
                    if (line == 1)
                    {
                        return DBError.Success;
                    }
                    else
                        return DBError.IsExisted;
                }
                catch
                {
                    return DBError.UnKnowError;
                }
            }
        }

    }
}
