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
                    var temp = JsonConvert.DeserializeObject<MissionCollection>(File.ReadAllText(p));
                    missionM = temp.dataM;
                    missionA = temp.dataA;
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
                    msg = "Здравствуйте, уважаемый злобный гений! Вас приветствует консоль управления вашими подчиненными. Здесь вы можете посылать злыдней на задания, либо вербовать новых злыдней.\n\nПомните, что чем больше злыдней вы пошлете на задание, тем выше вероятность его успешного завершения, но стоимость выполнения/разыскиваемость также повысится. Оставшиеся злыдни будут защищать вашу базу. Помните также, что чем больше ваша разыскиваемость, тем сложнее будет защитить базу и потребуется больше злыдней для ее защиты. \n\n P.S. Чтобы запустить протокол самоуничтожения - введите любой текст вместо цифр";
                }
                //выгрузка параметров (после того, как решено, новая игра или же нет)
                var lose = UserData.GetProperty<string>("lose");
                var mon = UserData.GetProperty<int>("mon");
                var awr = UserData.GetProperty<int>("awr");
                var nm = UserData.GetProperty<int>("nm");
                var choose = UserData.GetProperty<int>("choose");
                //проверка на проигрыш
                int number;
                bool result = Int32.TryParse(activity.Text, out number);
                if ((lose == "1") || ((!result) && (msg == "")))
                {
                    UserData.SetProperty<string>("ng", "new");
                    UserData.SetProperty<string>("l", "s");
                    msg = "\n\nВаша база была уничтожена. Нажмите любую цифру для продолжения";
                }
                //основная часть игры
                else
                {
                    int price = 0;
                    if ((choose == 0) && (msg == ""))
                    {
                        choose = Convert.ToInt32(activity.Text);
                    }
                    else
                    {
                        int pAtt = Convert.ToInt32(activity.Text);
                        int pDef= nm - pAtt;
                        int buyM=mon- pAtt*100;
                        int cod = 0;
                        int awr1=0;
                        int mon1 = 0;
                        int cow = 0;
                        int deadPeople=0;                        
                        switch (choose)
                        {                            
                            case 1:
                                if (pDef < 0)
                                {
                                    msg += "\n\nУ вас нет столько злыдней, чтобы отправить их на миссию.";
                                    choose = 1;
                                }
                                else {
                                    msg += $"\n\nНа вашей базе осталось {pDef} злыдней.";
                                    cod = awr - 4 * pDef;
                                    for (int i = 0; i < pDef; i++)
                                    {
                                        var random = new Random();
                                        if (random.Next(100) < cod)
                                        {
                                            deadPeople++;
                                        }
                                    }
                                    if (pDef - deadPeople < 0)
                                    {
                                        msg += $" Пытаясь вас защитить, погибли все ваши злыдни.";
                                        choose = 8;
                                    }
                                    else
                                    {
                                        msg += $" При отражении атаки погибло {deadPeople} злыдней, но ваша база выстояла.";
                                        cod = 0;
                                        switch (mission5[0].dif)
                                        {
                                            case "легко":
                                                awr1 = 5;
                                                mon1 = 100;
                                                cow = 30;
                                                cod = 10 - 2 * pAtt;
                                                break;
                                            case "средне":
                                                awr1 = 4;
                                                mon1 = 500;
                                                cow = 10;
                                                cod = 30 - 3 * pAtt;
                                                break;
                                            case "тяжело":
                                                awr1 = 3;
                                                mon1 = 1200;
                                                cow = 5;
                                                cod = 40 - 2 * pAtt;
                                                break;
                                        }
                                        nm = nm - deadPeople;
                                        deadPeople = 0;
                                        awr += awr1 * pAtt;
                                        var random = new Random();
                                        if (random.Next(100) < (cow * pAtt))
                                        {
                                            mon += mon1;
                                            msg += $"\n\nВаши злыдни выполнили миссию, ваша награда - {mon1}.";
                                        }
                                        else
                                        {
                                            msg += $"\n\nВаши злыдни провалили миссию.";
                                        }
                                        
                                        for (int i=0;i<pAtt; i++)
                                        {                                            
                                            if (random.Next(100) < cod)
                                            {
                                                deadPeople++;
                                            }
                                        }
                                        msg += $"\n\nПри выполнении миссии погибло - {deadPeople} злыдней.";
                                        nm = nm - deadPeople;
                                        mission5[0].doneflag = -1;
                                        choose = 0;
                                    }                                    
                                }                               
                                break;
                            case 2:
                                if (pDef < 0)
                                {
                                    msg += "\n\nУ вас нет столько злыдней, чтобы отправить их на миссию.";
                                    choose = 2;
                                }
                                else
                                {
                                    msg += $"\n\nНа вашей базе осталось {pDef} злыдней.";
                                    cod = awr - 4 * pDef;
                                    for (int i = 0; i < pDef; i++)
                                    {
                                        var random = new Random();
                                        if (random.Next(100) < cod)
                                        {
                                            deadPeople++;
                                        }
                                    }
                                    if (pDef - deadPeople < 0)
                                    {
                                        msg += $" Пытаясь вас защитить, погибли все ваши злыдни.";
                                        choose = 8;
                                    }
                                    else
                                    {
                                        msg += $" При отражении атаки погибло {deadPeople} злыдней, но ваша база выстояла.";
                                        cod = 0;
                                        switch (mission5[1].dif)
                                        {
                                            case "легко":
                                                awr1 = 5;
                                                mon1 = 100;
                                                cow = 30;
                                                cod = 10 - 2 * pAtt;
                                                break;
                                            case "средне":
                                                awr1 = 4;
                                                mon1 = 500;
                                                cow = 10;
                                                cod = 30 - 3 * pAtt;
                                                break;
                                            case "тяжело":
                                                awr1 = 3;
                                                mon1 = 1200;
                                                cow = 5;
                                                cod = 40 - 2 * pAtt;
                                                break;
                                        }
                                        nm = nm - deadPeople;
                                        deadPeople = 0;
                                        awr += awr1 * pAtt;
                                        var random = new Random();
                                        if (random.Next(100) < (cow * pAtt))
                                        {
                                            mon += mon1;
                                            msg += $"\n\nВаши злыдни выполнили миссию, ваша награда - {mon1}.";
                                        }
                                        else
                                        {
                                            msg += $"\n\nВаши злыдни провалили миссию.";
                                        }

                                        for (int i = 0; i < pAtt; i++)
                                        {
                                            if (random.Next(100)< cod)
                                            {
                                                deadPeople++;
                                            }
                                        }
                                        msg += $"\n\nПри выполнении миссии погибло - {deadPeople} злыдней.";
                                        nm= nm - deadPeople;
                                        mission5[1].doneflag = -1;
                                        choose = 0;
                                    }
                                }
                                break;
                            case 3:
                                if (pDef < 0)
                                {
                                    msg += "\n\nУ вас нет столько злыдней, чтобы отправить их на миссию.";
                                    choose = 3;
                                }
                                else
                                {
                                    msg += $"\n\nНа вашей базе осталось {pDef} злыдней.";
                                    cod = awr - 4 * pDef;
                                    for (int i = 0; i < pDef; i++)
                                    {
                                        var random = new Random();
                                        if (random.Next(100) < cod)
                                        {
                                            deadPeople++;
                                        }
                                    }
                                    if (pDef - deadPeople < 0)
                                    {
                                        msg += $" Пытаясь вас защитить, погибли все ваши злыдни.";
                                        choose = 8;
                                    }
                                    else
                                    {
                                        msg += $" При отражении атаки погибло {deadPeople} злыдней, но ваша база выстояла.";
                                        cod = 0;
                                        switch (mission5[2].dif)
                                        {
                                            case "легко":
                                                awr1 = 5;
                                                mon1 = 100;
                                                cow = 30;
                                                cod = 10 - 2 * pAtt;
                                                break;
                                            case "средне":
                                                awr1 = 4;
                                                mon1 = 500;
                                                cow = 10;
                                                cod = 30 - 3 * pAtt;
                                                break;
                                            case "тяжело":
                                                awr1 = 3;
                                                mon1 = 1200;
                                                cow = 5;
                                                cod = 40 - 2 * pAtt;
                                                break;
                                        }
                                        nm = nm - deadPeople;
                                        deadPeople = 0;
                                        awr += awr1 * pAtt;
                                        var random = new Random();
                                        if (random.Next(100) < (cow * pAtt))
                                        {
                                            mon += mon1;
                                            msg += $"\n\nВаши злыдни выполнили миссию, ваша награда - {mon1}.";
                                        }
                                        else
                                        {
                                            msg += $"\n\nВаши злыдни провалили миссию.";
                                        }

                                        for (int i = 0; i < pAtt; i++)
                                        {
                                            if (random.Next(100) < cod)
                                            {
                                                deadPeople++;
                                            }
                                        }
                                        msg += $"\n\nПри выполнении миссии погибло - {deadPeople} злыдней.";
                                        nm = nm - deadPeople;
                                        mission5[2].doneflag = -1;
                                        choose = 0;
                                    }
                                }
                                break;
                            case 4:
                                switch (mission5[3].dif)
                                {
                                    case "легко":
                                        awr1 = 15;
                                        mon1 = 15;
                                        cow = 30;
                                        cod = 10 - 2 * pAtt;
                                        price = 100;
                                        break;
                                    case "средне":
                                        awr1 = 25;
                                        mon1 = 25;
                                        cow = 10;
                                        cod = 30 - 3 * pAtt;
                                        price = 150;
                                        break;
                                    case "тяжело":
                                        awr1 = 40;
                                        mon1 = 40;
                                        cow = 5;
                                        cod = 40 - 2 * pAtt;
                                        price = 200;
                                        break;
                                }
                                if ((pDef < 0)||(mon-price*pAtt<0))
                                {
                                    msg += "\n\nУ вас нет столько злыдней или не хватает денег, чтобы начать миссию.";
                                    choose = 4;
                                }
                                else
                                {
                                    int cod1 = awr - 4 * pDef; 
                                    msg += $"\n\nНа вашей базе осталось {pDef} злыдней.";                                    
                                    for (int i = 0; i < pDef; i++)
                                    {
                                        var random = new Random();
                                        if (random.Next(100) < cod1)
                                        {
                                            deadPeople++;
                                        }
                                    }
                                    if (pDef - deadPeople < 0)
                                    {
                                        msg += $" Пытаясь вас защитить, погибли все ваши злыдни.";
                                        choose = 8;
                                    }
                                    else
                                    {
                                        msg += $" При отражении атаки погибло {deadPeople} злыдней, но ваша база выстояла.";                                        
                                        nm = nm - deadPeople;
                                        deadPeople = 0;
                                        mon -= price* pAtt;
                                        var random = new Random();
                                        if (random.Next(100) < (cow * pAtt))
                                        {
                                            awr -= awr1;
                                            msg += $"\n\nВаши злыдни выполнили миссию, снижение вашей разыскиваемости - {awr1}";
                                        }
                                        else
                                        {
                                            msg += $"\n\nВаши злыдни провалили миссию.";
                                        }

                                        for (int i = 0; i < pAtt; i++)
                                        {
                                            if (random.Next(100) < cod)
                                            {
                                                deadPeople++;
                                            }
                                        }
                                        msg += $"\n\nПри выполнении миссии погибло - {deadPeople} злыдней.";
                                        nm = nm - deadPeople;
                                        mission5[3].doneflag = -1;
                                        choose = 0;
                                    }
                                }
                                break;
                            case 5:
                                switch (mission5[4].dif)
                                {
                                    case "легко":
                                        awr1 = 15;
                                        mon1 = 15;
                                        cow = 30;
                                        cod = 10 - 2 * pAtt;
                                        price = 100;
                                        break;
                                    case "средне":
                                        awr1 = 25;
                                        mon1 = 25;
                                        cow = 10;
                                        cod = 30 - 3 * pAtt;
                                        price = 150;
                                        break;
                                    case "тяжело":
                                        awr1 = 40;
                                        mon1 = 40;
                                        cow = 5;
                                        cod = 40 - 2 * pAtt;
                                        price = 200;
                                        break;
                                }
                                if ((pDef < 0) || (mon - price * pAtt < 0))
                                {
                                    msg += "\n\nУ вас нет столько злыдней или не хватает денег, чтобы начать миссию.";
                                    choose = 5;
                                }
                                else
                                {
                                    int cod1 = awr - 4 * pDef;
                                    msg += $"\n\nНа вашей базе осталось {pDef} злыдней.";
                                    for (int i = 0; i < pDef; i++)
                                    {
                                        var random = new Random();
                                        if (random.Next(100) < cod1)
                                        {
                                            deadPeople++;
                                        }
                                    }
                                    if (pDef - deadPeople < 0)
                                    {
                                        msg += $" Пытаясь вас защитить, погибли все ваши злыдни.";
                                        choose = 8;
                                    }
                                    else
                                    {
                                        msg += $" При отражении атаки погибло {deadPeople} злыдней, но ваша база выстояла.";
                                        nm = nm - deadPeople;
                                        deadPeople = 0;
                                        mon -= price * pAtt;
                                        var random = new Random();
                                        if (random.Next(100) < (cow * pAtt))
                                        {
                                            awr -= awr1;
                                            msg += $"\n\nВаши злыдни выполнили миссию, снижение вашей разыскиваемости - {awr1}";
                                        }
                                        else
                                        {
                                            msg += $"\n\nВаши злыдни провалили миссию.";
                                        }

                                        for (int i = 0; i < pAtt; i++)
                                        {
                                            if (random.Next(100) < cod)
                                            {
                                                deadPeople++;
                                            }
                                        }
                                        msg += $"\n\nПри выполнении миссии погибло - {deadPeople} злыдней.";
                                        nm = nm - deadPeople;
                                        mission5[4].doneflag = -1;
                                        choose = 0;
                                    }
                                }
                                break;
                            case 6:
                                break;
                            case 7:
                                break;

                        }                        
                    }
                   
                    string s1 = ".";
                    
                    switch (choose)
                    {                        
                        //основное меню
                        case 0:
                            msg += "\n\n \n\nКоличество денег: " + mon + "\n\n Количество злыдней: " + nm + "\n\n Ваша разыскиваемость: " + awr + "%";
                            //заполняю строчки заданиями                            
                            for (var k = 0; k < 3; k++)
                            {
                                int k1 = k + 1;
                                mission5[k] = missionM.FirstOrDefault(t => t.doneflag == k1);
                                if (mission5[k] == null)
                                {
                                    var random = new Random();
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
                                    var random = new Random();
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
                            msg += "\n\nПолучение денег:";                       
                            for (int i = 0; i < 3; i++)
                                msg += $"\n\n {i + 1}. {mission5[i].shortMis} ({mission5[i].dif}) - {missionM[mission5[i].id].doneflag.ToString()}";
                            msg += "\n\nУменьшение разыскиваемости:";
                            for (int i = 3; i < 5; i++)
                                msg += $"\n\n {i + 1}. {mission5[i].shortMis} ({mission5[i].dif}) - {missionA[mission5[i].id].doneflag.ToString()}";
                            //выбираю задание
                            UserData.SetProperty<int>("choose", 0);
                            break;
                        case 1:
                            msg += $"\n\nМиссия по заработку денег:\n\n{mission5[0].capMis}. Сложность задачи - {mission5[0].dif}. Сколько злыдней вы хотите послать на выполнение задачи? Выберите 0, чтобы вернуться к списку задач";
                            msg += "\n\nВ настоящий момент у вас " + nm + " злыдней";
                            UserData.SetProperty<int>("choose", 1);
                            break;
                        case 2:
                            msg += $"\n\nМиссия по заработку денег:\n\n{mission5[1].capMis}. Сложность задачи - {mission5[1].dif}. Сколько злыдней вы хотите послать на выполнение задачи? Выберите 0, чтобы вернуться к списку задач";
                            msg += "\n\nВ настоящий момент у вас " + nm + " злыдней";
                            UserData.SetProperty<int>("choose", 2);
                            break;
                        case 3:
                            msg += $"\n\nМиссия по заработку денег:\n\n{mission5[2].capMis}. Сложность задачи - {mission5[2].dif}. Сколько злыдней вы хотите послать на выполнение задачи? Выберите 0, чтобы вернуться к списку задач";
                            msg += "\n\nВ настоящий момент у вас " + nm + " злыдней";
                            UserData.SetProperty<int>("choose", 3);
                            break;
                        case 4:                            
                            switch (mission5[3].dif)
                            {
                                case "легко":
                                    price = 100;
                                    break;
                                case "средне":
                                    price = 200;
                                    break;
                                case "тяжело":
                                    price = 300;
                                    break;
                            }
                            msg += $"\n\nМиссия по уменьшению разыскиваемости:\n\n{mission5[3].capMis}. Сложность задачи - {mission5[3].dif}. Стоимость участия каждого злыдня - {price} Сколько злыдней вы хотите послать на выполнение задачи? Выберите 0, чтобы вернуться к списку задач";
                            msg += "\n\nВ настоящий момент у вас " + nm + " злыдней"+ "и"+mon+" денег: ";
                            UserData.SetProperty<int>("choose", 4);
                            break;
                        case 5:                           
                            switch (mission5[4].dif)
                            {
                                case "легко":
                                    price = 100;
                                    break;
                                case "средне":
                                    price = 150;
                                    break;
                                case "тяжело":
                                    price = 200;
                                    break;
                            }
                            msg += $"\n\nМиссия по уменьшению разыскиваемости:\n\n{mission5[4].capMis}. Сложность задачи - {mission5[4].dif}. Стоимость участия каждого злыдня - {price} Сколько злыдней вы хотите послать на выполнение задачи? Выберите 0, чтобы вернуться к списку задач";
                            msg += "\n\nВ настоящий момент у вас " + nm + " злыдней" + "и" + mon + " денег: ";
                            UserData.SetProperty<int>("choose", 5);
                            break;
                        case 6:
                            msg += $"\n\nСтоимость найма одного злыдня - 100. При найме каждого злыдня ваша разыскиваемость увеличивается на 5%. Сколько злыдней вы хотите нанять? ";
                            msg += "\n\nВ настоящий момент у вас " + nm + " злыдней\n\n Ваша текущая разыскиваемость: " + awr + "%";
                            UserData.SetProperty<int>("choose", 6);
                            break;
                        case 7:
                            msg += $"\n\nВы уверены, что готовы захватить мир? Это очень сложный процесс, для него необходимо большое количество денег и миньонов!";
                            msg += "\n\nВ настоящий момент у вас " + nm + " злыдней\n\n Ваша текущая разыскиваемость: " + awr + "%";
                            UserData.SetProperty<int>("choose", 7);
                            break;
                        case 8:
                            UserData.SetProperty<string>("lose", "1");                            
                            break;

                    }
                    UserData.SetProperty<List<Mission>>("missionM", missionM);
                    UserData.SetProperty<List<Mission>>("missionA", missionA);
                    UserData.SetProperty<Mission[]>("mission5", mission5);
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