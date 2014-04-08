using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace ArenaTracker {
    class XmlReader {
       
        public Dictionary<string, List<Card>> loadedCards;
        public List<Arena> arenaHistory;

        public XmlReader() {
            this.loadedCards = new Dictionary<string,List<Card>>();
            this.loadedCards.Add("Neutral", loadCards("neutral"));
            this.loadedCards.Add("Druid", loadCards("druid"));
            this.loadedCards.Add("Hunter", loadCards("hunter"));
            this.loadedCards.Add("Mage", loadCards("mage"));
            this.loadedCards.Add("Paladin", loadCards("paladin"));
            this.loadedCards.Add("Priest", loadCards("priest"));
            this.loadedCards.Add("Rogue", loadCards("rogue"));
            this.loadedCards.Add("Shaman", loadCards("shaman"));
            this.loadedCards.Add("Warlock", loadCards("warlock"));
            this.loadedCards.Add("Warrior", loadCards("warrior"));
        }

        public List<Card> loadCards(string hero) {
            try {
                string filename = "cards/" + hero + ".xml";
                XmlSerializer xs = new XmlSerializer(typeof(Collection));
                FileStream read = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                Collection coll = (Collection)xs.Deserialize(read);
                return coll.Cards.ToList();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        public Arena LoadArena(string fileName) {
            XmlSerializer deserializer = new XmlSerializer(typeof(Arena));
            try {
                TextReader tr = new StreamReader(fileName);
                Arena arena = (Arena)deserializer.Deserialize(tr);
                tr.Close();
                return arena;
            } catch (Exception ex) {
                return null;
            }
        }

        public void SaveArena(Arena arena, string fileName) {
            XmlSerializer xs = new XmlSerializer(typeof(Arena));
            TextWriter tw = new StreamWriter(fileName);
            try {
                xs.Serialize(tw, arena);
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            } finally {
                tw.Close();
            }
        }

        public void LoadHistory() {
            string[] files = Directory.GetFiles("playedArenas/");
            foreach (string file in files) {
                this.arenaHistory.Add(LoadArena(file));
            }
        }

    }

    //helper class for card loading
    public class Collection {
        public ObservableCollection<Card> Cards { get; set; }
        public Collection() {
            Cards = new ObservableCollection<Card>();
        }
    }

}