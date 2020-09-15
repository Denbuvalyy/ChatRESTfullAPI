using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatRESTfullAPI.Models
{
    public class Message
    {
        [Key]
        public int MsgId { get; set; }
        public DateTime CreationTime { get; set; }
        public string Body { get; set; }
        public int UserId { get; set; }        
        public int ChatId { get; set; }
        public bool UserNotVisible { get; set; }
    }
}
