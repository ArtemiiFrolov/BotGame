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
                    UserData.SetProperty<int>("eWin", 0); 
                    msg = "Здравствуйте, сэр! Вас приветствует консоль управления вашими подчиненными \U0001f479. Здесь вы можете посылать подчиненных на задания и вербовать новых \U0001f479.\n\nПомните: чем больше \U0001f479 вы пошлете на задание, тем выше вероятность его успешного завершения, но стоимость выполнения\U0001f4B0 или внимание властей \U0001f46e также повысится.\n\n Оставшиеся \U0001f479 будут защищать вашу базу. Помните: чем больше \U0001f46e, тем сложнее будет защитить базу. \n\n P.S. Чтобы начать новую игру - введите любой текст вместо цифр";
                }
                //выгрузка параметров (после того, как решено, новая игра или же нет)
                var lose = UserData.GetProperty<string>("lose");
                var mon = UserData.GetProperty<int>("mon");
                var awr = UserData.GetProperty<int>("awr");
                var nm = UserData.GetProperty<int>("nm");
                var choose = UserData.GetProperty<int>("choose");
                var eWin = UserData.GetProperty<int>("eWin");
                //проверка на проигрыш
                int number;
                bool result = Int32.TryParse(activity.Text, out number);
                if ((lose == "1") || ((!result) && (msg == "")))
                {
                    UserData.SetProperty<string>("ng", "new");
                    UserData.SetProperty<string>("l", "s");
                    if (eWin >= 100)
                    {
                        msg = "\n\nВся страна ваша, мой господин! Властвуйте и процветайте!";
                    }
                    else
                    {
                        msg = "\n\nВаша база была уничтожена. Нажмите любую цифру для продолжения\n\n________\n\nПомогите разнообразить проект - перейдите по этому опросу и предложите свои идеи, что может делать глава мафиозного клана\n\n https://goo.gl/forms/LyEDiywbR9ecn3ds1";
                    }
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
                                int buyM = mon - pAtt * 100;
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
                                        if (pDef < 0)
                                        {
                                            msg += "\n\nНедостаточно \U0001f479";
                                            choose = 1;
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
                                            if (pDef - deadPeople <= 0)
                                            {
                                                msg += $" Пытаясь вас защитить, погибли все({pDef}) защитники \U0001f479.";
                                                choose = 8;
                                            }
                                            else
                                            {
                                                msg += $" Наши парни никому не дали добраться до вас. Погибло {deadPeople} из {pDef} защитников.";
                                                cod = 0;
                                                switch (mission5[0].dif)
                                                {
                                                    case "легко":
                                                        awr1 = 5;
                                                        mon1 = 100;
                                                        cow = 30;
                                                        cod = 30 - 4 * pAtt;
                                                        break;
                                                    case "средне":
                                                        awr1 = 4;
                                                        mon1 = 500;
                                                        cow = 10;
                                                        cod = 40 - 2 * pAtt;
                                                        break;
                                                    case "тяжело":
                                                        awr1 = 3;
                                                        mon1 = 1200;
                                                        cow = 5;
                                                        cod = 50 - 2 * pAtt;
                                                        break;
                                                }
                                                nm = nm - deadPeople;
                                                deadPeople = 0;
                                                awr += awr1 * pAtt;

                                                var r = random.Next(100);
                                                if ((r < cow * pAtt) && (r < 90))
                                                {
                                                    mon += mon1;
                                                    msg += $"\n\n\U0001f479 выполнили миссию, награда - {mon1} \U0001f4B0.";
                                                }
                                                else
                                                {
                                                    msg += $"\n\n\U0001f479 провалили миссию.";
                                                }

                                                for (int i = 0; i < pAtt; i++)
                                                {
                                                    if (random.Next(100) < cod)
                                                    {
                                                        deadPeople++;
                                                    }
                                                }
                                                msg += $"\n\nНа задании погибло {deadPeople} \U0001f479.";
                                                nm = nm - deadPeople;
                                                mission5[0].doneflag = -1;
                                                missionM[mission5[0].id].doneflag = -1;
                                                choose = 0;
                                            }
                                        }
                                        break;
                                    case 2:
                                        if (pDef < 0)
                                        {
                                            msg += "\n\nНедостаточно \U0001f479";
                                            choose = 2;
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
                                            if (pDef - deadPeople <= 0)
                                            {
                                                msg += $" Пытаясь вас защитить, погибли все({pDef}) защитники \U0001f479.";
                                                choose = 8;
                                            }
                                            else
                                            {
                                                msg += $" Наши парни никому не дали добраться до вас. Погибло {deadPeople} из {pDef} защитников.";
                                                cod = 0;
                                                switch (mission5[1].dif)
                                                {
                                                    case "легко":
                                                        awr1 = 5;
                                                        mon1 = 100;
                                                        cow = 30;
                                                        cod = 30 - 4 * pAtt;
                                                        break;
                                                    case "средне":
                                                        awr1 = 4;
                                                        mon1 = 500;
                                                        cow = 10;
                                                        cod = 40 - 2 * pAtt;
                                                        break;
                                                    case "тяжело":
                                                        awr1 = 3;
                                                        mon1 = 1200;
                                                        cow = 5;
                                                        cod = 50 - 2 * pAtt;
                                                        break;
                                                }
                                                nm = nm - deadPeople;
                                                deadPeople = 0;
                                                awr += awr1 * pAtt;

                                                var r = random.Next(100);
                                                if ((r < cow * pAtt) && (r < 90))
                                                {
                                                    mon += mon1;
                                                    msg += $"\n\n\U0001f479 выполнили миссию, награда - {mon1} \U0001f4B0.";
                                                }
                                                else
                                                {
                                                    msg += $"\n\n\U0001f479 провалили миссию.";
                                                }

                                                for (int i = 0; i < pAtt; i++)
                                                {
                                                    if (random.Next(100) < cod)
                                                    {
                                                        deadPeople++;
                                                    }
                                                }
                                                msg += $"\n\nНа задании погибло {deadPeople} \U0001f479.";
                                                nm = nm - deadPeople;
                                                mission5[1].doneflag = -1;
                                                missionM[mission5[1].id].doneflag = -1;
                                                choose = 0;
                                            }
                                        }
                                        break;
                                    case 3:
                                        if (pDef < 0)
                                        {
                                            msg += "\n\nНедостаточно \U0001f479";
                                            choose = 3;
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
                                            if (pDef - deadPeople <= 0)
                                            {
                                                msg += $" Пытаясь вас защитить, погибли все({pDef}) защитники \U0001f479.";
                                                choose = 8;
                                            }
                                            else
                                            {
                                                msg += $" Наши парни никому не дали добраться до вас. Погибло {deadPeople} из {pDef} защитников.";
                                                cod = 0;
                                                switch (mission5[2].dif)
                                                {
                                                    case "легко":
                                                        awr1 = 5;
                                                        mon1 = 100;
                                                        cow = 30;
                                                        cod = 30 - 4 * pAtt;
                                                        break;
                                                    case "средне":
                                                        awr1 = 4;
                                                        mon1 = 500;
                                                        cow = 10;
                                                        cod = 40 - 2 * pAtt;
                                                        break;
                                                    case "тяжело":
                                                        awr1 = 3;
                                                        mon1 = 1200;
                                                        cow = 5;
                                                        cod = 50 - 2 * pAtt;
                                                        break;
                                                }
                                                nm = nm - deadPeople;
                                                deadPeople = 0;
                                                awr += awr1 * pAtt;

                                                var r = random.Next(100);
                                                if ((r < cow * pAtt) && (r < 90))
                                                {
                                                    mon += mon1;
                                                    msg += $"\n\n\U0001f479 выполнили миссию, награда - {mon1} \U0001f4B0.";
                                                }
                                                else
                                                {
                                                    msg += $"\n\n\U0001f479 провалили миссию.";
                                                }

                                                for (int i = 0; i < pAtt; i++)
                                                {
                                                    if (random.Next(100) < cod)
                                                    {
                                                        deadPeople++;
                                                    }
                                                }
                                                msg += $"\n\nНа задании погибло {deadPeople} \U0001f479.";
                                                nm = nm - deadPeople;
                                                mission5[2].doneflag = -1;
                                                missionM[mission5[2].id].doneflag = -1;
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
                                                cod = 20 - 4 * pAtt;
                                                price = 50;
                                                break;
                                            case "средне":
                                                awr1 = 25;
                                                mon1 = 25;
                                                cow = 10;
                                                cod = 30 - 2 * pAtt;
                                                price = 45;
                                                break;
                                            case "тяжело":
                                                awr1 = 40;
                                                mon1 = 40;
                                                cow = 5;
                                                cod = 40 - 2 * pAtt;
                                                price = 40;
                                                break;
                                        }
                                        if ((pDef < 0) || (mon - price * pAtt < 0))
                                        {
                                            msg += "\n\nНе хватает \U0001f479 или \U0001f4B0.";
                                            choose = 4;
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
                                                msg += $" Пытаясь вас защитить, погибли все({pDef}) защитники \U0001f479.";
                                                choose = 8;
                                            }
                                            else
                                            {
                                                msg += $" Наши парни никому не дали добраться до вас. Погибло {deadPeople} из {pDef} защитников.";
                                                nm = nm - deadPeople;
                                                deadPeople = 0;
                                                mon -= price * pAtt;

                                                var r = random.Next(100);
                                                if ((r < cow * pAtt) && (r < 90))
                                                {
                                                    awr -= awr1;
                                                    if (awr < 0)
                                                    {
                                                        awr = 0;
                                                    }
                                                    msg += $"\n\n\U0001f479 выполнили миссию, снижение - {awr1}% \U0001f46e";
                                                }
                                                else
                                                {
                                                    msg += $"\n\n\U0001f479 провалили миссию.";
                                                }

                                                for (int i = 0; i < pAtt; i++)
                                                {
                                                    if (random.Next(100) < cod)
                                                    {
                                                        deadPeople++;
                                                    }
                                                }
                                                msg += $"\n\nНа задании погибло {deadPeople} \U0001f479.";
                                                nm = nm - deadPeople;
                                                mission5[3].doneflag = -1;
                                                missionA[mission5[3].id].doneflag = -1;
                                                choose = 0;
                                            }
                                        }
                                        break;
                                    case 5:
                                        switch (mission5[4].dif)
                                        {
                                            case "легко":
                                                awr1 = 25;
                                                mon1 = 15;
                                                cow = 30;
                                                cod = 20 - 4 * pAtt;
                                                price = 50;
                                                break;
                                            case "средне":
                                                awr1 = 40;
                                                mon1 = 25;
                                                cow = 10;
                                                cod = 30 - 2 * pAtt;
                                                price = 45;
                                                break;
                                            case "тяжело":
                                                awr1 = 60;
                                                mon1 = 40;
                                                cow = 5;
                                                cod = 40 - 2 * pAtt;
                                                price = 40;
                                                break;
                                        }
                                        if ((pDef < 0) || (mon - price * pAtt < 0))
                                        {
                                            msg += "\n\nНе хватает \U0001f479 или \U0001f4B0";
                                            choose = 5;
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
                                                msg += $" Пытаясь вас защитить, погибли все ({pDef}) защитники \U0001f479.";
                                                choose = 8;
                                            }
                                            else
                                            {
                                                msg += $" Наши парни никому не дали добраться до вас. Погибло {deadPeople} из {pDef} защитников.";
                                                nm = nm - deadPeople;
                                                deadPeople = 0;
                                                mon -= price * pAtt;

                                                var r = random.Next(100);
                                                if ((r < cow * pAtt) && (r < 90))
                                                {
                                                    awr -= awr1;
                                                    if (awr < 0)
                                                    {
                                                        awr = 0;
                                                    }
                                                    msg += $"\n\n\U0001f479 выполнили миссию, снижение - {awr1}% \U0001f46e";
                                                }
                                                else
                                                {
                                                    msg += $"\n\n\U0001f479 провалили миссию.";
                                                }

                                                for (int i = 0; i < pAtt; i++)
                                                {
                                                    if (random.Next(100) < cod)
                                                    {
                                                        deadPeople++;
                                                    }
                                                }
                                                msg += $"\n\nНа задании погибло {deadPeople} \U0001f479.";
                                                nm = nm - deadPeople;
                                                mission5[4].doneflag = -1;
                                                missionA[mission5[4].id].doneflag = -1;
                                                choose = 0;
                                            }
                                        }
                                        break;
                                    case 6:
                                        if (mon - pAtt * 100 < 0)
                                        {
                                            msg += "\n\nУ вас нет столько \U0001f4B0.";
                                            choose = 6;
                                        }
                                        else
                                        {
                                            mon = mon - pAtt * 100;
                                            nm = nm + pAtt;
                                            awr += 5;
                                            choose = 0;
                                        }
                                        break;
                                    case 7:
                                        deadPeople = 0;
                                        if ((pDef < 0) || (mon - 150 * pAtt < 0))
                                        {
                                            msg += "\n\nУ вас нет столько \U0001f479 или \U0001f4B0";
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
                                                if (random.Next(100) < 50 - 2 * pAtt)
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
                                                choose = 8;
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
                                                    msg += $" Пытаясь вас защитить, погибли все({pDef}) защитники \U0001f479.";
                                                    choose = 8;
                                                }
                                                else
                                                {
                                                    msg += $" Наши парни никому не дали добраться до вас. Погибло {deadPeople} из {pDef} защитников.";
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
                   
                    string s1 = ".";
                    
                    switch (choose)
                    {                        
                        //основное меню
                        case 0:
                            msg += "\n\n_____________________________________________\n\n\U0001f4B0: " + mon + " | | | \U0001f479: " + nm + " | | | \U0001f46e: " + awr + "% \n\n_____________________________________________";
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
                                msg += $"\n\n \U0001f4B0: {i + 1}. {mission5[i].shortMis} ({mission5[i].dif}) ";
                            for (int i = 3; i < 5; i++)
                                msg += $"\n\n \U0001f46e: {i + 1}. {mission5[i].shortMis} ({mission5[i].dif}) ";
                            msg += $"\n\n \U0001f479 6. Завербовать новичков";
                            msg += $"\n\n \U0001f479 7. Захватить власть в стране! Власть захвачена на {eWin}% ";
                            //выбираю задание
                            UserData.SetProperty<int>("choose", 0);
                            break;
                        case 1:
                            msg += $"\n\n\U0001f4B0: {mission5[0].capMis}. \n\nСложность - {mission5[0].dif}. Сколько \U0001f479 вы пошлете на задание? Выберите 0, чтобы вернуться к списку задач";
                            msg += "\n\nУ вас " + nm + " \U0001f479.";
                            UserData.SetProperty<int>("choose", 1);
                            break;
                        case 2:
                            msg += $"\n\n\U0001f4B0: {mission5[1].capMis}. \n\nСложность - {mission5[1].dif}. Сколько \U0001f479 вы пошлете на задание? Выберите 0, чтобы вернуться к списку задач";
                            msg += "\n\nУ вас " + nm + " \U0001f479.";
                            UserData.SetProperty<int>("choose", 2);
                            break;
                        case 3:
                            msg += $"\n\n\U0001f4B0: {mission5[2].capMis}. \n\nСложность - {mission5[2].dif}. Сколько \U0001f479 вы пошлете на задание? Выберите 0, чтобы вернуться к списку задач";
                            msg += "\n\nУ вас " + nm + " \U0001f479.";
                            UserData.SetProperty<int>("choose", 3);
                            break;
                        case 4:                            
                            switch (mission5[3].dif)
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
                            msg += $"\n\n\U0001f46e: {mission5[3].capMis}. Сложность - {mission5[3].dif}. Стоимость участия каждого \U0001f479 - {price}\U0001f4B0. Сколько \U0001f479 вы пошлете на задание? Выберите 0, чтобы вернуться к списку задач";
                            msg += "\n\nУ вас " + nm + " \U0001f479 " + "и "+mon+ " \U0001f4B0";
                            UserData.SetProperty<int>("choose", 4);
                            break;
                        case 5:                           
                            switch (mission5[4].dif)
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
                            msg += $"\n\n\U0001f46e: {mission5[4].capMis}. Сложность - {mission5[4].dif}. Стоимость участия каждого \U0001f479 - {price}\U0001f4B0. Сколько \U0001f479 вы пошлете на задание? Выберите 0, чтобы вернуться к списку задач";
                            msg += "\n\nУ вас " + nm + " \U0001f479 " + "и " + mon + " \U0001f4B0";
                            UserData.SetProperty<int>("choose", 5);
                            break;
                        case 6:
                            msg += $"\n\nСтоимость найма одного \U0001f479 - 100 \U0001f4B0. При каждом найме \U0001f46e увеличивается на 5%. Сколько \U0001f479 вы хотите нанять? ";
                            msg += "\n\nУ вас " + nm + " \U0001f479 и " + awr + "%  \U0001f46e";
                            UserData.SetProperty<int>("choose", 6);
                            break;
                        case 7:
                            msg += $"\n\nВы уверены, что готовы захватить власть в стране? Это очень сложный процесс! Сколько \U0001f479 вы хотите отправить? Стоимость участия каждого \U0001f479 - 150 \U0001f4B0";
                            msg += "\n\nВ настоящий момент страна захвачена на "+eWin+"%, у вас " + nm + " \U0001f479 и " + awr + "% \U0001f46e";
                            UserData.SetProperty<int>("choose", 7);
                            break;
                        case 8:
                            UserData.SetProperty<string>("ng", "new");
                            UserData.SetProperty<string>("l", "s");
                            if (eWin >= 100)
                            {
                                msg = "\n\nВся страна ваша, мой господин! Властвуйте и процветайте!";
                            }
                            else
                            {
                                msg = "\n\nВаша база была уничтожена. Нажмите любую цифру для продолжения";
                            }
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