using com.Goval.FacturaDigital.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.DependencyServices
{
    public interface IReportingService
    {
        void RunReport(Bill pBill);
    }
}
