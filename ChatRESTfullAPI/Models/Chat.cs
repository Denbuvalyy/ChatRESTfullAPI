using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatRESTfullAPI.Models
{
    public class Chat
    {
        [Key]
        public int ChatId { get; set; }
        public string ChatName { get; set; }
        public bool Private { get; set; }       
        public ICollection<Message>ChatMessages { get; set; }        
        public ICollection <ChatUser>ChatUsers { get; set; }    
    } 
}
