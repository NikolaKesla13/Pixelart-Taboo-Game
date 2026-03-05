using System.Collections.Generic;

namespace PTabuF2.Models
{
    public class GameSession
    {
        public string Team1Name { get; set; } = "Team A";
        public string Team2Name { get; set; } = "Team B";

        public int Team1Score { get; set; } = 0;
        public int Team2Score { get; set; } = 0;

        public int TargetScore { get; set; } = 15; // Hedef Skor
        public int CurrentTeam { get; set; } = 1; // 1 veya 2
        public int RemainingTime { get; set; } = 60; // Saniye cinsinden
        public int PassRights { get; set; } = 3; // Pas Hakkı

        public List<int> SelectedDeckIDs { get; set; } = new List<int>();
        // --- EKSİK OLAN KISIMLAR ---

        // Oynanacak kartların listesi
        public List<Card> CardList { get; set; } = new List<Card>();

        // Şu an ekranda açık olan kart (BUNU EKLEYİNCE DÜZELECEK)
        public Card CurrentCard { get; set; }
    }
}