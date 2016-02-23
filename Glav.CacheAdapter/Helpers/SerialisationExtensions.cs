using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Glav.CacheAdapter.Helpers
{
    public static class SerialisationExtensions
    {

            public static byte[] Serialize(this object o)
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

            public static T Deserialize<T>(this byte[] stream)
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
