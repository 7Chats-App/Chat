using Chat7.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Chat7.Controllers
{
    public class HomeController : Controller
    {
        ChatEntities db = new ChatEntities();
        SendEmail sendEmail = new SendEmail();
        public ActionResult Index()
        {

            return View();
        }
        public JsonResult Login(SignUp signUp)
        {
            var account_ = db.AccountDetails.FirstOrDefault(x => x.Email.ToLower() == signUp.email.ToLower() && x.Password == signUp.password);
            if (account_ != null)
            {
                Session["Name"] = account_.Email;
                Session["Expiry"] = DateTime.UtcNow.AddMinutes(20);
                Session["ID"] = account_.Id;
                return Json("successful");
            }
            else
            {
                return Json("User not found , incorrect username or password");
            }
        }
        public ActionResult Signup()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Signup(SignUp signUp)
        {
            var account_ = db.AccountDetails.FirstOrDefault(x => x.Email.ToLower() == signUp.email.ToLower());
            if (account_ != null)
            {
                return Json("account already exist");
            }
            AccountDetail account = new AccountDetail
            {
                Active = false,
                CreatedDay = DateTime.Now,
                Email = signUp.email,
                Password = signUp.password,
                FirstName = signUp.name,
            };
            try
            {

                db.AccountDetails.Add(account);
                db.SaveChanges();
                Session["Email"] = account.Email;
                Session["Expiry"] = DateTime.UtcNow.AddMinutes(20);
                Session["ID"] = account.Id;
            }
            catch (Exception ex)
            {
                return Json("Something went wrong please try again or contact this number");
            }
            return Json("successful");
        }
        public ActionResult ForgotPassword()
        {
            return View();
        }
        public ActionResult Chat()
        {
            return View();
        }
        public ActionResult CompleteProfile()
        {
            return View();
        }
        public ActionResult UploadImages()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadImages(List<HttpPostedFileBase> postedFiles)
        {
            string path = Server.MapPath("~/Uploads/");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            try
            {
                foreach (HttpPostedFileBase pFile in postedFiles)
                {
                    if (postedFiles != null)
                    {
                        string fileName = Path.GetFileName(pFile.FileName);
                        ImageToByteArray(pFile, pFile.FileName, pFile.ContentType);
                    }
                }
            }
            catch (Exception)
            {
                return View();
            }
            return RedirectToAction("Chat");
        }
        public void ImageToByteArray(HttpPostedFileBase model, string fileName, string fileType)
        {
            byte[] data;
            int ID = 0;
            try
            {
                var id = Session["ID"].ToString();
                int.TryParse(id, out ID);
            }
            catch (Exception)
            {

                throw;
            }

            using (Stream inputStream = model.InputStream)
            {
                MemoryStream memoryStream = inputStream as MemoryStream;
                if (memoryStream == null)
                {
                    memoryStream = new MemoryStream();
                    inputStream.CopyTo(memoryStream);
                }
                data = memoryStream.ToArray();
            }
            var account_ = db.AccountDetails.FirstOrDefault(x => x.Id == ID);
            account_.Image1 = data;
            account_.Active = true;
            db.Entry(account_).State = EntityState.Modified;
            db.SaveChanges();
            // ViewBag.Message += string.Format("<b>{0}</b> uploaded.<br />", fileName);
        }
        public JsonResult Complete(AccountDetail accountDetail)
        {
            if (Session["ID"] == null)
            {
                return Json("session expired sign in again");
            }
            var id = Session["ID"].ToString();
            var account_ = db.AccountDetails.FirstOrDefault(x => x.Id.ToString() == id);
            account_.LastName = accountDetail.LastName;
            account_.BirthDay = accountDetail.BirthDay;
            account_.City = accountDetail.City;
            account_.Province = accountDetail.Province;
            account_.ShowMe = accountDetail.ShowMe;
            account_.Gender = accountDetail.Gender;
            db.Entry(account_).State = EntityState.Modified;
            db.SaveChanges();
            return Json("successful");
        }
        public JsonResult LoadData()
        {
            if (Session["ID"] == null)
            {
                return Json("session expired sign in again");
            }
            var id = Session["ID"].ToString();
            var account_ = db.AccountDetails.FirstOrDefault(x => x.Id.ToString() == id);
            string temp_backToBytes = Convert.ToBase64String(account_.Image1);

            DataModel model = new DataModel()
            {
                image = temp_backToBytes,
                email = account_.Email,
                name = account_.FirstName + " " + account_.LastName,
                location = account_.Province + " , " + account_.City
            };
            return Json(new
            {
                success = "successful",
                data = model,
                JsonRequestBehavior.AllowGet
            });
        }
        public JsonResult ForgotPasswords(AccountDetail accountDetail)
        {
            var account = db.AccountDetails.FirstOrDefault(x => x.Email.ToLower().Trim() == accountDetail.Email.ToLower().Trim());
            if (account == null)
            {
                return Json("Account doesnt exist");
            }
            else
            {
                sendEmail.Send(account.Email, "Your password is " + account.Password, "forgot password");
                return Json("successful");
            }
        }

        public JsonResult LoadChat()
        {
            if (Session["ID"] == null)
            {
                return Json("unsuccessful");
            }
            var id = Session["ID"].ToString();
            var account_ = db.AccountDetails.FirstOrDefault(x => x.Id.ToString() == id);
            var users = db.AccountDetails.Where(x => x.Id != account_.Id && x.Province==account_.Province && x.Gender==account_.ShowMe);

            List<DataModel> list = new List<DataModel>();

            foreach (var s in users)
            {
                bool invite = false;
                var invited = db.Invites.FirstOrDefault(x => x.Second == s.Id && x.MainID.ToString() == id);
                if (invited == null)
                {
                    invite = false;
                }
                else
                {
                    invite = true;
                }

                string temp_backToBytes = Convert.ToBase64String(s.Image1);
                DataModel model = new DataModel()
                {
                    image = temp_backToBytes,
                    email = s.Email,
                    name = s.FirstName + " " + s.LastName,
                    location = s.Province + " , " + s.City,
                    id = s.Id,
                    invited = invite
                };

                var main = db.Invites.FirstOrDefault(x => x.MainID == s.Id && x.Second.ToString() == id);
                if (main == null)
                {
                    var messages = db.Messages.FirstOrDefault(x => (x.MainID.ToString() == id || x.SecondID.ToString() == id) && (x.MainID == s.Id || x.SecondID == s.Id));
                    if (messages == null)
                    {
                        list.Add(model);
                    }
                }
            }

            return Json(new
            {
                success = "successful",
                data = list,
                JsonRequestBehavior.AllowGet
            });
        }
        public JsonResult Invite(InviteModel invite)
        {

            try
            {
                if (Session["ID"] == null)
                {
                    return Json("session expired sign in again");
                }
                var mainID = Session["ID"].ToString();
                int _id = int.Parse(mainID);
                Invite model = new Invite()
                {
                    Accepted = false,
                    IsActive = true,
                    MainID = _id,
                    Second = invite.id
                };
                db.Invites.Add(model);
                db.SaveChanges();
                return Json("successful");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        public JsonResult LoadInvites()
        {
            if (Session["ID"] == null)
            {
                return Json("session expired sign in again");
            }
            var id = Session["ID"].ToString();
            var invite = db.Invites.Where(x => x.Second.ToString() == id);
            List<DataModel> list = new List<DataModel>();
            foreach (var s in invite)
            {
                var user = db.AccountDetails.FirstOrDefault(x => x.Id == s.MainID);
                if (user != null)
                {

                    string temp_backToBytes = Convert.ToBase64String(user.Image1);
                    DataModel model = new DataModel()
                    {
                        image = temp_backToBytes,
                        email = user.Email,
                        name = user.FirstName + " " + user.LastName,
                        location = user.Province + " , " + user.City,
                        id = user.Id,

                    };
                    list.Add(model);
                }
            }
            return Json(new
            {
                success = "successful",
                data = list,
                JsonRequestBehavior.AllowGet
            });
        }
        public JsonResult Accept(InviteModel invite)
        {
            try
            {
                if (Session["ID"] == null)
                {
                    return Json("session expired sign in again");
                }
                var id = Session["ID"].ToString();
                int main = int.Parse(id);
                Message message = new Message()
                {
                    DateCreated = DateTime.Now,
                    isMain = true,
                    MainID = main,
                    Message1 = "Click to chat.",
                    SecondID = invite.id
                };
                db.Messages.Add(message);
                var data = db.Invites.FirstOrDefault(x => x.Second.ToString() == id && x.MainID == invite.id);
                db.Invites.Remove(data);
                db.SaveChanges();
                return Json("successful");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }

        }
        public JsonResult Declined(InviteModel invite)
        {
            if (Session["ID"] == null)
            {
                return Json("session expired sign in again");
            }
            var id = Session["ID"].ToString();
            var data = db.Invites.FirstOrDefault(x => x.Second.ToString() == id && x.MainID == invite.id);
            db.Invites.Remove(data);
            db.SaveChanges();
            return Json("successful");
        }

        public JsonResult LoadCharHistory()
        {
            if (Session["ID"] == null)
            {
                return Json("unsuccessful");
            }
            List<MessageModel> list = new List<MessageModel>();
            var id = Session["ID"].ToString();
            var message = db.Messages.ToList().OrderByDescending(x=>x.DateCreated).Where(x => x.MainID.ToString() == id || x.SecondID.ToString() == id).GroupBy(p => p.SecondID).Select(grp => grp.FirstOrDefault()).ToList();

            foreach (var s in message)
            {
                var user = db.AccountDetails.FirstOrDefault(x => x.Id == s.MainID);
                if (user.Id.ToString() == id)
                {
                    user = db.AccountDetails.FirstOrDefault(x => x.Id == s.SecondID);
                }
                if (user != null)
                {
                    string temp_backToBytes = Convert.ToBase64String(user.Image1);
                    MessageModel model = new MessageModel()
                    {
                        messageID = s.Id,
                        message = s.Message1,
                        image = temp_backToBytes,
                        name = user.FirstName + " " + user.LastName,
                        date = s.DateCreated.ToString("MM/dd/yyyy HH:mm")
                        //date = s.DateCreated.ToString("HH:mm")
                    };
                    list.Add(model);
                }
                list = list.GroupBy(p => p.name).Select(grp => grp.LastOrDefault()).ToList();
            }
            return Json(new
            {
                success = "successful",
                data = list,
                JsonRequestBehavior.AllowGet
            });

        }
        public JsonResult SendInvite(SignUp signUp)
        {
            if (Session["ID"] == null)
            {
                return Json("session expired sign in again");
            }
            var id = Session["ID"].ToString();
            var account_ = db.AccountDetails.FirstOrDefault(x => x.Id.ToString() == id);
            string DomainName = HttpContext.Request.Url.Host;
            string subject = "You are invited by " + account_.FirstName + "to join the 7chats App";
            string email = signUp.email;
            string body = "You are invited by " + account_.FirstName + "to join the 7chats App , click on this link to join " + DomainName + " message : " + signUp.password;
            sendEmail.Send(email, body, subject);
            return Json("successful");
        }

        public ActionResult DirectChat(int messageID)
        {
            if(messageID.ToString()=="")
            {
                return RedirectToAction("Index", "Home");
            }
            if (Session["ID"]==null)
            {
                return RedirectToAction("Index", "Home");
            }
            int id_ = 0;
            var id = Session["ID"].ToString();
            var message = db.Messages.Where(x => x.Id == messageID);
            if (message.FirstOrDefault().MainID.ToString() == id)
            {
                id_ = message.FirstOrDefault().SecondID;
            }
            else
            {
                id_ = message.FirstOrDefault().MainID;
            }
            var account = db.AccountDetails.FirstOrDefault(x => x.Id == id_);
            string temp_backToBytes = Convert.ToBase64String(account.Image1);

            UserModel model = new UserModel()
            {
                id = id_,
                image = temp_backToBytes,
                name = account.FirstName
            };


            List<MessageData> messageDatas = new List<MessageData>();
            var messages = db.Messages.Where(x => (x.MainID.ToString() == id && x.SecondID == id_) || (x.MainID == id_ && x.SecondID.ToString() == id));
            foreach (var s in messages)

            {
                int mainId = 0;
                if (s.MainID.ToString() == id)
                {
                    mainId = 1;
                }

                MessageData messageData = new MessageData()
                {
                    date = s.DateCreated.ToString("MM/dd/yyyy"),
                    time = s.DateCreated.ToString("hh:mm:ss"),
                    message = s.Message1,
                    main = mainId
                };

                messageDatas.Add(messageData);
            }
   
            model.MessageData = messageDatas;
            return View(model);
        }

        public JsonResult SendTest(SignUp signUp)
        {
            try
            {
                if (Session["ID"] == null)
                {
                    return Json("session expired sign in again");
                }
                var id = Session["ID"].ToString();
                int main = int.Parse(id);
                int second = int.Parse(signUp.name);
                Message message = new Message()
                {
                    DateCreated = DateTime.Now,
                    isMain = true,
                    MainID = main,
                    Message1 = signUp.email,
                    SecondID = second
                };
                db.Messages.Add(message);
                db.SaveChanges();
                return Json("successful");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
    }
}