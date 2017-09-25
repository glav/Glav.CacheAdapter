using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Glav.CacheAdapter.Serialisation
{
    public class DefaultDataContractSerialiser : IObjectSerialiser
    {
        public byte[] Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }

            var srlzr = new NetDataContractSerializer();
            using (var memoryStream = new MemoryStream())
            {
                srlzr.Serialize(memoryStream, o);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        public T Deserialize<T>(byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }

            var srlzr = new NetDataContractSerializer();
            using (MemoryStream memoryStream = new MemoryStream(stream))
            {
                var result = (T)srlzr.Deserialize(memoryStream);
                return result;
            }
        }
    }
}
