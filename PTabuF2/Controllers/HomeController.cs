using Microsoft.AspNetCore.Mvc;
using PTabuF2.Models;
using PTabuF2.Data; // SqlHelper için
using System.Text.Json;
using System.Data;

namespace PTabuF2.Controllers
{
    public class HomeController : Controller
    {
        // SqlHelper'ý tanýmlýyoruz
        private readonly SqlHelper _sqlHelper;

        public HomeController(SqlHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Settings()
        {
            // 1. Ayarlarý Getir
            var settingsJson = HttpContext.Session.GetString("GlobalSettings");
            GameSession settings = settingsJson != null
                ? JsonSerializer.Deserialize<GameSession>(settingsJson)
                : new GameSession();

            // 2. Veritabanýndaki Desteleri Çek ve View'a Gönder (ViewBag ile)
            var dt = _sqlHelper.GetTable("SELECT DeckID, DeckName FROM Decks");
            List<Deck> deckList = new List<Deck>();

            foreach (DataRow row in dt.Rows)
            {
                deckList.Add(new Deck
                {
                    DeckID = Convert.ToInt32(row["DeckID"]),
                    DeckName = row["DeckName"].ToString()
                });
            }

            // Listeyi View'a taþýyoruz
            ViewBag.DeckList = deckList;

            return View(settings);
        }

        [HttpPost]
        public IActionResult SaveSettings(GameSession settings)
        {
            HttpContext.Session.SetString("GlobalSettings", JsonSerializer.Serialize(settings));
            return RedirectToAction("Index");
        }
    }
}