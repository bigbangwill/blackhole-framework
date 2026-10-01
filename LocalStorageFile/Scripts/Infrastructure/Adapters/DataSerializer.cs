using Blackhole.SharedKernel.Contract;
using Newtonsoft.Json;
using System.Text;

namespace Blackhole.LocalStorageFile.Infrastructure
{
    public class DataSerializer : IDataSerializer
    {
        public Result<byte[], SerializationResults> SerializeData<T>(T data)
        {
            if (data == null)
            {
                return Result<byte[], SerializationResults>.Failure(
                    SerializationResults.NullValue);
            }

            try
            {
                string json = JsonConvert.SerializeObject(data);

                return Result<byte[], SerializationResults>.Success(
                    Encoding.UTF8.GetBytes(json));
            }
            catch (JsonException)
            {
                return Result<byte[], SerializationResults>.Failure(
                    SerializationResults.SerializationFailed);
            }
        }
        public Result<T, SerializationResults> DeserializeData<T>(byte[] data)
        {
            try
            {
                string json = Encoding.UTF8.GetString(data);

                T? value = JsonConvert.DeserializeObject<T>(json);

                if (value == null)
                {
                    return Result<T, SerializationResults>.Failure(
                        SerializationResults.NullValue);
                }

                return Result<T, SerializationResults>.Success(value);
            }
            catch (JsonException)
            {
                return Result<T, SerializationResults>.Failure(
                    SerializationResults.InvalidJson);
            }
        }
    }
}
