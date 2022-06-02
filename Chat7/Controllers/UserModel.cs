using System.Collections.Generic;

namespace Chat7.Controllers
{
    public class UserModel
    {
        public int id { get; set; }
        public string image { get; set; }
        public string name{ get; set; }
        public List<MessageData> MessageData { get; set; }
        //public List<MessageData1> MessageData1 { get; set; }
    }
    public class MessageData
    {
        public string message { get; set; }
        public int main { get; set; }
        public string date { get; set; }
        public string time { get; set; }
    }

    //public class MessageData1
    //{
    //    public string message { get; set; }
    //    public int main { get; set; }
    //    //public string date { get; set; }
    //    public string time { get; set; }
    //}
}