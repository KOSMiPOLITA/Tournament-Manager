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
using System.Net;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;

namespace PAI_141249.Controllers
{
    public class HomeController : Controller
    {

        TournamentDatabaseEntities db = new TournamentDatabaseEntities();
        SignUpDatabaseEntities db2 = new SignUpDatabaseEntities();
        UserDatabaseEntities db3 = new UserDatabaseEntities();
        DrabinkaDatabaseEntities db4 = new DrabinkaDatabaseEntities();
        WynikiDatabaseEntities db5 = new WynikiDatabaseEntities();
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
            Session["id_turnieju"] = turniej.IdTurnieju;
            Session["limit"] = turniej.Limit;
            return View(turniej);
        }

        public ActionResult NewTournament()
        {
            return View();
        }

        public ActionResult SingUp()
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "User");
            }
            return View();
        }

        public ActionResult EditTournament(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tournament turniej = db.Tournaments.Where(a => a.IdTurnieju == id).FirstOrDefault();

            if (turniej == null)
            {
                return HttpNotFound();
            }

            return View(turniej);
        }

        [HttpPost, ActionName("EditTournament")]
        [ValidateAntiForgeryToken]
        public ActionResult EditTournamentPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var turniejToUpdate = db.Tournaments.Find(id);

            if (TryUpdateModel(turniejToUpdate, "", new string[] {"Nazwa","Dyscyplina","Data","MapaGoogleX", "MapaGoogleY", "Limit", "Deadline", "Rozstawieni", "Logos"}))
            {
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Nie można zapisać!");
                }
            }
            return View(turniejToUpdate);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SingUp(int? id, [Bind(Include = "Licencja,Ranking")] SingUp rej)
        {
            string Message = "";

            int id_turnieju = (int)Session["id_turnieju"];
            User us = (User)Session["user"];
            string em = us.Email.ToString();
            User uzytkownik = db3.Users.Where(a => a.Email == em).FirstOrDefault();
            int id_uzytkownika = uzytkownik.UserId;

            int ilu = db2.SingUps.Where(a => a.IdTurnieju == id_turnieju).Count();
            Tournament limit = db.Tournaments.Where(a => a.IdTurnieju == id_turnieju).FirstOrDefault();
            Debug.WriteLine(ilu);
            Debug.WriteLine(limit.Limit);

            if (ModelState.IsValid)
            {                
                if (ilu < limit.Limit)
                {
                    if (DateTime.Now < limit.Deadline)
                    {
                        if (db2.SingUps.Where(a => a.IdTurnieju == id_turnieju && a.UserId == id_uzytkownika).FirstOrDefault() == null)
                        {
                            rej.IdTurnieju = id_turnieju;
                            rej.UserId = id_uzytkownika;
                            db2.SingUps.Add(rej);
                            db2.SaveChanges();
                            us = null;
                            uzytkownik = null;
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            Message = "Aktualnie zalogowany użytkownik już zarejestrował się w tym turnieju";
                        }
                    }
                    else
                    {
                        Message = "Minął deadline turnieju";
                    }
                }

                else
                {
                    Message = "Dany turniej ma już maksymalną liczbę uczestników";
                }
                
            }

            ViewBag.Message = Message;
            return View(rej);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewTournament([Bind(Include = "Nazwa,Data,MapaGoogleX," +
            "MapaGoogleY,Limit,Deadline,Rozstawieni,Logos")] Tournament turniej)
        {
            Debug.WriteLine("Turniej");
            User user = (User)Session["user"];

            Debug.WriteLine(ModelState.IsValid);
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (ModelState modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    Debug.WriteLine(error);
                }
            }
            var allErrors = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
            foreach (var i in allErrors)
            {
                Debug.WriteLine(i);
            }

            if (user != null && ModelState.IsValid)
            {
                turniej.Dyscyplina = "Tenis";
                turniej.Czas = 1;
                turniej.Organizator = user.Email.ToString();
                Debug.WriteLine("Wewnątrz IFA");
                Debug.WriteLine(turniej.Nazwa, turniej.Data, turniej.MapaGoogleX, turniej.MapaGoogleY, turniej.Limit, turniej.Deadline, turniej.Rozstawieni, turniej.Logos, turniej.Dyscyplina, turniej.Czas, turniej.Organizator);

                db.Tournaments.Add(turniej);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            Debug.WriteLine(turniej.Nazwa);
            Debug.WriteLine(turniej.Data);
            Debug.WriteLine(turniej.MapaGoogleX);
            Debug.WriteLine(turniej.MapaGoogleY);
            Debug.WriteLine(turniej.Limit);
            Debug.WriteLine(turniej.Deadline);
            Debug.WriteLine(turniej.Rozstawieni);
            Debug.WriteLine(turniej.Logos);
            Debug.WriteLine(turniej.Dyscyplina);
            Debug.WriteLine(turniej.Czas);
            Debug.WriteLine(turniej.Organizator);

            Debug.WriteLine(ModelState.IsValid);
            Debug.WriteLine("Nie dodaje turnieju");
            return View(turniej);
        }

        public ActionResult UserTournaments(string search, int? pageNumber)
        {
            if (Session["user"] != null)
            {
                User us = (User)Session["user"];
                int userId = us.UserId;
                List<SingUp> zapisany = db2.SingUps.Where(a => a.UserId == userId).ToList();
                List<WynikiTurniejow> listaDlaUzytkownika = new List<WynikiTurniejow>();
                List<Tournament> turnieje = new List<Tournament>();
                //List<int> id_tur = new List<int>();
                //List<Tournament> turnieje = new List<Tournament>();

                if (zapisany != null)
                {
                    foreach (var z in zapisany)
                    {
                        var lista = db5.WynikiTurniejows.Where(a => a.IdTurnieju == z.IdTurnieju && a.IdUser == userId && a.Wygrany != null && a.Runda > 0)
                                                        .OrderByDescending(a => a.Runda)
                                                        .FirstOrDefault();
                        if (lista != null )
                        {
                            lista.Nazwa = db.Tournaments.Where(a => a.IdTurnieju == z.IdTurnieju).FirstOrDefault().Nazwa;
                            Debug.WriteLine(lista.Nazwa);
                            listaDlaUzytkownika.Add(lista);
                            turnieje.Add(db.Tournaments.Where(a => a.IdTurnieju == z.IdTurnieju).FirstOrDefault());
                        }
                        else
                        {
                            WynikiTurniejow x = new WynikiTurniejow();
                            x.Nazwa = db.Tournaments.Where(a => a.IdTurnieju == z.IdTurnieju).FirstOrDefault().Nazwa;
                            x.Runda = 0;
                            x.IdTurnieju = z.IdTurnieju;
                            listaDlaUzytkownika.Add(x);
                            turnieje.Add(db.Tournaments.Where(a => a.IdTurnieju == z.IdTurnieju).FirstOrDefault());
                        }


                    }
                }


                if (listaDlaUzytkownika != null)
                {
                    ViewBag.Turnieje = turnieje;
                    return View(listaDlaUzytkownika);
                }

                /*
                    if (listaDlaUzytkownika != null)
                {
                    foreach (var i in listaDlaUzytkownika) id_tur.Add(i.IdTurnieju);
                    foreach (var t in id_tur)
                    {
                        var v = db.Tournaments.Where(a => a.IdTurnieju == t).FirstOrDefault();
                        if (v != null)
                        {
                            turnieje.Add(v);
                        }
                    }
                    if (turnieje != null)
                    {
                        return View(turnieje.ToPagedList(pageNumber ?? 1, 5));
                    }
                }*/
            }

            return View();
            
        }

        public ActionResult DrawTournamentBracket()
        {
            if (Session["id_turnieju"] != null)
            { 
                Random rng = new Random();
                int id = (int)Session["id_turnieju"];
                DrabinkaTurniejow drabinka = new DrabinkaTurniejow();
                Debug.WriteLine(db4.DrabinkaTurniejows.Where(a => a.TurniejId == id));

                if (db4.DrabinkaTurniejows.Where(a => a.TurniejId == id).FirstOrDefault() == null)
                {
                    Tournament turniej = db.Tournaments.Where(a => a.IdTurnieju == id).FirstOrDefault();
                    List<SingUp> zapisy = db2.SingUps.Where(a => a.IdTurnieju == id).OrderBy(a => a.Ranking).ToList();
                    List<User> usersRozstawieniId = new List<User>();
                    List<User> usersId = new List<User>();
                    List<User> user_ost = new List<User>();
                    int roz = 1;

                    int krok_rozs = 0;
                    if (turniej.Rozstawieni != 0)
                    {
                        krok_rozs = (int)(turniej.Limit / turniej.Rozstawieni);
                    }

                    for (int i = 0; i < turniej.Limit; i++)
                    {
                        #region Znajdź wsyzstkie Id
                        if (i < zapisy.Count)
                        {
                            Debug.WriteLine(zapisy[i].UserId);
                            int o = zapisy[i].UserId;
                            User u = db3.Users.Where(a => a.UserId == o).FirstOrDefault();
                            if (usersRozstawieniId.Count < turniej.Rozstawieni) usersRozstawieniId.Add(u);
                            else usersId.Add(u);
                        }
                        else
                        {   
                            if (usersRozstawieniId.Count < turniej.Rozstawieni)
                            {
                                #region Fake User
                                User u = new User();
                                u.UserId = 1000 + i;
                                u.Imie = "Imie" + i;
                                u.Nazwisko = "Nazwisko" + i;
                                u.Email = "ROZSTAWIONY" + roz;
                                roz++;
                                u.PotwierdzonyEmail = true;
                                u.Haslo = "********";
                                u.KodAktywacyjny = new System.Guid();
                                u.KodZresetowanegoHasla = "";
                                usersRozstawieniId.Add(u);
                                #endregion
                            }
                            else
                            {
                                #region Fake User
                                User u = new User();
                                u.UserId = 1000 + i;
                                u.Imie = "Imie" + i;
                                u.Nazwisko = "Nazwisko" + i;
                                u.Email = "ZAWODNIK" + i;
                                u.PotwierdzonyEmail = true;
                                u.Haslo = "********";
                                u.KodAktywacyjny = new System.Guid();
                                u.KodZresetowanegoHasla = "";
                                usersId.Add(u);
                                #endregion
                            }

                        }
                        #endregion
                    }
                    usersId = usersId.OrderBy(a => rng.Next()).ToList();

                    int e = 0, f = 0;
                    for (int i = 0; i < turniej.Limit; i++)
                    {
                        if (krok_rozs != 0)
                        {
                            if ((i % krok_rozs == 0) && (e < usersRozstawieniId.Count))
                            {
                                user_ost.Add(usersRozstawieniId[returnIndexOfRozstawieni(usersRozstawieniId, e)]);
                                e++;
                            }

                            else
                            {
                                user_ost.Add(usersId[f]);
                                f++;
                            }
                        }

                    }

                    string lista_uczestnikow = "";
                    foreach (var u in user_ost)
                    {
                        string user = (string)u.Email;
                        lista_uczestnikow = lista_uczestnikow + user + ";";
                    }

                    drabinka.TurniejId = id;
                    drabinka.Lista = lista_uczestnikow;
                    db4.DrabinkaTurniejows.Add(drabinka);
                    db4.SaveChanges();
                    Debug.WriteLine(lista_uczestnikow);
                }

                else
                {
                    drabinka = db4.DrabinkaTurniejows.Where(a => a.TurniejId == id).FirstOrDefault();
                    Debug.WriteLine(drabinka);
                }

                if (drabinka != null)
                    return View(drabinka);
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult FindTournamentBracket()
        {
            if (Session["id_turnieju"] != null)
            {
                Random rng = new Random();
                int id = (int)Session["id_turnieju"];
                WynikiTurniejow wynik = new WynikiTurniejow();
                List<WynikiTurniejow> wyniki_ostateczne = new List<WynikiTurniejow>();

                if (db5.WynikiTurniejows.Where(a => a.IdTurnieju == id).FirstOrDefault() == null)
                {
                    Tournament turniej = db.Tournaments.Where(a => a.IdTurnieju == id).FirstOrDefault();
                    List<SingUp> zapisy = db2.SingUps.Where(a => a.IdTurnieju == id).OrderBy(a => a.Ranking).ToList();
                    List<User> usersRozstawieniId = new List<User>();
                    List<User> usersId = new List<User>();
                    List<User> user_ost = new List<User>();

                    int roz = 1;

                    int krok_rozs = 0;
                    if (turniej.Rozstawieni != 0)
                    {
                        krok_rozs = (int)(turniej.Limit / turniej.Rozstawieni);
                    }

                    for (int i = 0; i < turniej.Limit; i++)
                    {
                        #region Znajdź wsyzstkie Id
                        if (i < zapisy.Count)
                        {
                            Debug.WriteLine(zapisy[i].UserId);
                            int o = zapisy[i].UserId;
                            User u = db3.Users.Where(a => a.UserId == o).FirstOrDefault();
                            if (usersRozstawieniId.Count < turniej.Rozstawieni) usersRozstawieniId.Add(u);
                            else usersId.Add(u);
                        }
                        else
                        {
                            if (usersRozstawieniId.Count < turniej.Rozstawieni)
                            {
                                #region Fake User
                                User u = new User();
                                u.UserId = 1000 + i;
                                u.Imie = "Imie" + i;
                                u.Nazwisko = "Nazwisko" + i;
                                u.Email = "ROZSTAWIONY" + roz;
                                roz++;
                                u.PotwierdzonyEmail = true;
                                u.Haslo = "********";
                                u.KodAktywacyjny = new System.Guid();
                                u.KodZresetowanegoHasla = "";
                                usersRozstawieniId.Add(u);
                                #endregion
                            }
                            else
                            {
                                #region Fake User
                                User u = new User();
                                u.UserId = 1000 + i;
                                u.Imie = "Imie" + i;
                                u.Nazwisko = "Nazwisko" + i;
                                u.Email = "ZAWODNIK" + i;
                                u.PotwierdzonyEmail = true;
                                u.Haslo = "********";
                                u.KodAktywacyjny = new System.Guid();
                                u.KodZresetowanegoHasla = "";
                                usersId.Add(u);
                                #endregion
                            }

                        }
                        #endregion
                    }
                    usersId = usersId.OrderBy(a => rng.Next()).ToList();

                    int e = 0, f = 0;
                    for (int i = 0; i < turniej.Limit; i++)
                    {
                        if (krok_rozs != 0)
                        {
                            if ((i % krok_rozs == 0) && (e < usersRozstawieniId.Count))
                            {
                                user_ost.Add(usersRozstawieniId[returnIndexOfRozstawieni(usersRozstawieniId, e)]);
                                e++;
                            }

                            else
                            {
                                user_ost.Add(usersId[f]);
                                f++;
                            }
                        }

                    }

                    for (int i = 0; i < user_ost.Count; i++)
                    {
                        int id_p = 0;
                        if (i % 2 == 0) id_p = user_ost[i + 1].UserId;
                        else id_p  = user_ost[i - 1].UserId;

                        wyniki_ostateczne.Add(new WynikiTurniejow()
                        {   IdTurnieju = id,
                            IdUser = user_ost[i].UserId,
                            IdPrzeciwnik = id_p,
                            Runda = 1,
                            Para = (int)(i / 2),
                            Wygrany = -1,
                            Nazwa = user_ost[i].Email
                        });

                        db5.WynikiTurniejows.Add(new WynikiTurniejow()
                        {
                            IdTurnieju = id,
                            IdUser = user_ost[i].UserId,
                            IdPrzeciwnik = id_p,
                            Runda = 1,
                            Para = (int)(i / 2),
                            Wygrany = -1,
                            Nazwa = user_ost[i].Email
                        });
                        db5.SaveChanges();
                    }
                }
            
                if (db5.WynikiTurniejows.Where(a => a.IdTurnieju == id) != null)
                {
                    return View(db5.WynikiTurniejows.Where(a => a.IdTurnieju == id).OrderBy(a => a.Runda).ThenBy(a=>a.Para).ThenBy(a=>a.IdUser).ToList());
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult AddResult()
        {
            int id_turnieju = 0;
            Session["msg"] = null;

            if (Session["id_turnieju"] == null || Session["user"] == null)
            {
                Session["msg"] = "Niezalogowany użytkownik nie może wprowadzić wyniku";
                Debug.WriteLine(1);
                id_turnieju = Convert.ToInt32(Session["id_turnieju"]);
                return RedirectToAction("FindTournamentBracket", "Home", new { id = id_turnieju });
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = (User)Session["user"];       

            int id_uczestnika = user.UserId;

            id_turnieju = Convert.ToInt32(Session["id_turnieju"]);
            var w = db5.WynikiTurniejows.Where(a => a.IdTurnieju == id_turnieju).Where(a=>a.IdUser == id_uczestnika).OrderByDescending(a => a.Runda).FirstOrDefault();
            var z = db.Tournaments.Where(a => a.IdTurnieju == id_turnieju).FirstOrDefault();
            int liczbaRund = Convert.ToInt32(Math.Log(z.Limit, 2) + 1);
            if (w == null)
            {
                Session["msg"] = "Użytkownik nie bierze udziału w tym turnieju";
                return RedirectToAction("FindTournamentBracket", "Home", new { id = id_turnieju });
            }
            var przec = db5.WynikiTurniejows.Where(a => a.IdTurnieju == id_turnieju && a.Para == w.Para && a.IdUser != id_uczestnika && a.Runda == w.Runda).OrderByDescending(a => a.Runda).FirstOrDefault();
            if (przec != null)
            {
                Session["przeciwnik"] = przec.IdUser;
                Debug.WriteLine(Session["przeciwnik"].ToString());
            }
            else
            {
                if (w.Runda == liczbaRund)
                {
                    Session["msg"] = "Nie można dodać wyniku, gdyż turniej ma już zwycięzce!";
                    return RedirectToAction("FindTournamentBracket", "Home", new { id = id_turnieju });
                }
                Session["msg"] = "Użytkownik nie ma jeszcze pewnego przeciwnika";
                return RedirectToAction("FindTournamentBracket", "Home", new { id = id_turnieju });
            }

            if (w == null)
            {
                Debug.WriteLine(2);
                Session["msg"] = "Nie bierzech udziału w tym turnieju!";
                return RedirectToAction("FindTournamentBracket", "Home", new { id = id_turnieju});
            }
            if (w.Wygrany == null)
            {
                Session["msg"] = "Użytkownik już przegrał!";
                return RedirectToAction("FindTournamentBracket", "Home", new { id = id_turnieju });
            }

            if (w.Wygrany != -1)
            {
                Session["msg"] = "Użytkownik już podał wynik!";
                return RedirectToAction("FindTournamentBracket", "Home", new { id = id_turnieju });
            }

            return View(w);
        }

        [HttpPost, ActionName("AddResult")]
        [ValidateAntiForgeryToken]
        public ActionResult AddResultPost()
        {
            if (Session["id_turnieju"] == null || Session["user"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = (User)Session["user"];

            int id_uczestnika = user.UserId;

            int id_turnieju = (int)Session["id_turnieju"];

            var tur = db.Tournaments.Where(a => a.IdTurnieju == id_turnieju).FirstOrDefault();
            double limit = Convert.ToDouble(tur.Limit);

            var w = db5.WynikiTurniejows.Where(a => a.IdTurnieju == id_turnieju && a.IdUser == id_uczestnika).OrderByDescending(a => a.Runda).FirstOrDefault();
            var przec = db5.WynikiTurniejows.Where(a => a.IdTurnieju == id_turnieju && a.Para == w.Para && a.IdUser != id_uczestnika && a.Runda == w.Runda).OrderByDescending(a => a.Runda).FirstOrDefault();
            Debug.WriteLine(przec.IdUser);
            if (w.Wygrany != null)
            {
                if (TryUpdateModel(w, "", new string[] { "Wygrany" }))
                {
                    try
                    {
                        db5.SaveChanges();
                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        Debug.WriteLine(4);
                        ModelState.AddModelError("", "Nie można zapisać!");
                    }
                }
            }

            else
            {
                return RedirectToAction("FindTournamentBracket", "Home", new { id = id_turnieju });
            }

            if (w != null && przec != null)
            {
                if (w.Wygrany != w.IdUser && w.Wygrany != przec.IdUser)
                {
                    Session["msg"] = "Użytkownik podał wartość różną od id, którejkolwiek osoby z pary!";
                    db5.Database.ExecuteSqlCommand("UPDATE WynikiTurniejow SET Wygrany = -1 WHERE IdUser = {0}", w.IdUser);
                    return RedirectToAction("FindTournamentBracket", "Home", new { id = id_turnieju });
                }

                if (przec.Wygrany != -1 && przec.Wygrany != null && (w.Runda == przec.Runda))
                {
                    if (w.Wygrany != przec.Wygrany)
                    {
                        db5.Database.ExecuteSqlCommand("UPDATE WynikiTurniejow SET Wygrany = -1 WHERE IdUser = {0}", w.IdUser);
                        db5.Database.ExecuteSqlCommand("UPDATE WynikiTurniejow SET Wygrany = -1 WHERE IdUser = {0}", przec.IdUser);
                        db5.SaveChanges();
                        return RedirectToAction("FindTournamentBracket", "Home", new { id = id_turnieju });
                    }
                    else if (w.Wygrany == przec.Wygrany && w.Wygrany == w.IdUser)
                    {
                        db5.Database.ExecuteSqlCommand("UPDATE WynikiTurniejow SET Wygrany = null WHERE IdUser IN ({0})", przec.IdUser);
                        int para = (int)(w.Para / 2);
                        int runda = (int)(w.Runda) + 1;
                        if (Math.Ceiling(Math.Log(limit, 2)) < runda)
                        {
                            db5.Database.ExecuteSqlCommand("INSERT INTO WynikiTurniejow VALUES({0},{1},{2},{3},{4},{5},{6})", w.IdTurnieju, w.IdUser, null, para, null, runda, w.Nazwa);
                        }
                        else
                        {
                            db5.Database.ExecuteSqlCommand("INSERT INTO WynikiTurniejow VALUES({0},{1},{2},{3},{4},{5},{6})", w.IdTurnieju, w.IdUser, null, para, -1, runda, w.Nazwa);
                        }
                        
                        db5.SaveChanges();
                        return RedirectToAction("FindTournamentBracket", "Home", new { id = id_turnieju });
                    }
                    else if (w.Wygrany == przec.Wygrany && w.Wygrany == przec.IdUser)
                    {
                        db5.Database.ExecuteSqlCommand("UPDATE WynikiTurniejow SET Wygrany = null WHERE IdUser IN ({0})", w.IdUser);
                        int para = (int)(przec.Para / 2);
                        int runda = (int)(w.Runda) + 1;
                        if (Math.Ceiling(Math.Log(limit, 2)) < runda)
                        {
                            db5.Database.ExecuteSqlCommand("INSERT INTO WynikiTurniejow VALUES({0},{1},{2},{3},{4},{5},{6})", przec.IdTurnieju, przec.IdUser, null, para, null, runda, przec.Nazwa);
                                                    }
                        else
                        {
                            db5.Database.ExecuteSqlCommand("INSERT INTO WynikiTurniejow VALUES({0},{1},{2},{3},{4},{5},{6})", przec.IdTurnieju, przec.IdUser, null, para, -1, runda, przec.Nazwa);
                        }
                        db5.SaveChanges();
                        return RedirectToAction("FindTournamentBracket", "Home", new { id = id_turnieju });
                    }
                }

                else
                {
                    return RedirectToAction("FindTournamentBracket", "Home", new { id = id_turnieju });
                }
            }

            return View(w);
        }

        public int returnIndexOfRozstawieni(List<User> lista, int nr_iteracji)
        {
            int N = lista.Count;
            int bit = (int)Math.Log(N, 2);
            string binary = Convert.ToString(nr_iteracji, 2).PadLeft(bit, '0');
            char[] binary_rev = binary.ToCharArray();
            Array.Reverse(binary_rev);
            binary = new string(binary_rev);
            int index = 0;
            for (int i = 0; i < bit; i++)
            {
                index += (int)((N / Math.Pow(2, (i+1)))*((int)binary[i] - '0'));
            }
            Debug.WriteLine(index);
            return index;
        }
    }

    
}