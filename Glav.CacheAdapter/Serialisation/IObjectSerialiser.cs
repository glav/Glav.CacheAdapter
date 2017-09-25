using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glav.CacheAdapter.Serialisation
{
    public interface IObjectSerialiser
    {
        byte[] Serialize(object o);
        T Deserialize<T>(byte[] stream);
    }
}
