using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatRESTfullAPI.Data;
using ChatRESTfullAPI.Models;
using Microsoft.AspNetCore.Cors;

namespace ChatRESTfullAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("HealthPolicy")]
    public class ChatsController : ControllerBase
    {
        private readonly ChatContext _context;

        public ChatsController(ChatContext context)
        {
            _context = context;
        }


        /// <summary>
        /// gets all chats, for chats that are private it includes chatUsers for the chat
        /// </summary>
        /// <returns></returns>
        // GET: api/Chats        
        [HttpGet]
        public IEnumerable<Chat> GetChats()
        {
            List<Chat> chats = null;
            chats = _context.Chats.Include(u => u.ChatUsers).ToList();
           

            for (int i=0;i<chats.Count;i++)
            {
                if(!chats[i].Private)
                {
                    chats[i].ChatUsers = null;
                }
                else
                {
                    var chatusers = chats[i].ChatUsers.ToList();
                    for(int j=0;j<chatusers.Count;j++)
                    {                        
                        chatusers[j].Chat = null;
                    }
                    chats[i].ChatUsers = chatusers;
                }
            }
            return chats;
        }


        // GET: api/Chats/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChat([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var chat = await _context.Chats.FindAsync(id);

            if (chat == null)
            {
                return NotFound();
            }            

            return Ok(chat);
        }


        /// <summary>
        /// returns number=msgsOnPage messages for chat with chatId=Id from page=pageNumber 
        /// </summary>
        /// <param name="id">chat Id</param>
        /// <param name="pageNumber">page number to return messages from</param>
        /// <param name="msgsOnPage">number of messages to return</param>
        /// <returns>messages from certain page</returns>
        // GET: api/Chats/5/messages/5/20
        [HttpGet("{id}/messages/{pageNumber}/{msgsOnPage}")]
        public async Task<IActionResult> GetChatMessages([FromRoute] int id,int pageNumber,int msgsOnPage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
                       
            var chat = await _context.Chats.Include(m => m.ChatMessages)
                .FirstOrDefaultAsync(i => i.ChatId == id);

            if (chat == null)
            {
                return NotFound();
            }

            int msgsCount = chat.ChatMessages.Count();
            int pagesNumb, rest, count;

            if (msgsCount % msgsOnPage == 0)
            {
                pagesNumb = msgsCount / msgsOnPage;
                rest = 0;
                count = msgsOnPage;
            }
            else
            {
                pagesNumb = msgsCount / msgsOnPage + 1;
                rest=msgsOnPage- msgsCount % msgsOnPage;
                count = msgsCount % msgsOnPage;
            }
            List<Message> tempMessages = null;

            if (pageNumber == 1)
            {
                
                tempMessages = chat.ChatMessages.Take(count).ToList();
            }
            else if (pageNumber == 0)
            {
                tempMessages = chat.ChatMessages.TakeLast(msgsOnPage).ToList();
            }
            else
            {
                tempMessages = chat.ChatMessages.Skip((pagesNumb - 1) * msgsOnPage - rest).Take(msgsOnPage).ToList();
            }
          
            (List<Message>, int) complexChat = (tempMessages, chat.ChatMessages.Count);

            return Ok(complexChat);

        }


        /// <summary>
        /// returns users for certain chat with chatId=Id
        /// </summary>
        /// <param name="id">chatId for chat to return users from</param>
        /// <returns>users of the chat</returns>
        // GET: api/Chats/5/users
        [HttpGet("{id}/users")]
        public async Task<IActionResult> GetChatUsers([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userWithChats = await _context.Users.Include(u=>u.UserChats ).Include(m=>m.UserMessages).ToListAsync();
             
            if (userWithChats == null)
            {
                return NotFound();
            }
            List<User> users = new List<User>();
            foreach(var user in userWithChats)
            {
                var chat = user.UserChats.FirstOrDefault(h => h.ChatId == id);
                if (chat != null)
                {
                    user.UserChats = null;
                    users.Add(user);
                }
            }

            return Ok(users.OrderBy(n=>n.UserName));
        }


        /// <summary>
        /// updates certain chat with Id=id
        /// </summary>
        /// <param name="id">chat Id</param>
        /// <param name="message">message to be updated or added to 
        /// the current chat</param>
        /// <returns></returns>
        // PUT: api/Chats/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChat([FromRoute] int id, [FromBody] Message message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var user = await _context.Users.FindAsync(message.UserId);
            var chat = await _context.Chats.Include(m => m.ChatMessages).Include(p => p.ChatUsers)
                .FirstOrDefaultAsync(i => i.ChatId == id);           

            if (chat == null)
            {
                return NotFound();
            }

            if (user == null)
            {
                return NotFound();
            }

            if (!chat.ChatUsers.Where(n => n.UserId == message.UserId).Any())
            {
                ChatUser chatUser = new ChatUser();               
                chatUser.Chat = chat;
                chatUser.User = user;
                _context.ChatsUsers.Add(chatUser);
                await _context.SaveChangesAsync();
            }
            chat.ChatMessages.Add(message);          
            _context.Entry(chat).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChatExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        /// <summary>
        /// creates a new chat. Checks if chat is private 
        /// and if there is alredy a private chat with the same users. 
        /// </summary>
        /// <param name="chat">chat to be created</param>
        /// <returns>created chat or private chat which alredy exists with such users</returns>
        // POST: api/Chats
        [HttpPost]
        public async Task<IActionResult> PostChat([FromBody] Chat chat)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            List<ChatUser> tempUsers = chat.ChatUsers.ToList();
            bool chatExist = false;
           
            if (chat.Private)
            {
                var tempChats = _context.Chats.Include(c => c.ChatUsers).Where(p => p.Private == true).ToList();
                
                for (int i = 0; i < tempChats.Count; i++)
                {
                    var chatUsers = tempChats[i].ChatUsers.ToList();

                    for (int j = 0; j < tempUsers.Count; j++)
                    {
                        if (chatUsers.Where(n => n.UserId == tempUsers[j].UserId).Any())
                        {
                            chatExist = true;
                        }
                        else
                        {
                            chatExist = false;
                            break;
                        }
                    }
                    if (chatExist)
                    {                      
                        chat = _context.Chats.Find(tempChats[i].ChatId);
                        chat.ChatUsers = null;
                        return Ok(chat);                     
                    }
                }
            }           
                chat.ChatUsers = null;
                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();

                for (int i = 0; i < tempUsers.Count; i++)
                {
                    ChatUser chatUser = new ChatUser();
                    chatUser.Chat = chat;

                    var user = _context.Users.Find(tempUsers[i].UserId);
                    chatUser.User = user;

                    _context.ChatsUsers.Add(chatUser);
                    _context.SaveChanges();
                }

                _context.Entry(chat).State = EntityState.Modified;
                _context.SaveChanges();
                     
            chat.ChatUsers = null;

            return Ok(chat);  
        }



        // DELETE: api/Chats/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChat([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var chat = await _context.Chats.FindAsync(id);
            if (chat == null)
            {
                return NotFound();
            }

            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();

            return Ok(chat);
        }

        private bool ChatExists(int id)
        {
            return _context.Chats.Any(e => e.ChatId == id);
        }
    }
}