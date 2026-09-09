using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Threading.Tasks;

namespace Driver.Extensions
{
    public static class SecureCacheSerializer
    {
        private const string StaticDataPrivateKey = "LYxxzb9PHSfas8215325Dsgddh6012GAsgajgASHHA12525zb7gWSRMpHMtWXAYUCun";

        public static async Task<T> DeserializeAsync<T>(string data) where T : class
        {
            if (string.IsNullOrEmpty(data))
                return null;

            return await Task.Run(() =>
            {
                return JsonConvert.DeserializeObject<T>(Decrypt(data), GetJsonSerializerSettings());
            });
        }

        public static async Task<(bool success, T data)> TryDeserializeFromPathAsync<T>(string path) where T : class
        {
            if (!File.Exists(path))
                return (false, default);

            try
            {
                var json = await File.ReadAllTextAsync(path);
                var result = await DeserializeAsync<T>(json);
                return (result != null, result);
            }
            catch (Exception)
            {
                return (false, default);
            }
        }

        public static async Task<string> SerializeAsync(object @object, Formatting formatting = Formatting.Indented)
        {
            return await Task.Run(() =>
            {
                return Serialize(@object, formatting);
            });
        }

        public static string Serialize(object @object, Formatting formatting = Formatting.Indented)
        {
            return Encrypt(JsonConvert.SerializeObject(@object, formatting));
        }

        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings()
            {
                Converters = { new StringEnumConverter(), }
            };
        }

        private static string Decrypt(string data)
        {
            return DataExtensions.Decrypt(data, StaticDataPrivateKey);
        }

        private static string Encrypt(string data)
        {
            return DataExtensions.Encrypt(data, StaticDataPrivateKey);
        }
    }
}