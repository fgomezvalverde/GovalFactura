using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Abstraction.DependencyServices
{
    public interface IReportingService
    {
        Task<Stream> CreateAndRunReport(Dictionary<string, string> pBill, string pBillNumber);
    }
}
