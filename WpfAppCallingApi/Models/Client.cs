using System;
using System.Collections.Generic;
using System.Text;

namespace WpfAppCallingApi.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Cpf{ get; set; }
        public int Type { get; set; }
        public int Gender { get; set; }
        public int Situation { get; set; }
    }
}
