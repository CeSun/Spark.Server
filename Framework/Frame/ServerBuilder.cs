using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Frame
{
    public class ServerBuilder
    {
        BuildOptions Options;

        Func<ConfigBase> LoadConfigFun;

        private ServerBuilder()
        {
            Options = new BuildOptions();
            LoadConfigFun = ReadConfig<ConfigBase>;
        }
        public static ServerBuilder CreateBuilder(string[] args)
        {
            return new ServerBuilder();
        }

        public void SetConfigType<T>() where T : ConfigBase
        {
            LoadConfigFun = ReadConfig<T>;
        }

        private T ReadConfig<T>() where T: ConfigBase
        {
            using (StreamReader sr = new StreamReader(Options.ConfigFilePath))
            {
                var config = JsonSerializer.Deserialize<T>(sr.BaseStream);
                if (config != null)
                {
                    return config;
                }
                throw new Exception("解析配置文件失败！");
            }
        }
        public void Config(BuildOptions options)
        {
            Options = options;
        }
        public T Build<T>() where T : Application, new()
        {
            return new T()
            {
                Config = LoadConfigFun()
            };
        }
    }

    public class BuildOptions
    {
        public string ConfigFilePath = "Config.json";
    }
}
