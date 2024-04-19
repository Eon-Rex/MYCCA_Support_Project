using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PVCCCAF.Models
{
    public class RoleMenuViewModel
    {
    }
    public class Role
    {
        public string rolecode { get; set; }

        public Role(string _rolecode)
        {
            rolecode = _rolecode;
        }
    }
    public class Menu
    {
        public string name { get; set; }

        public Menu(string _name)
        {
            name = _name;
        }
    }
    public class User
    {
        public string name { get; set; }

        public User(string _name)
        {
            name = _name;
        }
    }
}