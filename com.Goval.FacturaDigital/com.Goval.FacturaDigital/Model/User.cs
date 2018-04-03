
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Model
{

    [AddINotifyPropertyChangedInterface]
    public class User
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public int UserId { get; set; }

        public Boolean HasAdminPrivilegies { get; set; } = false;
    }
}
