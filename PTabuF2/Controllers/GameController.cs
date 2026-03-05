using Microsoft.AspNetCore.Mvc;
using PTabuF2.Models;
using PTabuF2.Data;
using System.Text.Json;
using System.Data;
using System.Linq; // Karıştırma (Shuffle) için gerekli

namespace PTabuF2.Controllers
{
    public class GameController : Controller
    {
        private readonly SqlHelper _sqlHelper;

        public GameController(SqlHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        // 1. OYUNU BAŞLATMA (Ayarları ve Seçilen Desteyi Yükler)
        public IActionResult StartGame()
        {
            GameSession session;
            var settingsJson = HttpContext.Session.GetString("GlobalSettings");

            // Eğer Ayarlar sayfasından kayıt yapıldıysa o ayarları al
            if (settingsJson != null)
            {
                session = JsonSerializer.Deserialize<GameSession>(settingsJson);
                // Ayarları aldık ama oyun durumunu sıfırlayalım (Skorlar 0, Takım 1)
                session.Team1Score = 0;
                session.Team2Score = 0;
                session.CurrentTeam = 1;
                session.CardList = new List<Card>();
            }
            else
            {
                // Ayar yoksa varsayılan başlat
                session = new GameSession();
            }

            // --- DESTE SEÇİMİ ---
            string query;

            // Eğer listede en az 1 tane deste seçildiyse
            if (session.SelectedDeckIDs != null && session.SelectedDeckIDs.Count > 0)
            {
                // Seçilen ID'leri virgülle birleştir (Örn: "1,3,5")
                string idList = string.Join(",", session.SelectedDeckIDs);

                // SQL'in IN operatörünü kullanıyoruz
                query = $"SELECT * FROM Cards WHERE DeckID IN ({idList})";
            }
            else
            {
                // Hiçbir şey seçilmediyse Varsayılan (Klasik Deste)
                query = "SELECT * FROM Cards WHERE DeckID = (SELECT TOP 1 DeckID FROM Decks WHERE DeckName = 'Klasik Deste')";
            }
            // --------------------

            var dt = _sqlHelper.GetTable(query);

            // Eğer kart bulunamazsa ana sayfaya at (Hata vermesin)
            if (dt.Rows.Count == 0) return RedirectToAction("Index", "Home");

            // Kartları Listeye Doldur
            foreach (DataRow row in dt.Rows)
            {
                session.CardList.Add(new Card
                {
                    CardID = Convert.ToInt32(row["CardID"]),
                    TargetWord = row["TargetWord"].ToString(),
                    Tag = row["Tag"] != DBNull.Value ? row["Tag"].ToString() : "",
                    ForbiddenWord1 = row["ForbiddenWord1"].ToString(),
                    ForbiddenWord2 = row["ForbiddenWord2"].ToString(),
                    ForbiddenWord3 = row["ForbiddenWord3"].ToString(),
                    ForbiddenWord4 = row["ForbiddenWord4"].ToString(),
                    ForbiddenWord5 = row["ForbiddenWord5"].ToString()
                });
            }

            // Kartları Karıştır (Shuffle)
            Random rnd = new Random();
            session.CardList = session.CardList.OrderBy(x => rnd.Next()).ToList();

            // İlk Kartı Seç
            if (session.CardList.Count > 0)
            {
                session.CurrentCard = session.CardList[0];
            }

            // Oturumu Başlat ve Oyuna Git
            HttpContext.Session.SetString("GameSession", JsonSerializer.Serialize(session));
            return RedirectToAction("Play");
        }

        // 2. OYUN EKRANI (Kart Gösterimi)
        public IActionResult Play()
        {
            var sessionJson = HttpContext.Session.GetString("GameSession");
            if (sessionJson == null) return RedirectToAction("StartGame");

            var session = JsonSerializer.Deserialize<GameSession>(sessionJson);

            // Kart bittiyse veya hata varsa bitişe git
            if (session.CurrentCard == null || session.CardList.Count == 0)
            {
                return RedirectToAction("EndGame");
            }

            return View(session.CurrentCard);
        }

        // 3. HAMLE YAPMA (Doğru, Pas, Tabu)
        [HttpPost]
        public IActionResult MakeMove(string move)
        {
            var sessionJson = HttpContext.Session.GetString("GameSession");
            if (sessionJson == null) return RedirectToAction("StartGame");
            var session = JsonSerializer.Deserialize<GameSession>(sessionJson);

            // --- PUANLAMA ---
            if (move == "correct")
            {
                if (session.CurrentTeam == 1) session.Team1Score++;
                else session.Team2Score++;
            }
            else if (move == "pass")
            {
                if (session.PassRights > 0) session.PassRights--;
                else return RedirectToAction("Play"); // Pas hakkı bittiyse işlem yapma
            }
            else if (move == "taboo")
            {
                // Tabu yapılırsa puan silinir
                if (session.CurrentTeam == 1) session.Team1Score--;
                else session.Team2Score--;
            }

            // --- HEDEF SKOR KONTROLÜ (OYUN BİTTİ Mİ?) ---
            if (session.Team1Score >= session.TargetScore || session.Team2Score >= session.TargetScore)
            {
                HttpContext.Session.SetString("GameSession", JsonSerializer.Serialize(session));
                return RedirectToAction("EndGame");
            }

            // --- KARTI LİSTEDEN SİL ---
            if (session.CurrentCard != null)
            {
                var index = session.CardList.FindIndex(c => c.TargetWord == session.CurrentCard.TargetWord);
                if (index != -1)
                {
                    session.CardList.RemoveAt(index);
                }
            }

            // Kart bittiyse oyun biter
            if (session.CardList.Count == 0) return RedirectToAction("EndGame");

            // --- YENİ KART SEÇ ---
            Random rnd = new Random();
            session.CurrentCard = session.CardList[rnd.Next(session.CardList.Count)];

            // Kaydet ve Devam Et
            HttpContext.Session.SetString("GameSession", JsonSerializer.Serialize(session));
            return RedirectToAction("Play");
        }

        // 4. SÜRE BİTTİ (Ara Ekran)
        public IActionResult TurnOver()
        {
            var sessionJson = HttpContext.Session.GetString("GameSession");
            if (sessionJson == null) return RedirectToAction("Index", "Home");

            var session = JsonSerializer.Deserialize<GameSession>(sessionJson);
            return View(session);
        }

        // 5. YENİ TURU BAŞLAT (Takım Değiştir ve Süreyi Sıfırla)
        [HttpPost]
        public IActionResult StartRound()
        {
            // 1. Mevcut Oyun Oturumunu Çek
            var sessionJson = HttpContext.Session.GetString("GameSession");
            if (sessionJson == null) return RedirectToAction("Index", "Home");
            var session = JsonSerializer.Deserialize<GameSession>(sessionJson);

            // 2. Takımı Değiştir (1 -> 2 veya 2 -> 1)
            session.CurrentTeam = (session.CurrentTeam == 1) ? 2 : 1;

            // --- DÜZELTİLEN KISIM: SÜREYİ VE PAS HAKKINI AYARLARDAN ÇEK ---

            // Global Ayarları Kontrol Et
            var settingsJson = HttpContext.Session.GetString("GlobalSettings");

            if (settingsJson != null)
            {
                // Ayarlar varsa oradaki süreyi ve pas hakkını kullan (Örn: 20 sn, 5 pas)
                var settings = JsonSerializer.Deserialize<GameSession>(settingsJson);
                session.RemainingTime = settings.RemainingTime;
                session.PassRights = settings.PassRights;
            }
            else
            {
                // Ayar yoksa varsayılanları kullan
                session.RemainingTime = 60;
                session.PassRights = 3;
            }
            // -------------------------------------------------------------

            // Kaydet ve Oyna
            HttpContext.Session.SetString("GameSession", JsonSerializer.Serialize(session));
            return RedirectToAction("Play");
        }

        // 6. OYUN SONU (Kazananı Göster)
        public IActionResult EndGame()
        {
            var sessionJson = HttpContext.Session.GetString("GameSession");
            if (sessionJson == null) return RedirectToAction("Index", "Home");

            var session = JsonSerializer.Deserialize<GameSession>(sessionJson);
            return View(session);
        }
    }
}