using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cacheapi;
using CacheServer.Modules;
using Frame;
using StackExchange.Redis;

namespace CacheServer.Tables
{
    public abstract class Table
    {
        protected abstract Dictionary<string, string> Fields { get; }
        protected abstract string TableName { get; }
        public async Task<(RecordInfo, EErrno)> QueryAsync(string key)
        {
            EErrno errno;
            var redisKey = string.Format("{0}|{1}", TableName, key);
            var redis = Redis.Instance.Database;
            if (redis == null) return (null, EErrno.Fail);
            if (await redis.SetContainsAsync("DeleteDirtyKey", redisKey))
                return (null, EErrno.RecoreIsNotExisted);
            var result = await redis.HashGetAllAsync(redisKey);
            RecordInfo rtv = null;
            do
            {
                if (result.Length > 0)
                {
                    rtv = HashEntrysToPb(result);
                    errno = EErrno.Succ;
                }
                else
                {
                    var res = await LoadFromMysqlAsync(key);
                    if (res.err != EErrno.Succ)
                    {
                        errno = res.err;
                        break;
                    }
                    var entrys = await SaveRedisAsync(key, res.Item1);
                    if (entrys.err != EErrno.Succ)
                    {
                        errno = entrys.err;
                        break;
                    }
                    rtv = HashEntrysToPb(entrys.Item1);
                    errno = EErrno.Succ;
                }
            } while (false);
            return (rtv, errno);
        }
        public async Task<EErrno> UpdateAsync(string key, RecordInfo record)
        {
            var redisKey = string.Format("{0}|{1}", TableName, key);
            var redis = Redis.Instance.Database;

            if (await redis.SetContainsAsync("DeleteDirtyKey", redisKey))
                return EErrno.RecoreIsNotExisted;
            var res = await QueryAsync(key);
            if (res.Item2 != EErrno.Succ)
               return res.Item2;
            var updateLua = @"if redis.call('SISMEMBER', 'DeleteDirtyKey', KEYS[1]) ~= 0  then
                                return -1    
                            end
                            if tonumber(redis.call('HGET', KEYS[1], 'version')) == tonumber(ARGV[1]) then 
                                for i = 1, (#ARGV - 1) do
                                    redis.call('HSET',KEYS[1], KEYS[i+1], ARGV[i + 1])
                                end
                                redis.call('HSET',KEYS[1], 'version', ARGV[1] + 1)
                                redis.call('SADD', 'DirtyKey', KEYS[1])
                                redis.call('EXPIRE', KEYS[1], 60 * 60)
                                return 1
                            else 
                                return -2
                            end";
            var entrys = PbToHashEntrys(record);
            var keys = new RedisKey[entrys.Length + 1];
            var args = new RedisValue[entrys.Length + 1];
            keys[0] = redisKey;
            args[0] = record.Version;
            for (int i = 0; i < entrys.Length; i++)
            {
                keys[i + 1] = entrys[i].Name.ToString();
                args[i + 1] = entrys[i].Value;
            }
            var result = await redis.ScriptEvaluateAsync(updateLua, keys, args);
            if (((int)result) == 1)
            {
                return EErrno.Succ;
            }
            else
            {
                return EErrno.VersionError;
            }



        }
        public async Task<EErrno> InsertAsync(string key, RecordInfo record)
        {
            record.Version = 1;
            var redisKey = string.Format("{0}|{1}", TableName, key);
            var redis = Redis.Instance.Database;
            if (redis == null)
                return EErrno.Fail;
            var b = await QueryAsync(key);
            if (b.Item2 == EErrno.Succ)
                return EErrno.RecoreExisted;
            var entrys = PbToHashEntrys(record);
            var err = await InsertRedisAsync(redisKey, entrys);
            return err;
        }
        public async Task<EErrno> DeleteAsync(string key)
        {
            var redis = Redis.Instance.Database;
            var redisKey = string.Format("{0}|{1}", TableName, key);
            var ret = await QueryAsync(key);
            if (ret.Item2 != EErrno.Succ)
                return ret.Item2;
            try
            {
                var result = await redis.ScriptEvaluateAsync(@"
                            if redis.call('SISMEMBER', 'DeleteDirtyKey', KEYS[1]) ~= 0  then
                                return -1
                            end
                            if redis.call('EXISTS', KEYS[1] ) == 0 then
                                return -2
                            end
                            redis.call('DEL', KEYS[1])
                            redis.call('SADD', 'DeleteDirtyKey', KEYS[1])
                            return 1
            ", new RedisKey[] { redisKey });
                if (((int)result) == 1)
                    return EErrno.Succ;
                else
                    return EErrno.Fail;
            } catch (Exception ex)
            {

            }

            return EErrno.Fail;
        }
        private RecordInfo HashEntrysToPb(HashEntry[] entrys)
        {
            RecordInfo recordInfo = new RecordInfo();
            foreach (var entry in entrys)
            {
                if (((string)entry.Name) == "version")
                {
                    recordInfo.Version = ((uint)entry.Value);
                }
                else
                {
                    if (Fields.FirstOrDefault(RES => RES.Value == entry.Name).Value == default)
                        continue;
                    var recordField = new RecordFieldInfo();
                    recordField.Field = entry.Name;
                    recordField.Data = Google.Protobuf.ByteString.CopyFrom(entry.Value);
                    recordInfo.Field.Add(recordField);
                }
            }
            return recordInfo;
        }
        private HashEntry[] PbToHashEntrys(RecordInfo record)
        {
            List<HashEntry> entrys = new List<HashEntry>();
            foreach (var field in record.Field)
            {
                if (!Fields.ContainsValue(field.Field))
                    continue;
                var key = field.Field;
                var value = field.Data.ToByteArray();
                HashEntry hashEntry = new HashEntry(key, value);
                entrys.Add(hashEntry);
            }
            entrys.Add(new HashEntry("version", record.Version));

            return entrys.ToArray();
        }
        private async Task<(HashEntry[], EErrno err)> LoadFromMysqlAsync(string key)
        {
            List<HashEntry> entrys = new List<HashEntry>();
            using (var mysql = await Mysql.Instance.BorrowAsync())
            {
                if (mysql == null) return (null, EErrno.Fail);
                var cmd = mysql.Connector.CreateCommand();
                cmd.CommandText = string.Format("select * from {0} where c_key=@key", TableName);
                cmd.Parameters.AddWithValue("key", key);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var version = await reader.GetFieldValueAsync<uint>("c_version");
                        HashEntry entry = new HashEntry("version", version);
                        entrys.Add(entry);
                        foreach (var param in Fields)
                        {
                            var data = await reader.GetFieldValueAsync<byte[]>(param.Key);
                            entry = new HashEntry(param.Value, data);
                            entrys.Add(entry);
                        }
                        return (entrys.ToArray(), EErrno.Succ);
                    }
                    else
                    {
                        return (null, EErrno.RecoreIsNotExisted);
                    }

                }
            }

        }
        private async Task<EErrno> RecordIsExistedAsync(string key)
        {
            using (var mysql = await Mysql.Instance.BorrowAsync())
            {
                if (mysql == null) return EErrno.Fail;
                var cmd = mysql.Connector.CreateCommand();
                cmd.CommandText = string.Format("select 1 from {0} where c_key=@key", TableName);
                cmd.Parameters.AddWithValue("key", key);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return EErrno.Succ;
                    }
                    else
                    {
                        return EErrno.RecoreIsNotExisted;
                    }

                }
            }

        }
        public async Task<EErrno> SaveToMysqlAsync(string key, HashEntry[] entrys)
        {
            var sql = "update {0} set {1} where c_key=@key;";
            string param = null;
            foreach (var entry in entrys)
            {
                var colum = "";
                if (entry.Name == "version")
                    colum = "c_version";
                else
                    colum = Fields.FirstOrDefault(res => res.Value == entry.Name).Key;
                if (param == null)
                    param = colum + "=@" + entry.Name;
                param += ", " + colum + "=@" + entry.Name;
            }
            sql = string.Format(sql, TableName, param);
            using (var mysql = await Mysql.Instance.BorrowAsync())
            {
                if (mysql == null)
                    return EErrno.Fail;
                var cmd = mysql.Connector.CreateCommand();
                cmd.Parameters.AddWithValue("@key", key);
                foreach (var entry in entrys)
                {
                    if (entry.Name == "version")
                    {
                        cmd.Parameters.AddWithValue("@" + entry.Name, (uint)entry.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@" + entry.Name, (byte[])entry.Value);
                    }
                }
                cmd.CommandText = sql;
                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 1)
                {
                    return EErrno.Succ;
                }
                else
                {
                    sql = "insert into {0}({1}) values({2});";
                    var column = "`c_key`";
                    var values = "@key";
                    cmd = mysql.Connector.CreateCommand();
                    cmd.Parameters.AddWithValue("@key" , key);
                    foreach (var entry in entrys)
                    {
                        values += ", @" + entry.Name;
                        if (entry.Name == "version")
                        {
                            column += ",`c_version`";
                            cmd.Parameters.AddWithValue("@" + entry.Name, (uint)entry.Value);
                        }
                        else
                        {
                            column += ",`" + Fields.FirstOrDefault(res => res.Value == entry.Name).Key + "`";
                            cmd.Parameters.AddWithValue("@" + entry.Name, (byte[])entry.Value);
                        }
                    }
                    cmd.CommandText = string.Format(sql, TableName, column, values);
                    rows = await cmd.ExecuteNonQueryAsync();
                    if (rows == 1)
                    {
                        return EErrno.Succ;
                    }
                    // todo 更新失败的时候要保存
                    return EErrno.Fail;
                }
            }


        }
        public async Task<EErrno> DeleteFromMysqlAsync(string key)
        {
            var sql = "delete from {0} where c_key=@key;";
            using (var conn = Mysql.Instance.Borrow())
            {
                var mysql = conn.Connector;
                var cmd = mysql.CreateCommand();
                cmd.CommandText = string.Format(sql, TableName);
                cmd.Parameters.AddWithValue("@key", key);

                var lines = await cmd.ExecuteNonQueryAsync();
                if (lines == 1)
                    return EErrno.Succ;
                else
                    return EErrno.RecoreIsNotExisted;
            }

        }
        private async Task<(HashEntry[], EErrno err)> SaveRedisAsync(string key, HashEntry[] entrys)
        {
            var redis = Redis.Instance.Database;
            if (redis == null) return (null, EErrno.Fail);
            var saveLua = @"if redis.call('SISMEMBER', 'DeleteDirtyKey', KEYS[1]) == 0  then
                                return nil    
                            end
                            if redis.call('EXISTS', KEYS[1]) == 0 then 
                                for i = 1, #ARGV do
                                    redis.call('HSET',KEYS[1], KEYS[i+1], ARGV[i])
                                end
                            end
                            return redis.call('hgetall', KEYS[1])";
            var keys = new RedisKey[entrys.Length + 1];
            var args = new RedisValue[entrys.Length];
            keys[0] = key;
            for (int i = 0; i < entrys.Length; i++)
            {
                keys[i + 1] = entrys[i].Name.ToString();
                args[i] = entrys[i].Value;
            }
            try
            {
                var result = await redis.ScriptEvaluateAsync(saveLua, keys, args);
                if (result.IsNull)
                    return (null, EErrno.RecoreIsNotExisted);
                HashEntry[] resEntrys = new HashEntry[((RedisResult[])result).Length / 2];
                int j = 1;
                (RedisValue key, RedisValue value) pari = default;
                foreach (var en in (RedisResult[])result)
                {
                    if (j % 2 == 1)
                    {
                        pari.key = ((RedisValue)en);
                    }
                    else
                    {
                        pari.value = ((RedisValue)en);
                        resEntrys[j / 2 - 1] = new HashEntry(pari.key, pari.value);
                    }
                    j++;
                }
                return (resEntrys, EErrno.Succ);
            } catch (Exception ex)
            {

            }
            return (null, EErrno.Fail);


        }
        private async Task<EErrno> InsertRedisAsync(string key, HashEntry[] entrys)
        {
            var insretLua = @"if redis.call('EXISTS', KEYS[1]) ~= 0 then 
                                return 0
                            end
                            for i = 1, #ARGV do
                                redis.call('HSET',KEYS[1], KEYS[i+1], ARGV[i])
                            end
                            redis.call('EXPIRE', KEYS[1], 60 * 60)
                            redis.call('SADD', 'DirtyKey', KEYS[1])
                            redis.call('SREM', 'DeleteDirtyKey', KEYS[1])
                            return 1";
            var keys = new RedisKey[entrys.Length + 1];
            var args = new RedisValue[entrys.Length];
            keys[0] = key;
            for (int i = 0; i < entrys.Length; i++)
            {
                keys[i + 1] = entrys[i].Name.ToString();
                args[i] = entrys[i].Value;
            }
            var redis = Redis.Instance.Database;
            if (redis == null) return EErrno.Fail;
            var result = await redis.ScriptEvaluateAsync(insretLua, keys, args);
            if (((int)result) == 1)
            {
                return EErrno.Succ;
            }
            else
            {
                return EErrno.RecoreExisted;
            }
        }
        
    }
}
