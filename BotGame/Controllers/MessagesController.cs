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
using Newtonsoft.Json;
//залечь на дно 
//сделать псевдорандом
namespace BotGame
{
    public class Mission
    {
        public int id;
        public string dif;
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
    public class MissionCollection
    {
        public List<Mission> dataM;
        public List<Mission> dataA;
    }
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        Random random = new Random();
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {

                List<Mission> missionM = new List<Mission>();
                List<Mission> missionA = new List<Mission>();
                Mission[] mission5 = new Mission[5];
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                var State = activity.GetStateClient();
                var UserData = await State.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                //загружаем параметры бота
                var l = UserData.GetProperty<string>("l");
                var ng = UserData.GetProperty<string>("ng");
                var numbM = UserData.GetProperty<int>("numbM");
                var numbA = UserData.GetProperty<int>("numbA");
                string msg = "";
                //загружаем список вопросов
                if ((l == null) || (l == "s"))
                {
                    var p = System.Web.HttpContext.Current.Request.MapPath("~/Data/missions.json");
                    var temp1 = JsonConvert.DeserializeObject<MissionCollection>(File.ReadAllText(p));
                    missionM = temp1.dataM;
                    missionA = temp1.dataA;
                    // закончил создавать миссии
                    UserData.SetProperty<List<Mission>>("missionM", missionM);
                    UserData.SetProperty<List<Mission>>("missionA", missionA);
                    UserData.SetProperty<string>("l", "1");
                }
                else
                {
                    missionM = UserData.GetProperty<List<Mission>>("missionM");
                    missionA = UserData.GetProperty<List<Mission>>("missionA");
                    mission5 = UserData.GetProperty<Mission[]>("mission5");
                }
                //проверяем новая игра или нет
                if ((ng == null) || (ng == "new"))
                {
                    UserData.SetProperty<string>("lose", "0");
                    UserData.SetProperty<int>("mon", 0);
                    UserData.SetProperty<int>("awr", 0);
                    UserData.SetProperty<int>("nm", 5);
                    UserData.SetProperty<int>("choose", 0);
                    UserData.SetProperty<string>("ng", "old");
                    UserData.SetProperty<int>("eWin", 0);
                    UserData.SetProperty<int>("aprt", 3);                    
                    msg = "Здравствуйте, босс! Вся мафия ждала вашего возвращения. Вы можете посылать ваших подчиненных \U0001f479 на задания и вербовать новых \U0001f479." +
                        "\n\nПомните: чем больше \U0001f479 вы пошлете на задание, тем выше вероятность его успешного завершения, но внимание властей \U0001f46e также повысится пропорционально."+
                        "\n\n Оставшиеся \U0001f479 будут защищать вашу базу. Помните: чем больше \U0001f46e, тем сложнее будет защитить базу."+
                        "\n\n P.S. Чтобы начать новую игру - введите любой текст вместо цифр.\n\n_________________________________________________";
                }
                //выгрузка параметров (после того, как решено, новая игра или же нет)
                var lose = UserData.GetProperty<string>("lose");
                var mon = UserData.GetProperty<int>("mon");
                var awr = UserData.GetProperty<int>("awr");
                var nm = UserData.GetProperty<int>("nm");
                var choose = UserData.GetProperty<int>("choose");
                var eWin = UserData.GetProperty<int>("eWin");
                var aprt = UserData.GetProperty<int>("aprt");
                int temp = 0;
                //проверка на проигрыш
                int number;
                bool result = Int32.TryParse(activity.Text, out number);
                if ((lose == "1") || ((!result) && (msg == "")))
                {
                    UserData.SetProperty<string>("ng", "new");
                    UserData.SetProperty<string>("l", "s");
                    if (eWin >= 100)
                    {
                        msg = "\n\nВся страна ваша, босс! Наслаждайтесь властью!";
                    }
                    else
                    {
                        msg = "\n\nВаша база была захвачена. Нажмите любую цифру для продолжения\n\n________\n\nПрисылайте ваши предложения, замечания, найденные баги - arfr@live.ru";
                    }
                }
                //основная часть игры
                else 
                {
                    int price = 0;
                    if ((choose == 0) && (msg == ""))
                    {
                        choose = Convert.ToInt32(activity.Text);
                        if (choose == 8)
                        {                            
                            if (aprt > 0)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    mission5[i].doneflag = 0;
                                    missionM[mission5[i].id].doneflag = 0;
                                }
                                for (int i = 3; i < 5; i++)
                                {
                                    mission5[i].doneflag = 0;
                                    missionA[mission5[i].id].doneflag = 0;
                                }
                                aprt = aprt - 1;
                                awr = awr - 5;
                                if (awr < 0)
                                {
                                    awr = 0;
                                }
                                msg += $"Все задания были обновлены, а \U0001f46e немного уменьшилось.  У вас осталось {aprt} секретных квартир.\n\n_________________________________________________";
                            }
                            else
                            {
                                msg += "У вас не осталось секретных квартир!\n\n_________________________________________________";
                            }
                            choose = 0;
                        }
                    }
                    else
                    {
                        if (msg != "")
                        {
                            choose = 0;
                        }
                        else
                        {
                            if (Convert.ToInt32(activity.Text) == 0)
                            {
                                choose = 0;
                            }
                            else
                            {
                                int pAtt = Convert.ToInt32(activity.Text);
                                int pDef = nm - pAtt;                              
                                int cod = 0;
                                int awr1 = 0;
                                int mon1 = 0;
                                int cow = 0;
                                int deadPeople = 0;
                                switch (choose)
                                {
                                    case 0:
                                        choose = 0;
                                        break;
                                    case 1:                                        
                                    case 2:
                                    case 3:
                                        temp = choose;
                                        if (pDef < 0)
                                        {
                                            msg += "\n\nНедостаточно \U0001f479 \n\n_________________________________________________";
                                            choose = temp;
                                        }
                                        else
                                        {
                                            cod = awr - 3 * pDef;
                                            for (int i = 0; i < pDef; i++)
                                            {
                                                if (random.Next(100) < cod)
                                                {
                                                    deadPeople++;
                                                }
                                            }
                                            if ((awr>0)&&(pDef - deadPeople <= 0))
                                            {
                                                msg += $"Пытаясь вас защитить, погибли все защитники \U0001f479.\n\n_________________________________________________";
                                                choose = 9;
                                            }
                                            else
                                            {
                                                if (awr > 0) { msg += $"Наши парни никому не дали добраться до вас. Погибло {deadPeople} из {pDef} защитников.\n\n_________________________________________________"; }
                                                else { msg += $"Повезло, что власти нами не интересуются и нас сегодня никто не штурмовал.\n\n_________________________________________________"; }                                                
                                                cod = 0;
                                                switch (mission5[temp-1].dif)
                                                {
                                                    case "легко":
                                                        awr1 = 4;
                                                        mon1 = 150;
                                                        cow = 30;
                                                        cod = 20 - 4 * pAtt;
                                                        break;
                                                    case "средне":
                                                        awr1 = 3;
                                                        mon1 = 400;
                                                        cow = 14;
                                                        cod = 30 - 3 * pAtt;
                                                        break;
                                                    case "тяжело":
                                                        awr1 = 2;
                                                        mon1 = 900;
                                                        cow = 10;
                                                        cod = 40 - 3 * pAtt;
                                                        break;
                                                }
                                                nm = nm - deadPeople;
                                                deadPeople = 0;
                                                awr += awr1 * pAtt;
                                                var r = random.Next(100);
                                                if ((r < (cow * pAtt)) && (r < 95))
                                                {
                                                    mon += mon1;
                                                    msg += $"\n\nМиссия выполнена, награда - {mon1} \U0001f4B0.";
                                                }
                                                else
                                                {
                                                    msg += $"\n\nМиссия провалена.";
                                                }

                                                for (int i = 0; i < pAtt; i++)
                                                {
                                                    if (random.Next(100) < cod)
                                                    {
                                                        deadPeople++;
                                                    }
                                                }
                                                msg += $" На задании погибло {deadPeople} \U0001f479.\n\n_________________________________________________";
                                                nm = nm - deadPeople;
                                                mission5[temp-1].doneflag = -1;
                                                missionM[mission5[temp-1].id].doneflag = -1;
                                                choose = 0;
                                            }
                                        }
                                        break;
                                    case 4:                                        
                                    case 5:
                                        temp = choose;
                                        switch (mission5[temp-1].dif)
                                        {
                                            case "легко":
                                                awr1 = 20;
                                                mon1 = 20;
                                                cow = 30;
                                                cod = 20 - 4 * pAtt;
                                                price = 50;
                                                break;
                                            case "средне":
                                                awr1 = 35;
                                                mon1 = 35;
                                                cow = 14;
                                                cod = 30 - 3 * pAtt;
                                                price = 45;
                                                break;
                                            case "тяжело":
                                                awr1 = 50;
                                                mon1 = 50;
                                                cow = 10;
                                                cod = 40 - 3 * pAtt;
                                                price = 40;
                                                break;
                                        }
                                        if ((pDef < 0) || (mon - price * pAtt < 0))
                                        {
                                            msg += "\n\nНе хватает \U0001f479 или \U0001f4B0.\n\n_________________________________________________";
                                            choose = temp;
                                        }
                                        else
                                        {
                                            int cod1 = awr - 3 * pDef;
                                            for (int i = 0; i < pDef; i++)
                                            {

                                                if (random.Next(100) < cod1)
                                                {
                                                    deadPeople++;
                                                }
                                            }
                                             if ((awr>0)&&(pDef - deadPeople <= 0))
                                            {
                                                msg += $"Пытаясь вас защитить, погибли все защитники \U0001f479.\n\n_________________________________________________";
                                                choose = 9;
                                            }
                                            else
                                            {
                                                if (awr > 0) { msg += $"Наши парни никому не дали добраться до вас. Погибло {deadPeople} из {pDef} защитников.\n\n_________________________________________________"; }
                                                else { msg += $"Повезло, что власти нами не интересуются и нас сегодня никто не штурмовал.\n\n_________________________________________________"; }   
                                                nm = nm - deadPeople;
                                                deadPeople = 0;
                                                mon -= price * pAtt;

                                                var r = random.Next(100);
                                                if ((r < (cow * pAtt)) && (r < 95))
                                                {
                                                    awr -= awr1;
                                                    if (awr < 0)
                                                    {
                                                        awr = 0;
                                                    }
                                                    msg += $"\n\nМиссия выполнена, снижение - {awr1}% \U0001f46e";
                                                }
                                                else
                                                {
                                                    msg += $"\n\nМиссия провалена.";
                                                }

                                                for (int i = 0; i < pAtt; i++)
                                                {
                                                    if (random.Next(100) < cod)
                                                    {
                                                        deadPeople++;
                                                    }
                                                }
                                                msg += $" На задании погибло {deadPeople} \U0001f479.\n\n_________________________________________________";
                                                nm = nm - deadPeople;
                                                mission5[temp-1].doneflag = -1;
                                                missionA[mission5[temp-1].id].doneflag = -1;
                                                choose = 0;
                                            }
                                        }
                                        break;
                                    case 6:
                                        if (mon - pAtt * 100 < 0)
                                        {
                                            msg += "\n\nУ вас нет столько \U0001f4B0.\n\n_________________________________________________";
                                            choose = 6;
                                        }
                                        else
                                        {
                                            mon = mon - pAtt * 100;
                                            nm = nm + pAtt;
                                            awr += 3*pAtt;
                                            choose = 0;
                                        }
                                        break;
                                    case 7:
                                        deadPeople = 0;
                                        if ((pDef < 0) || (mon - 150 * pAtt < 0))
                                        {
                                            msg += "\n\nУ вас нет столько \U0001f479 или \U0001f4B0.\n\n_________________________________________________";
                                            choose = 7;
                                        }
                                        else
                                        {
                                            cow = pAtt * 4;
                                            for (int i = 0; i < pAtt; i++)
                                            {

                                                if (random.Next(100) < cow)
                                                {
                                                    eWin += random.Next(6);
                                                }
                                                if (random.Next(100) < 40 - 3 * pAtt)
                                                {
                                                    deadPeople++;
                                                }
                                            }
                                            nm -= deadPeople;
                                            awr = awr + 3 * pAtt;
                                            mon = mon - 150 * pAtt;
                                            deadPeople = 0;
                                            if (eWin >= 100)
                                            {
                                                choose = 9;
                                            }
                                            else
                                            {
                                                int cod1 = awr - 3 * pDef;
                                                for (int i = 0; i < pDef; i++)
                                                {

                                                    if (random.Next(100) < cod1)
                                                    {
                                                        deadPeople++;
                                                    }
                                                }
                                                if (pDef - deadPeople <= 0)
                                                {
                                                    msg += $"Пытаясь вас защитить, погибли все({pDef}) защитники \U0001f479.\n\n_________________________________________________";
                                                    choose = 9;
                                                }
                                                else
                                                {
                                                    msg += $"Наши парни никому не дали добраться до вас. Погибло {deadPeople} из {pDef} защитников.\n\n_________________________________________________";
                                                    nm = nm - deadPeople;
                                                    choose = 0;
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        choose = 0;
                                        break;
                                }
                            }
                        }
                    }  
                    //Вывод меню
                    switch (choose)
                    {    
                        case 0:
                            msg += "\n\n\U0001f4B0: " + mon + " | | | \U0001f479: " + nm + " | | | \U0001f46e: " + awr + "% \n\n_________________________________________________";
                            //заполняю строчки заданиями                            
                            for (var k = 0; k < 3; k++)
                            {
                                int k1 = k + 1;
                                mission5[k] = missionM.FirstOrDefault(t => t.doneflag == k1);
                                if (mission5[k] == null)
                                {
                                    List<int> randMission = new List<int>();
                                    for (var i = 0; i < missionM.Count; i++)
                                        if (missionM[i].doneflag == 0)
                                            randMission.Add(missionM[i].id);
                                    if (randMission.Count == 0)
                                    {
                                        for (int i=0;i< missionM.Count; i++)
                                        {
                                            if (missionM[i].doneflag < 0)
                                            {
                                                missionM[i].doneflag = 0;
                                                randMission.Add(missionM[i].id);
                                            }
                                        }

                                    }
                                    mission5[k] = missionM[randMission[random.Next(randMission.Count)]];
                                    mission5[k].doneflag = k1;
                                }
                            }
                            for (var k = 3; k < 5; k++)
                            {
                                int k1 = k + 1;
                                mission5[k] = missionA.FirstOrDefault(t => t.doneflag == k1);
                                if (mission5[k] == null)
                                {
                                    
                                    List<int> randMission = new List<int>();
                                    for (var i = 0; i < missionA.Count; i++)
                                        if (missionA[i].doneflag == 0)
                                            randMission.Add(missionA[i].id);
                                    if (randMission.Count == 0)
                                    {
                                        for (int i = 0; i < missionA.Count; i++)
                                        {
                                            if (missionA[i].doneflag < 0)
                                            {
                                                missionA[i].doneflag = 0;
                                                randMission.Add(missionA[i].id);
                                            }
                                        }
                                        
                                    }
                                    mission5[k] = missionA[randMission[random.Next(randMission.Count)]];
                                    mission5[k].doneflag = k1;
                                }
                            }
                                                   
                            for (int i = 0; i < 3; i++)
                                msg += $"\n\n({i + 1}) {mission5[i].shortMis} ({mission5[i].dif}) \U0001f4B0+";
                            for (int i = 3; i < 5; i++)
                                msg += $"\n\n({i + 1}) {mission5[i].shortMis} ({mission5[i].dif}) \U0001f46e-";
                            msg += $"\n\n(6) Завербовать новичков  \U0001f479+";
                            msg += $"\n\n(7) Захватить власть в стране! Власть захвачена на {eWin}% \U0001F3C6+";
                            msg += $"\n\n(8) Отсидеться в секретной квартире. Осталось раз: {aprt}. \U000023f3+";
                            //выбираю задание
                            UserData.SetProperty<int>("choose", 0);
                            break;
                        case 1:                            
                        case 2:                           
                        case 3:
                            temp = choose;
                            msg += $"\n\n\U0001f4B0: {mission5[temp-1].capMis}\n\n_________________________________________________\n\nСложность - {mission5[temp-1].dif}.\n\nСколько \U0001f479 вы пошлете на задание? Выберите 0, чтобы вернуться к списку задач";
                            msg += "\n\nУ вас " + nm + " \U0001f479.";
                            UserData.SetProperty<int>("choose", temp);
                            break;
                        case 4:                            
                        case 5:
                            temp = choose;                   
                            switch (mission5[temp-1].dif)
                            {
                                case "легко":
                                    price = 50;
                                    break;
                                case "средне":
                                    price =45;
                                    break;
                                case "тяжело":
                                    price = 40;
                                    break;
                            }
                            msg += $"\n\n\U0001f46e: {mission5[temp-1].capMis}\n\n_________________________________________________\n\nСложность - {mission5[temp-1].dif}. Стоимость участия каждого \U0001f479 - {price}\U0001f4B0. \n\nСколько \U0001f479 вы пошлете на задание? Выберите 0, чтобы вернуться к списку задач.";
                            msg += "\n\nУ вас " + nm + " \U0001f479 " + "и " + mon + " \U0001f4B0";
                            UserData.SetProperty<int>("choose", temp);
                            break;
                        case 6:
                            msg += $"\n\nСтоимость найма одного \U0001f479 - 100 \U0001f4B0. При каждом найме \U0001f46e увеличивается на 3%. Сколько \U0001f479 вы хотите нанять? ";
                            msg += "\n\nУ вас " + nm + " \U0001f479, "+mon+ " \U0001f4B0  и " + awr + "%  \U0001f46e";
                            UserData.SetProperty<int>("choose", 6);
                            break;
                        case 7:
                            msg += $"\n\nВы уверены, что готовы захватить власть в стране? Это сложный и постепенный процесс! Сколько \U0001f479 вы хотите отправить? Стоимость участия каждого \U0001f479 - 150 \U0001f4B0";
                            msg += "\n\nВ настоящий момент страна захвачена на "+eWin+ "%, у вас " + nm + " \U0001f479, " + mon + " \U0001f4B0  и " + awr + "%  \U0001f46e";
                            UserData.SetProperty<int>("choose", 7);
                            break;
                        case 8:
                           
                            break;
                        case 9:
                            UserData.SetProperty<string>("ng", "new");
                            UserData.SetProperty<string>("l", "s");
                            if (eWin >= 100)
                            {
                                msg += "\n\nВся страна ваша, босс! Наслаждайтесь властью!";
                            }
                            else
                            {
                                msg += "\n\nВаша база была захвачена. Нажмите любую цифру для продолжения\n\n________\n\nПрисылайте ваши предложения, замечания, найденные баги - arfr@live.ru";
                            }
                            break;
                         default:
                            UserData.SetProperty<int>("choose", 0);
                            break;
                    }
                    UserData.SetProperty<List<Mission>>("missionM", missionM);
                    UserData.SetProperty<List<Mission>>("missionA", missionA);
                    UserData.SetProperty<Mission[]>("mission5", mission5);
                    UserData.SetProperty<string>("lose", lose);
                    UserData.SetProperty<int>("mon", mon);
                    UserData.SetProperty<int>("awr", awr);
                    UserData.SetProperty<int>("nm", nm);  
                    UserData.SetProperty<int>("eWin", eWin);
                    UserData.SetProperty<int>("aprt", aprt);
                }
                Activity reply = activity.CreateReply(msg);
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