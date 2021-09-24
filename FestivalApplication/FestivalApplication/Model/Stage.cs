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
        public bool StageActive { get; set; }
        public List<UserActivity> Log { get; set; }
        // public List<MusicListActivity> PlayList { get; set; }
        //public Stage(Stage _stage)
        //{

        //    if (_stage.Log.Count == 0)
        //    {
        //        _stage.StageActive = false;
        //    }
        //    else
        //    {
        //        _stage.StageActive = true;
        //    }
        //}
    }
   
}
