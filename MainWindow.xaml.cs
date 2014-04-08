using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;
namespace ArenaTracker {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private Arena currentArena;
        private XmlReader xmlReader;
        private List<Card> currentDeck;
        private List<string> heroList;
        private List<Card> loadedCards;

        public MainWindow() {
            InitializeComponent();

            this.heroList = new List<string>();
            this.heroList.Add("Druid");
            this.heroList.Add("Hunter");
            this.heroList.Add("Mage");
            this.heroList.Add("Paladin");
            this.heroList.Add("Priest");
            this.heroList.Add("Rogue");
            this.heroList.Add("Shaman");
            this.heroList.Add("Warlock");
            this.heroList.Add("Warrior");

            cmbHero.ItemsSource = this.heroList;
            cmbOpponentHero.ItemsSource = this.heroList;

            List<string> result = new List<string>();
            result.Add("Win");
            result.Add("Loss");
            cmbResult.ItemsSource = result;

            //Load history and cards
            this.xmlReader = new XmlReader();

            //set statistics tab
            setStatisticsTab();
            
            //Try to load saved arena
            this.currentArena = this.xmlReader.LoadArena("currentArena.xml");
            if (this.currentArena == null) {
                setNewArenaTab();
            } else {
                setCurrentArenaTab();
            }
        }

        private void setCurrentArenaTab() {
            tbStatus.Text = "Loaded saved arena run.";
            tabControl.SelectedIndex = 0;
            tabNewArena.IsEnabled = false;

            this.currentArena.deck = this.currentArena.deck.OrderBy(c=>c.Name).ToList();

            lbGames.ItemsSource = null;
            lbGames.ItemsSource = this.currentArena.matches;

            this.currentDeck = this.currentArena.deck;

            lbDeck.ItemsSource = null;
            lbDeck.ItemsSource = this.currentDeck;

            lblHero.Content = "Hero: " + this.currentArena.hero;
        }

        private void setNewArenaTab() {
            tbStatus.Text = "Can't load saved arena. Creating new arena run.";
            this.currentArena = new Arena();

            tabCurrentArena.IsEnabled = false;
            tabNewArena.IsEnabled = true;
            tabControl.SelectedIndex = 1;

            txtCard.IsEnabled = false;
            btnAddCard.IsEnabled = false;
            btnSaveToCurrentArena.IsEnabled = false;
            btnRemoveLast.IsEnabled = false;

            lbCards.ItemsSource = null;
        }

        private void setStatisticsTab() {
            this.xmlReader.arenaHistory = new List<Arena>();
            this.xmlReader.LoadHistory();
            Dictionary<string, int> arenasByHero = new Dictionary<string, int>();

            foreach (string hero in heroList) {
                arenasByHero.Add(hero, 0);
            }

            foreach (Arena arena in xmlReader.arenaHistory) {
                arenasByHero[arena.hero]++;
            }

            chartPlayedArenasByHero.ItemsSource = arenasByHero;
            lblPlayedArenas.Content = "Arenas played: " + xmlReader.arenaHistory.Count;

            Dictionary<string, int> cardQuality = new Dictionary<string, int>();
            cardQuality.Add("Free", 0);
            cardQuality.Add("Common", 0);
            cardQuality.Add("Rare", 0);
            cardQuality.Add("Epic", 0);
            cardQuality.Add("Legendary", 0);

            int cardsTotal = 0;
            foreach (Arena arena in xmlReader.arenaHistory) {
                foreach (Card card in arena.deck) {
                    cardsTotal++;
                    cardQuality[card.Quality]++;
                }
            }
            chartCardQuality.ItemsSource = cardQuality;
            lblCardsTotal.Content = "Cards total: " + cardsTotal;

            Dictionary<string, float> avgWinsByHero = new Dictionary<string, float>();

            foreach (string hero in heroList) {
                avgWinsByHero.Add(hero, 0);
            }

            //fist count all wins for each hero
            foreach (Arena arena in xmlReader.arenaHistory) {
                avgWinsByHero[arena.hero] += arena.countVictories();
            }

            //divide with total number of games to get avg
            float avg = 0;
            int heroCount = 0;
            foreach (string hero in heroList) {
                if (avgWinsByHero[hero] != 0) {
                    avgWinsByHero[hero] = avgWinsByHero[hero] / arenasByHero[hero];
                    avg += avgWinsByHero[hero];
                    heroCount++;
                }
            }
            lblAvgWins.Content = "Average victories: " + (avg/heroCount).ToString("0.00");
            chartAvgVictories.ItemsSource = avgWinsByHero;
        }

        private void btnAddCard_Click(object sender, RoutedEventArgs e) {
            addCard();
        }

        private void txtCard_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return)
                addCard();
        }

        private void addCard() {
            //check if card name is correct
            int index = loadedCards.FindIndex(r => r.Name == txtCard.Text);
            if (index == -1) {
                tbStatus.Text = "Card " + '"' + txtCard.Text + '"' + " not found.";
                return;
            }

            //check if deck is full
            if (this.currentArena.deck.Count == 30) {
                btnSaveToCurrentArena.IsEnabled = true;
                btnAddCard.IsEnabled = false;
                tbStatus.Text = "Deck is full";
                return;
            }

            Card card = new Card(txtCard.Text, loadedCards[index].Quality, loadedCards[index].Cost);
            this.currentArena.deck.Add(card);

            lbCards.ItemsSource = null;
            lbCards.ItemsSource = this.currentArena.deck;

            //check if deck is full
            if (this.currentArena.deck.Count == 30) {
                btnSaveToCurrentArena.IsEnabled = true;
                btnAddCard.IsEnabled = false;
            }

            lblNumberOfCards.Content = this.currentArena.deck.Count + "/30";
            tbStatus.Text = "Card " + '"' + txtCard.Text + '"' + " added.";
            txtCard.Text = "";
            txtCard.Focus();

            btnRemoveLast.IsEnabled = true;
            cmbHero.IsEnabled = false;
        }

        private void cmbHero_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            this.currentArena.hero = cmbHero.SelectedValue.ToString();

            //enable ui
            btnAddCard.IsEnabled = true;
            txtCard.IsEnabled = true;

            //add cards for chosen hero
            loadedCards = this.xmlReader.loadedCards[this.currentArena.hero];
            loadedCards.AddRange(this.xmlReader.loadedCards["Neutral"]);
            txtCard.ItemsSource = null;
            txtCard.ItemsSource = loadedCards;

        }

        private void btnSaveToCurrentArena_Click(object sender, RoutedEventArgs e) {
            this.xmlReader.SaveArena(this.currentArena, "currentArena.xml");

            tabNewArena.IsEnabled = false;
            tabCurrentArena.IsEnabled = true;
            tabControl.SelectedIndex = 0;

            setCurrentArenaTab();
        }

        private void btnAddGameResult_Click(object sender, RoutedEventArgs e) {
            if (currentArena.countDefeats() >= 3) {
                tbStatus.Text = "Maximum amount of defeats.";
                return;
            } else if (currentArena.countVictories() >= 12) {
                tbStatus.Text = "Maximum amount of victories.";
                return;
            }

            if (cmbOpponentHero.SelectedIndex == -1 || cmbResult.SelectedIndex == -1) {
                tbStatus.Text = "Can't add game result.";
            } else {
                Match match = new Match();
                match.Opponent = cmbOpponentHero.SelectedValue.ToString();
                match.Result = cmbResult.SelectedValue.ToString();
                this.currentArena.matches.Add(match);
                lbGames.ItemsSource = null;
                lbGames.ItemsSource = this.currentArena.matches;
                this.xmlReader.SaveArena(this.currentArena, "currentArena.xml");
            }

        }

        private void btnEndArena_Click(object sender, RoutedEventArgs e) {
            File.Delete("currentArena.xml");

            string date = DateTime.Now.ToString("dd-MM-yy-HH-mm-ss");
            string fileName = "playedArenas/" + this.currentArena.hero + "-" + date + ".xml";
            tbStatus.Text = "Saved to file: " + fileName;
            this.xmlReader.SaveArena(this.currentArena, fileName);
            setStatisticsTab();
            setNewArenaTab();
        }

        private void btnRemoveLast_Click(object sender, RoutedEventArgs e) {

            if (this.currentArena.deck.Count > 0) {
                this.currentArena.deck.RemoveAt(this.currentArena.deck.Count - 1);
                lbCards.ItemsSource = null;
                lbCards.ItemsSource = this.currentArena.deck;
                lblNumberOfCards.Content = this.currentArena.deck.Count + "/30";
                btnAddCard.IsEnabled = true;
                btnSaveToCurrentArena.IsEnabled = false;

                if (this.currentArena.deck.Count == 0) {
                    cmbHero.IsEnabled = true;
                    btnRemoveLast.IsEnabled = false;
                }
            }


        }

        private void btnRemoveResult_Click(object sender, RoutedEventArgs e) {
            if (this.currentArena.matches.Count > 0) {
                this.currentArena.matches.RemoveAt(this.currentArena.matches.Count - 1);
                lbGames.ItemsSource = null;
                lbGames.ItemsSource = this.currentArena.matches;
                tbStatus.Text = "Removed last result.";
            } else {
                tbStatus.Text = "Can't remove result.";
            }
        }
    }
}
