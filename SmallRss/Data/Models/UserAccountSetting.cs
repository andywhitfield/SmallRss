using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallRss.Data.Models
{
    [TableName("UserAccountSetting")]
    public class UserAccountSetting
    {
        public int UserAccountId { get; set; }
        public string SettingType { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
    }
}
