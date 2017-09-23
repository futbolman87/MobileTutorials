using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTGLandBalancer
{
    public class Card
    {
        public Card() { }
        public string name { get; set; }
        public string layout { get; set; }
        public string manaCost { get; set; }
        public string cmc { get; set; }
        public IList<string> colors { get; set; }
        public string type { get; set; }
        public IList<string> types { get; set; }
        public IList<string> subtypes { get; set; }
        public string text { get; set; }
        public string power { get; set; }
        public string toughness { get; set; }
        public string imageName { get; set; }
        public IList<string> colorIdentity { get; set; }

    }
}
