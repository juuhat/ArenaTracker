using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaTracker {
    public class Arena {
        public string hero;
        public List<Card> deck;
        public List<Match> matches;

        public Arena() {
            this.deck = new List<Card>();
            this.matches = new List<Match>();
        }

        public int countDefeats() {
            int defeats = 0;
            foreach (Match match in matches) {
                if (match.Result == "Loss") {
                    defeats++;
                }
            }
            return defeats;
        }

        public int countVictories() {
            int victories = 0;
            foreach (Match match in matches) {
                if (match.Result == "Win") {
                    victories++;
                }
            }
            return victories;
        }

    }
}
