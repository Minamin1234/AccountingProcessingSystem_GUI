using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountingProcessingSystem_GUI
{
    /// <summary>
    /// 会計データ
    /// </summary>
    public partial class ACCOUNTDATA : IEquatable<ACCOUNTDATA>
    {
        public int Id = 0;
        public DateTime? Date = null;
        public List<GROUP> Groups = null;
        public GROUP Group = null;
        public string Title = string.Empty;
        public int Paid = 0;
        public int Income = 0;

        public override string ToString()
        {
            string res = "";
            res += "Id:     " + this.Id.ToString() + Environment.NewLine;
            res += "Date:   " + this.Date.ToString() + Environment.NewLine;
            if (this.Group == null) res += "Group:  null" + Environment.NewLine;
            else res += "Group:  " + this.Group.ToString() + Environment.NewLine;
            res += "Title:  " + this.Title + Environment.NewLine;
            res += "Paid:   " + this.Paid.ToString() + Environment.NewLine;
            res += "Income: " + this.Income.ToString() + Environment.NewLine;

            return res;
        }

        public bool Equals(ACCOUNTDATA other)
        {
            return this.Id == other.Id;
        }
    }
}
