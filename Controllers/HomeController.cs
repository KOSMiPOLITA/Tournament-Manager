using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PAI_141249.Models;
using PagedList;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace PAI_141249.Controllers
{
    public class HomeController : Controller
    {

        TournamentDatabaseEntities db = new TournamentDatabaseEntities();

        // GET: Home
        public ActionResult Index(string search, int? pageNumber)
        {
            return View(db.Tournaments.Where(a=>a.Nazwa.Contains(search) || search == null).ToList().ToPagedList(pageNumber ?? 1, 10));
        }

        public ActionResult Details(int id)
        {
            Tournament turniej = db.Tournaments.Where(a => a.IdTurnieju == id).FirstOrDefault();
            if (turniej == null)
            {
                return View("Not found"); 
            }
            return View(turniej);
        }

        public ActionResult NewTournament()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult NewTournament([Bind(Include = "IdTurnieju,Nazwa,Dyscyplina,Czas,MapaGoogleX," +
            "MapaGoogleY,Limit,Deadline,Rozstawieni,Organizator")] Tournament turniej)
        {
            if (ModelState.IsValid)
            {
                db.Tournaments.Add(turniej);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            return View(turniej);
        }
    }
}