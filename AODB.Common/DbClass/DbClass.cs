using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AODB.Common.DbClasses
{

    public abstract class DbClass
    {
        public List<object> Members { get; set; }

        public DbClass()
        {
            this.Members = new List<object>();
        }

        public string[] GetNames()
        {
            List<string> names = new List<string>();
            names.AddRange(Members.Select(x => x.GetType().Name)); //ClassNames
            names.AddRange(Members.SelectMany(x => x.GetType().GetProperties().Select(y => y.Name))); //PropertyNames
            names.Add("__class_id__");
            names.Add("obj");
            return names.Distinct().ToArray();
        }

        public List<T> GetMembers<T>()
        {
            return this.Members.OfType<T>().Cast<T>().ToList();
        }
    }
}
