using Microsoft.AspNetCore.Mvc;
using PTabuF2.Data;
using PTabuF2.Models;
using System.Data;

namespace PTabuF2.Controllers
{
    public class DecksController : Controller
    {
        private readonly SqlHelper _sqlHelper;

        public DecksController(SqlHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        // 1. DESTELERİ LİSTELE
        public IActionResult Index()
        {
            var dt = _sqlHelper.GetTable("SELECT * FROM Decks");
            List<Deck> decks = new List<Deck>();

            foreach (DataRow row in dt.Rows)
            {
                decks.Add(new Deck
                {
                    DeckID = Convert.ToInt32(row["DeckID"]),
                    DeckName = row["DeckName"].ToString(),
                    IsDefault = Convert.ToBoolean(row["IsDefault"])
                });
            }
            return View(decks);
        }

        // 2. YENİ DESTE OLUŞTUR
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(string deckName)
        {
            if (!string.IsNullOrEmpty(deckName))
            {
                string query = $"INSERT INTO Decks (DeckName, IsDefault) VALUES ('{deckName}', 0)";
                _sqlHelper.ExecuteQuery(query);
            }
            return RedirectToAction("Index");
        }

        // 3. DESTE SİL
        public IActionResult Delete(int id)
        {
            _sqlHelper.ExecuteQuery($"DELETE FROM Cards WHERE DeckID = {id}");
            _sqlHelper.ExecuteQuery($"DELETE FROM Decks WHERE DeckID = {id}");
            return RedirectToAction("Index");
        }

        // 4. DESTE DETAYLARI
        public IActionResult Details(int id)
        {
            var deckDt = _sqlHelper.GetTable($"SELECT DeckName FROM Decks WHERE DeckID = {id}");
            if (deckDt.Rows.Count == 0) return RedirectToAction("Index");

            ViewBag.DeckName = deckDt.Rows[0]["DeckName"].ToString();
            ViewBag.DeckID = id;

            var cardDt = _sqlHelper.GetTable($"SELECT * FROM Cards WHERE DeckID = {id}");
            List<Card> cards = new List<Card>();

            foreach (DataRow row in cardDt.Rows)
            {
                cards.Add(new Card
                {
                    CardID = Convert.ToInt32(row["CardID"]),
                    DeckID = Convert.ToInt32(row["DeckID"]),
                    TargetWord = row["TargetWord"].ToString(),
                    Tag = row["Tag"] != DBNull.Value ? row["Tag"].ToString() : "", // Tag Eklendi
                    ForbiddenWord1 = row["ForbiddenWord1"].ToString(),
                    ForbiddenWord2 = row["ForbiddenWord2"].ToString(),
                    ForbiddenWord3 = row["ForbiddenWord3"].ToString(),
                    ForbiddenWord4 = row["ForbiddenWord4"].ToString(),
                    ForbiddenWord5 = row["ForbiddenWord5"].ToString()
                });
            }

            return View(cards);
        }

        // 5. KART EKLEME SAYFASI (GET)
        public IActionResult AddCard(int id)
        {
            ViewBag.DeckID = id;
            return View();
        }

        // 6. KART KAYDETME İŞLEMİ (POST) -> İSMİ GÜNCELLENDİ: SaveCard
        [HttpPost]
        public IActionResult SaveCard(Card card)
        {
            string tagValue = string.IsNullOrEmpty(card.Tag) ? "NULL" : $"'{card.Tag}'";

            string query = $@"INSERT INTO Cards 
                            (DeckID, TargetWord, Tag, ForbiddenWord1, ForbiddenWord2, ForbiddenWord3, ForbiddenWord4, ForbiddenWord5) 
                            VALUES 
                            ({card.DeckID}, '{card.TargetWord}', {tagValue}, '{card.ForbiddenWord1}', '{card.ForbiddenWord2}', '{card.ForbiddenWord3}', '{card.ForbiddenWord4}', '{card.ForbiddenWord5}')";

            _sqlHelper.ExecuteQuery(query);

            return RedirectToAction("Details", new { id = card.DeckID });
        }

        // 7. KART SİLME
        public IActionResult DeleteCard(int id)
        {
            var dt = _sqlHelper.GetTable($"SELECT DeckID FROM Cards WHERE CardID = {id}");
            int deckId = 0;
            if (dt.Rows.Count > 0) deckId = Convert.ToInt32(dt.Rows[0]["DeckID"]);

            _sqlHelper.ExecuteQuery($"DELETE FROM Cards WHERE CardID = {id}");
            return RedirectToAction("Details", new { id = deckId });
        }
    }
}