﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Pages.MasterDetail
{
    public class MasterPageItem
    {
        public string Title { get; set; }

        public string IconSource { get; set; }

        public PageType TargetType { get; set; }
        

    }


    public enum PageType
    {
        BillList,ClientList,Configuration,ProductList,Logout,ValidateHaciendaUser
    }
}
