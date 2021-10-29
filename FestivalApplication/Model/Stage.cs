using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model
{
    public class Stage
    {
        public int StageID { get; set; }
        public string StageName { get; set; }
        public Boolean StageActive { get; set; }
        public Boolean Archived { get; set; }
        public string Genre { get; set; }
        public string Restriction { get; set; }
        public List<UserActivity> Log { get; set; }
        //public List<MusicListActivity> PlayList { get; set; }
    }
   
}
