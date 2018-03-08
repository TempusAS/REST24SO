using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace in24seven.Models
{
    public class LassReg
    {
        public string Dato { get; set; }
        public string KundeNr { get; set; }
        public string ProsjektNr { get; set; }
        public string Sjafor { get; set; }
        public string bilNr { get; set; }
        public string SkiftStartKl { get; set; }
        public string Merknad { get; set; }
        public string lassnr { get; set; }
        public string LasteSted { get; set; }
        public string TippetSted { get; set; }
        public string TidLastet { get; set; }
        public string TidTippet { get; set; }
        public string TypeBil { get; set; }
        public string Massetype { get; set; }
        public string Ventetid { get; set; }
        public string AntallTonn { get; set; }
        public string AntallM3 { get; set; }
        public string TippelappNr { get; set; }
        public string GodkjenteVentetid { get; set; }
        public string ProsjektSluttKl { get; set; }
        public string kommentarerOmLasset { get; set; }
    }
}