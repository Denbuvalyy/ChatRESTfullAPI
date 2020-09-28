using ChatRESTfullAPI.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatRESTfullAPI.Data
{
    public class ArtificialData
    {
        public static void Initialize(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ChatContext>();
                context.Database.EnsureCreated();               

                if (context.Users != null && context.Users.Any())
                    return;

                var users = ArtificialData.GetUsers().ToArray();
                context.Users.AddRange(users);
                context.SaveChanges();

                var chats = ArtificialData.GetChats().ToArray();
                context.Chats.AddRange(chats);
                context.SaveChanges();

                var chatusers = ArtificialData.GetChatUsers(context).ToArray();
                context.ChatsUsers.AddRange(chatusers);
                context.SaveChanges();

                var messages = ArtificialData.GetMessages(context).ToArray();
                context.Messages.AddRange(messages);
                context.SaveChanges();
            }
        }
        public static string MessegaGeny()
        {
            Random rnd = new Random();            
            List<string> bunch = new List<string>
            {
                "Hello!","By the way","First off all","I don't think so","It's up to you","wouldn't do that",
                "That's a realy nice place","Morning","I'm flying to Spain this Saturday","Never mind",
                "How are you?","Felling good","Great news","to be or not to be","I'm working tomorrow",
                "Does it","I don't know him","Bingo","Goodnight","Tomorrow morning we'll see that",
                "It's gonna be the last time","Well, well","It's raining over here"
            };
            return bunch[rnd.Next(0, bunch.Count)];
        }

        public static int CheckIndex(List<int> indexList,int count)
        {
            Random rnd = new Random();
            int index;
            do
            {
                index = rnd.Next(0, count);

                if (indexList.Count >= count)
                {
                    break;
                }
                    
            } while (indexList.Contains(index));
            return index;
        }

        public static List<Message> GetMessages(ChatContext db)
        {
            Random rnd = new Random();
            List<Chat> chats = db.Chats.ToList();
            List<ChatUser> chatUsers = db.ChatsUsers.ToList();
          
            List<ChatUser> chatUsersIds = null;
            int index;
            List<Message> messages = new List<Message>();

            for (int i = 0; i < 100;i++)
            {
                var message = new Message();
                message.Body = MessegaGeny();
                message.CreationTime = DateTime.Now;
                index = rnd.Next(0, chats.Count);
                message.ChatId = chats[index].ChatId;
                chatUsersIds = chatUsers.Where(p => p.ChatId == chats[index].ChatId).ToList();
                index = rnd.Next(0,chatUsersIds.Count);
                message.UserId =chatUsersIds[index].UserId;                
                messages.Add(message);
            }                     
            return messages;
        }
       
        public static List<User> GetUsers()
        {
            List<User> users = new List<User>
            {
                new User{UserName="Sunshy", Login="sunshy", Password="sunshy1234"},
                new User{UserName="Ann", Login="ann", Password="ann1234"},
                new User{UserName="Tom", Login="tom", Password="tom1234"},
                new User{UserName="Grumpy", Login="grumpy", Password="grumpy1234"},
                new User{UserName="Smiley", Login="smiley", Password="smiley1234"},
                new User{UserName="The Last", Login="thelast", Password="thelast1234"},
                new User{UserName="Ficus", Login="ficus", Password="ficus1234"},
                new User{UserName="Archie", Login="archie", Password="archie1234"}
            };
            return users;
        }

        public static List<Chat>GetChats()
        {
            List<Chat>chats = new List<Chat>
            {
                new Chat{ChatName="Joyfull mood"},
                new Chat{ChatName="Weekend plans"},
                new Chat{ChatName="Holiday impression"},
                new Chat{ChatName="The best pub opinion"},
                new Chat{ChatName="Getting out from here"},
                new Chat{ChatName="Monday mood"},
                new Chat{ChatName="The best impression"}
            };
            return chats;
        }

        public static List<ChatUser>GetChatUsers(ChatContext db)
        {
            Random rnd = new Random();
            List<Chat> chats = db.Chats.ToList();
            List<User> users = db.Users.ToList();
            List<ChatUser> chatUsers = new List<ChatUser>();
            
            foreach(var chat in chats)
            {
                int index;
                List<int> existingItems = new List<int>();
                for (int i = 0; i < rnd.Next(1, users.Count+1); i++)
                {
                    var item = new ChatUser();
                    item.Chat = chat;

                    index = CheckIndex(existingItems, users.Count);
                    item.User = users[index];
                    existingItems.Add(index);

                    chatUsers.Add(item);
                }
            }
            return chatUsers;
        }       
    }
}
