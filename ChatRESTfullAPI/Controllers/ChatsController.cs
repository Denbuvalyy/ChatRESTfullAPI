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

            //tempMessages = chat.ChatMessages.
            //    Skip((pageNumber - 1) * msgsOnPage).TakeLast(msgsOnPage).ToList();

            //List<Message> tempMessages = chat.ChatMessages.
            //    Skip((pageNumber - 1) * msgsOnPage).Take(msgsOnPage).ToList();


            (List<Message>, int) complexChat = (tempMessages, chat.ChatMessages.Count);

            //(List<Message>, int) complexChat = (chat.ChatMessages.ToList(), chat.ChatMessages.Count);
            return Ok(complexChat);
            //return Ok(chat.ChatMessages);
        }

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

        // PUT: api/Chats/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChat([FromRoute] int id, [FromBody] Message message)//[FromBody] Chat chat)
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

            //if (id != chat.ChatId)
            //{
            //    return BadRequest();
            //}

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
            //return CreatedAtAction("GetChat", new { id = chat.ChatId }, chat);
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