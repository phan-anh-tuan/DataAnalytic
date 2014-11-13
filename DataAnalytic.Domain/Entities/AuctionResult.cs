using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DataAnalytic.Domain.Entities
{
    public class AuctionResult
    {
        [HiddenInput(DisplayValue = false)]
        public int ID { get; set; }

        [Required(ErrorMessage = "Please enter TransactionDate")]
        [DataType(DataType.Date)]
        public System.DateTime TransactionDate { get; set; }

        [Required(ErrorMessage = "Please enter Suburb")]
        public string Suburb { get; set; }

        [Required(ErrorMessage = "Please enter Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Please enter Type")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Please enter Number  of Bedrooms")]
        public int NoOfBedroom { get; set; }

        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Please enter Result")]
        public string Result { get; set; }

        [Required(ErrorMessage = "Please enter Agent")]
        public string Agent { get; set; }

        public DateTime UpdatedDate { get; set; }

        [Required(ErrorMessage = "Please enter City")]
        public string City { get; set; }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            text.Append("TransactionDate=").Append(TransactionDate.ToString("yyyy-MM-dd"));
            text.Append("; Suburb=").Append(Suburb);
            text.Append("; Address=").Append(Address);
            text.Append("; Type=").Append(Type);
            text.Append("; NoOfBedroom=").Append(NoOfBedroom);
            text.Append("; Price=").Append(String.Format("{0:c}",Price));
            text.Append("; Result=").Append(Result);
            text.Append("; Agent=").Append(Agent);
            text.Append("; City=").Append(City);
            return text.ToString();
        }
    }
}
