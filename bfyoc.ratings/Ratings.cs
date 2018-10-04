using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFYOC.Ratings
{
    public class Ratings
    {
        public string id { get; set; }
        public string timestamp { get; set; }
        public string userId { get; set; }
        public string productId { get; set; }
        public string locationName { get; set; }
        public string rating { get; set; }
        public string userNotes { get; set; }
    }
}
