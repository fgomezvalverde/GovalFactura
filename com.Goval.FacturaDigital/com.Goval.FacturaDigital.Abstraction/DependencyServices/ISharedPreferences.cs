using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Abstraction.DependencyServices
{
    public interface ISharedPreferences
    {
        String GetString(string pKey);
        Boolean Contains(string pKey);
        void SaveString(String pKey, String pValue);
        void RemoveString(String pkey);
    }
}
