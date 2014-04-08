using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaTracker {
    public class Match {

        public string Opponent { get; set; }
        public string Result { get; set; }

        public string ListView {
            get { return Result + " vs. " + Opponent; }
        }

        public Match() {
        }

        public Match(string opponent, string result) {
            this.Opponent = opponent;
            this.Result = result;
        }
    }
}
