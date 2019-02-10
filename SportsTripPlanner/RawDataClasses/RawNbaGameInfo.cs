using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsTripPlanner
{
    public class Bd
    {
        public List<object> b { get; set; }
    }

    public class V
    {
        public int tid { get; set; }
        public string re { get; set; }
        public string ta { get; set; }
        public string tn { get; set; }
        public string tc { get; set; }
        public string s { get; set; }
    }

    public class H
    {
        public int tid { get; set; }
        public string re { get; set; }
        public string ta { get; set; }
        public string tn { get; set; }
        public string tc { get; set; }
        public string s { get; set; }
    }

    public class Pl
    {
        public string pid { get; set; }
        public string fn { get; set; }
        public string ln { get; set; }
        public string val { get; set; }
        public int tid { get; set; }
        public string ta { get; set; }
        public string tn { get; set; }
        public string tc { get; set; }
    }

    public class Ptsls
    {
        public List<Pl> pl { get; set; }
    }

    public class G
    {
        public string gid { get; set; }
        public string gcode { get; set; }
        public string seri { get; set; }
        public int @is { get; set; }
        public string gdte { get; set; }
        public DateTime htm { get; set; }
        public DateTime vtm { get; set; }
        public DateTime etm { get; set; }
        public string an { get; set; }
        public string ac { get; set; }
        public string @as { get; set; }
        public string st { get; set; }
        public string stt { get; set; }
        public Bd bd { get; set; }
        public V v { get; set; }
        public H h { get; set; }
        public string gdtutc { get; set; }
        public string utctm { get; set; }
        public string ppdst { get; set; }
        public Ptsls ptsls { get; set; }
        public int seq { get; set; }
    }

    public class Mscd
    {
        public string mon { get; set; }
        public List<G> g { get; set; }
    }

    public class Lscd
    {
        public Mscd mscd { get; set; }
    }

    public class RawNbaGameInfo
    {
        public List<Lscd> lscd { get; set; }
    }
}
