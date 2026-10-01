
namespace Blackhole.SharedKernel.Contract
{
    public interface IDataSerializer 
    {
        /// <summary>
        /// Serializes the specified object into a UTF-8 encoded JSON byte array.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="data">The object to serialize.</param>
        /// <returns>A UTF-8 encoded byte array containing the serialized JSON representation of the object.</returns>
        public Result<byte[],SerializationResults> SerializeData<T>(T data);

        /// <summary>
        /// Deserializes a UTF-8 encoded JSON byte array into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="data">The UTF-8 encoded JSON byte array.</param>
        /// <returns>The deserialized object.</returns>      
        public Result<T, SerializationResults> DeserializeData<T>(byte[] data);
    }
}
