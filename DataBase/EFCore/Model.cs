using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.EFCore
{
    public class CommonDb
    {
        [Key]
        public string Key {  get; set; }
        public byte[] Value {  get; set; }

        public int Version { get; set; }
    }
}
