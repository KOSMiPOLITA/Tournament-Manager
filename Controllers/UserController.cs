using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PAI_141249.Models;

namespace PAI_141249.Controllers
{
    public class UserController : Controller
    {
        // REJESTRACJA
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        // REJESTRACJA POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "PotwierdzonyEmail,KodAktywacyjny")] User user)
        {
            bool Status = false;
            string Message = "";

            #region Walidacja Modelu
            if (ModelState.IsValid)
            {
                #region Email już istnieje
                var emailExist = IsEmailExist(user.Email);
                if (emailExist)
                {
                    ModelState.AddModelError("EmailExists", "Email został już przypisany do użytkownika");
                    return View(user);
                }
                #endregion

                #region Generowanie linku aktywacyjnego
                user.KodAktywacyjny = Guid.NewGuid();
                #endregion

                #region Hasło - haszowanie
                user.Haslo = PassCryptor.Hash(user.Haslo);
                user.PotwierdzHaslo = PassCryptor.Hash(user.PotwierdzHaslo);
                #endregion

                user.PotwierdzonyEmail = false;

                #region Dodanie do DB
                using (UserDatabaseEntities db = new UserDatabaseEntities())
                {
                    db.Users.Add(user);
                    db.SaveChanges();

                    #region Wyślij link aktywacyjny
                    SendVerificationLink(user.Email, user.KodAktywacyjny.ToString(), "VerifyAccount");
                    Message = "Rejestracja zakończona powodzeniem. Link do aktywacji konta został wysłany na email: " + user.Email;
                    Status = true;
                    #endregion 

                }
                #endregion
            }
            #endregion

            else
            {
                Message = "Invalid Request";
            }

            ViewBag.Message = Message;
            ViewBag.Status = Status;
            return View(user);
        }

        // WERYFIKACJA KONTA
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (UserDatabaseEntities db = new UserDatabaseEntities())
            {
                db.Configuration.ValidateOnSaveEnabled = false; //W wypadku gdy potwierdzone hasło się nie zgadza

                var v = db.Users.Where(a => a.KodAktywacyjny == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.PotwierdzonyEmail = true;
                    db.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid request";
                }
            }
            ViewBag.Status = Status;
            return View();
        }

        // LOGOWANIE
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // LOGOWANIE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string ReturnUrl = "")
        {
            string message = "";
            using (UserDatabaseEntities db = new UserDatabaseEntities())
            {
                // Czy email znajduje się w DB
                var v = db.Users.Where(a => a.Email == login.Email).FirstOrDefault();
                if (v != null)
                {
                    Session["user"] = v;
                    if (string.Compare(PassCryptor.Hash(login.Haslo), v.Haslo) == 0)
                    {

                        #region Godzinne logowanie
                        /*
                        var ticket = new FormsAuthenticationTicket(login.Email, false, 60);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(60);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);*/
                        #endregion

                        #region Przekierowanie
                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            //return Redirect(ReturnUrl);
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        #endregion

                    }
                    else
                    {
                        message = "Podano błędne hasło";
                    }
                }
                else
                {
                    message = "Podano błędne dane";
                }
            }
            ViewBag.Message = message;
            return View();
        }

        // LOGOUT
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session["user"] = null;
            return RedirectToAction("Index", "Home");
        }

        // PRZYPOMNIENIE HASŁA
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string Email)
        {
            string message = "";
            bool status = false;

            #region Weryfikacja Emaila
            using (UserDatabaseEntities db = new UserDatabaseEntities())
            {
                var konto = db.Users.Where(a => a.Email == Email).FirstOrDefault();
                if (konto != null)
                {
                    string resetCode = Guid.NewGuid().ToString();
                    SendVerificationLink(konto.Email, resetCode, "ResetPassword");
                    konto.KodZresetowanegoHasla = resetCode;
                    db.Configuration.ValidateOnSaveEnabled = false; //uniknięcie błędu dla problemu z brakiem powtórzonego hasła
                    db.SaveChanges();
                }
                else
                {
                    message = "Nie znaleziono konta o takim adresie email";
                }
            }
            #endregion

            ViewBag.Message = message;
            ViewBag.Status = status;
            return View();
        }

        public ActionResult ResetPassword(string id)
        {
            using (UserDatabaseEntities db = new UserDatabaseEntities())
            {
                var user = db.Users.Where(a => a.KodZresetowanegoHasla == id).FirstOrDefault();
                if (user != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.ResetCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                using (UserDatabaseEntities db = new UserDatabaseEntities())
                {
                    var user = db.Users.Where(a => a.KodZresetowanegoHasla == model.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        user.Haslo = PassCryptor.Hash(model.NewPassword);
                        user.KodZresetowanegoHasla = "";
                        db.Configuration.ValidateOnSaveEnabled = false;
                        db.SaveChanges();
                        message = "Poprawnie ustawiono nowe hasło!";
                    }
                }
            }
            else
            {
                message = "Błędne dane";
            }

            ViewBag.Message = message;
            return View(model);
        }

        /*
         * DODATKOWE
         */

        [NonAction]
        public bool IsEmailExist(string email)
        {
            using (UserDatabaseEntities db = new UserDatabaseEntities())
            {
                var v = db.Users.Where(a => a.Email == email).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void SendVerificationLink(string email, string activationCode, string emailFor = "")
        {
            var verUrl = "/" + emailFor + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace("/Registration", "").Replace("/ForgotPassword", "") + verUrl;
            Debug.WriteLine(Request.Url.AbsoluteUri);
            Debug.WriteLine(link); 

            #region ...
            var sender = new MailAddress("erixon09@gmail.com", "Aktywacja konta");
            var receiver = new MailAddress(email);
            var haslo = "POrtoryko123?";
            #endregion

            string subject = "";
            string body = "";

            if (emailFor == "VerifyAccount")
            {
                subject = "Link aktywacyjny konta";
                body = "<br/><br/> Twoje konto w bazie danych zostało poprawnie utworzone. W celu weryfikacji konta, kliknij w poniższy link" +
                    "<br/><br/><a href='" + link + "'>" + link + "</a>";
            }
            else if (emailFor == "ResetPassword") {
                subject = "Zmiana hasła";
                body = "Witaj!<br/><br/> Otrzymaliśmy prośbę o reset hasła z tego emaila. Kliknij w poniższy link, żeby zresetować hasło:<br/><br/><a href='"
                    + link + "'>" + link + "</a>";
            }

            #region Konfiguracja SMTP
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(sender.Address, haslo)
            };
            #endregion
            using (var message = new MailMessage(sender, receiver)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

    }
}