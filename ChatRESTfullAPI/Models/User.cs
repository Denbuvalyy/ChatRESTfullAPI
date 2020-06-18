using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatRESTfullAPI.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public ICollection<Message>UserMessages { get; set; }
        public ICollection <ChatUser>ChatUsers { get; set; }   
    }
}
