using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

namespace CrossCutting.Utilitarios
{
    public static class ObjectCopier
    {
        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(source));
            }

            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                JsonSerializer.SerializeAsync(stream, source, source.GetType()).Wait();
                stream.Seek(0, SeekOrigin.Begin);

                // Deserialize using System.Text.Json
                return JsonSerializer.DeserializeAsync<T>(stream).Result;
            }
        }
    }
}