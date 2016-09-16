using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace BotGame
{
    public class Mission
    {
        public int dif;
        public string capMis;
        public string shortMis;
        public int doneflag;
    }
    public class finalMission
    {
        public string capmis;
        public int mon;
        public int lose;
    }
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

            if (activity.Type == ActivityTypes.Message)
            {
                List<Mission> missionM= new List<Mission>();
                List<Mission> missionA = new List<Mission>();
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                var State = activity.GetStateClient();
                var UserData = await State.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
              
                //загружаем параметры бота
                var l= UserData.GetProperty<string>("l");
                var ng = UserData.GetProperty<string>("ng");
                var lose = UserData.GetProperty<string>("lose");
                var mon = UserData.GetProperty<int>("mon");
                var awr = UserData.GetProperty<int>("awr");
                var nm = UserData.GetProperty<int>("nm");
                var choose = UserData.GetProperty<int>("choose");
                var numbM = UserData.GetProperty<int>("numbM");
                var numbA = UserData.GetProperty<int>("numbA");
                string msg = "";
                //загружаем список вопросов
                if (l == null)
                            {
                                //создаю миссии
                                Mission temp1 = new Mission();
                                temp1.dif = 1;
                                temp1.doneflag = 0;
                                temp1.shortMis = "Убить";
                                temp1.capMis = "Убить президента";
                                missionM.Add(temp1);
                                // закончил создавать миссии
                                UserData.SetProperty<int>("nmm",1);
                                UserData.SetProperty<int>("nma", 1);
                                UserData.SetProperty<List<Mission>>("missionM", missionM);
                                UserData.SetProperty<List<Mission>>("missionA", missionA);
                                UserData.SetProperty<string>("l", "1");
                            }
                            else
                            {
                                missionM = UserData.GetProperty<List<Mission>>("missionM");
                                missionA = UserData.GetProperty<List<Mission>>("missionA");
                            }
                
                //проверяем новая игра или нет
                if ((ng==null)||(ng=="new"))
                    {
                        UserData.SetProperty<string>("lose", "0");
                        UserData.SetProperty<int>("mon", 0);
                        UserData.SetProperty<int>("awr", 0);
                        UserData.SetProperty<int>("nm", 5);
                        UserData.SetProperty<int>("choose", 0);
                        UserData.SetProperty<string>("ng", "old");
                        msg = "Здравствуйте, уважаемый злобный гений! Вас приветствует консоль управления вашими подчиненными. Здесь вы можете посылать злыдней на задания, либо вербовать новых злыдней.\r\nПомните, что чем больше злыдней вы пошлете на задание, тем выше вероятность его успешного завершения, но ваша известность также повысится. Оставшиеся злыдни будут защищать вашу базу. Помните также, что чем больше ваша известность, тем сложнее будет защитить базу и потребуется больше злыдней для ее защиты. P.S. Чтобы запустить протокол самоуничтожения - введите любой текст вместо цифр";
                    }
                //проверка на проигрыш
                int number;
                bool result = Int32.TryParse(activity.Text, out number);
                if ((lose == "1")||(!result))
                    {
                         UserData.SetProperty<string>("ng", "new");
                         msg = "Ваша база была уничтожена. Нажмите любую клавишу для продолжения";
                    }
               //основная часть игры
               else
                {
                    switch (choose)
                    {
                        //основное меню
                        case 0:

                            break;
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                        case 4:
                            break;
                        case 5:
                            break;
                        case 6:
                            break;
                        case 7:
                            break;
                    }
                }
                    Activity reply = activity.CreateReply($"{msg},\r\n  {missionM[0].capMis} ");
                    await State.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, UserData);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}