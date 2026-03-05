namespace PTabuF2.Models
{
    public class SettingsViewModel
    {
        public string Team1Name { get; set; } = "TEAM A"; // Varsayılan İsim
        public string Team2Name { get; set; } = "TEAM B"; // Varsayılan İsim
        public int TimeLimit { get; set; } = 60;   // Süre (Varsayılan 60)
        public int PassLimit { get; set; } = 3;    // Pas Hakkı (Varsayılan 3)
        public int TargetScore { get; set; } = 15; // Bitiş Puanı (Varsayılan 15)

        // Veritabanındaki tüm desteler buraya gelecek
        public List<Deck> AvailableDecks { get; set; } = new List<Deck>();

        // Kullanıcının seçtiği destelerin ID'leri burada tutulacak
        public List<int> SelectedDeckIds { get; set; } = new List<int>();
    }
}