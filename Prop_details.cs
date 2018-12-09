using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDP_Framework
{
   public class Prop_details
    {
        private int _pro_id;
        private string _mod_id;
        private int _modl_price;
        private string _prod_date;
        private string _mdel_date;
        private string _flg;

        public int ProdId {
            get
            {
                return _pro_id;
            }
            set
            {
                _pro_id = value;
            }
        }

        public string Model_Id
        {
            get
            {
                return _mod_id;
            }
            set
            {
                _mod_id = value;
            }
        }

        public int Model_Price
        {
            get
            {
                return _modl_price;
            }
            set
            {
                _modl_price = value;
            }
        }

        public string Prod_date
        {
            get
            {
                return _prod_date;
            }
            set
            {
                _prod_date = value;
            }
        }

        public string Model_date
        {
            get
            {
                return _mdel_date;
            }
            set
            {
                _mdel_date = value;
            }
        }

        public string Flag
        {
            get
            {
                return _flg;
            }
            set
            {
                _flg = value;
            }
        }

    }
}
