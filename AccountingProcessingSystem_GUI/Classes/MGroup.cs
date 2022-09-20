using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountingProcessingSystem_GUI
{
    /// <summary>
    /// 品目のグループ
    /// </summary>
    public partial class GROUP : IEquatable<GROUP>
    {
        public string Name = string.Empty;
        public override string ToString()
        {
            return this.Name;
        }

        //同型のオブジェクトについて、何を持って等価であるかを定義
        public bool Equals(GROUP other)
        {
            return this.Name == other.Name;
        }

        public GROUP()
        {

        }

        public GROUP(string name)
        {
            this.Name = name;
        }
    }
}
