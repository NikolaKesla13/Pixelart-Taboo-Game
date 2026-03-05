namespace PTabuF2.Models
{
    public class Card
    {
        public int CardID { get; set; }
        public int DeckID { get; set; }
        public string TargetWord { get; set; }

        public string Tag { get; set; } // Örn: "Akraba", "Spor" vs.
        // Yasaklı kelimeleri ayrı ayrı tutmak yerine liste olarak yönetmek işimizi kolaylaştırır
        public string ForbiddenWord1 { get; set; }
        public string ForbiddenWord2 { get; set; }
        public string ForbiddenWord3 { get; set; }
        public string ForbiddenWord4 { get; set; }
        public string ForbiddenWord5 { get; set; }

        // Bu özellik veritabanında yok ama oyun içinde kolaylık olsun diye ekliyoruz
        public List<string> GetForbiddenList()
        {
            return new List<string> { ForbiddenWord1, ForbiddenWord2, ForbiddenWord3, ForbiddenWord4, ForbiddenWord5 };
        }
    }
}