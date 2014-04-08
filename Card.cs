using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaTracker {

    public class Card {

        public string Name { get; set; }
        public string Quality { get; set; }
        public int Cost { get; set; }

        public string ListView {
            get { return Name + ", " + Quality + ", " + Cost; }
        }

        public Card(string name, string quality, int cost) {
            this.Name = name;
            this.Quality = quality;
            this.Cost = cost;
        }

        public Card() {
        }

        public string toString() {
            return this.Name;
        }

    }
}
