using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toll.Web.UI.Entity
{
    public class AppSetting
    {
        [Key]
        public Guid Id { get; set; }
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }

    }
}
