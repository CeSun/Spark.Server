using Cacheapi;
using CacheServerApi;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServerApi.Tables
{
    public enum DBError
    {
        Success,
        IsExisted,
        IsNotExisted,
        ObjectIsEmpty,
        VersionError,
        UnKnowError,
        TimeOut,
    }
    public abstract class Table<TSub, TKey, TPB> where TSub: Table<TSub, TKey, TPB>, new() where TPB : IMessage<TPB>, new()
    {
        public TPB Base { get; protected set; }
        protected uint Version;
        public static TSub New()
        {
            TSub sub = new TSub();
            sub.Base = new TPB();
            sub.Version = 0;
            sub.Init();
            return sub;
        }

        protected virtual void Init()
        {

        }
        protected virtual RecordInfo GetRecorInfo()
        {
            RecordInfo recordInfo = new RecordInfo();
            recordInfo.Version = Version;
            recordInfo.Table = TableName;
            recordInfo.Key = GetKey();
            recordInfo.Field.Add(new RecordFieldInfo { Field = "base", Data = ByteString.CopyFrom(Base.ToByteArray()) });
            return recordInfo;
        }
        protected virtual void SetData(RecordInfo recordInfo)
        {
            if (recordInfo == null)
                return;
            var parser = new MessageParser<TPB>(() => new TPB());
            Version = recordInfo.Version;
            if (recordInfo.Field.Count > 0)
            {
                Base = parser.ParseFrom(recordInfo.Field[0].Data);
            }
        }
        protected abstract string TableName { get; }
        protected abstract string GetKey();
        protected abstract string GetKey(TKey key);
        public TKey Key { get; protected set; }
        public async Task<DBError> SaveAync()
        {
            var key = GetKey();
            var recordInfo = GetRecorInfo();
            var rsp = await CacheServerModule.Instance.SaveAsync(TableName, key, recordInfo);
            switch (rsp.Item1.Errcode)
            {
                case Cacheapi.EErrno.Succ:
                    Version++;
                    return DBError.Success;
                case Cacheapi.EErrno.RecoreExisted:
                    return DBError.IsExisted;
                case Cacheapi.EErrno.RecoreIsNotExisted:
                    return DBError.IsNotExisted;
                case Cacheapi.EErrno.VersionError:
                    return DBError.VersionError;
                case EErrno.TimeOut:
                    return DBError.TimeOut;
                case Cacheapi.EErrno.TableIsNotExisted:
                case Cacheapi.EErrno.Fail:
                default:
                    return DBError.UnKnowError;
            }
            
        }

        public async Task<DBError> DeleteAync()
        {
            var key = GetKey();
            var rsp = await CacheServerModule.Instance.DeleteAsync(TableName, key);
            switch (rsp.Item1.Errcode)
            {
                case Cacheapi.EErrno.Succ:
                    return DBError.Success;
                case Cacheapi.EErrno.RecoreExisted:
                    return DBError.IsExisted;
                case Cacheapi.EErrno.RecoreIsNotExisted:
                    return DBError.IsNotExisted;
                case Cacheapi.EErrno.VersionError:
                    return DBError.VersionError;
                case EErrno.TimeOut:
                    return DBError.TimeOut;
                case Cacheapi.EErrno.TableIsNotExisted:
                case Cacheapi.EErrno.Fail:
                default:
                    return DBError.UnKnowError;
            }
        }
        public static async Task<(TSub Row, DBError Error)> QueryAync(TKey key)
        {
            var row = new TSub();
            var strKey = row.GetKey(key);
            var rsp = await CacheServerModule.Instance.QueryAsync(row.TableName, strKey);
            DBError error = default;
            switch (rsp.Item1.Errcode)
            {
                case Cacheapi.EErrno.Succ:
                    if (rsp.Item2.Record != null)
                    {
                        row.SetData(rsp.Item2.Record);
                        error = DBError.Success;
                    }
                    else error = DBError.IsNotExisted;
                    break;
                case Cacheapi.EErrno.RecoreExisted:
                    error = DBError.IsExisted;
                    break;
                case Cacheapi.EErrno.RecoreIsNotExisted:
                    error = DBError.IsNotExisted;
                    break;
                case Cacheapi.EErrno.VersionError:
                    error = DBError.VersionError;
                    break;
                case EErrno.TimeOut:
                    error = DBError.TimeOut;
                    break;
                case Cacheapi.EErrno.TableIsNotExisted:
                case Cacheapi.EErrno.Fail:
                default:
                    error = DBError.UnKnowError;
                    break;
            }
            return (row, error);
        }
        public static async Task<DBError> DeleteAync(TKey key)
        {
            var row = new TSub();
            row.Key = key;
            return await row.DeleteAync();
        }
    }
}
