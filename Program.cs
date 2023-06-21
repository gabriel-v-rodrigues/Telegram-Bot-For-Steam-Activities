using System.Text.Json.Serialization;
using System.Text.Json;
using System.Web;
using Newtonsoft.Json;
using Telegram.Bot;

namespace TelegramBotForSteamActivity
{
    class Program
    {

        //CONFIG:
        //Insert steamApiKey
        public static string steamApiKey = "paste here";
        //Insert telegram token api
        public static string token = "paste here";
        public static object telegramBotClient = new TelegramBotClient(token);
        //Insert the chat id where the bot should send the messages
        public static long chatId = -0;
        //Insert the steamID here
        public static string steamId = "76561198942077537";

        //Should create a poll?
        public static bool pollAllow = true;

        public static string questionPoll = "What do you think about it?";
        public static string[] optionsPoll = new string[] { "Nice one bro!", "WTF?", "Invite me bro plz" };

        //Final message that the bot should send
        public static string Finalmessage(string name, string game)
        {
            string msg = "";
            Random random = new Random();

            //Add more options here
            int option = random.Next(random.Next(0, 1));
            switch (option)
            {
                case 0: msg = "⚠️ The Player <b>" + name + "</b> is Playing <b>" + game + "</b>"; break;
                case 1: msg = "⚠️ ALERT: <b>" + name.ToUpper() + "</b> WAS FOUND PLAYING THE GAME <b>" + game.ToUpper() + "</b> 😱 ⚠️"; break;

            }
            //end changing options

            msg = HttpUtility.UrlEncode(msg);
            return msg;
        }

        //ACTUAL CODE:

        static async Task Main(string[] args)
        {
            Task task = Task.Run(() => SteamAlert(chatId, steamId));
            await task;
        }

        //Task to realize all the work
        public static async Task SteamAlert(long chatId, string steamId)
        {
            
            bool IsPlaying = false;

            while (true)
            {

                try
                {
                    // Calling API
                    HttpClient httpClient = new HttpClient();
                    string jsonResult = await httpClient.GetStringAsync(apiUrl(steamId));
                    TimeSpan currentTime = DateTime.Now.TimeOfDay;

                    // Deserealizing JSON to a SteamUserSummary object  
                    SteamUserSummary userSummary = JsonConvert.DeserializeObject<SteamUserSummary>(jsonResult);

                    // Acessing callback info
                    string name = userSummary.Response.Players[0].PersonaName;
                    name = name.ToLower();

                    if (IsPlaying == false){

                        // If the user is playing:
                        if (userSummary.Response.Players[0].GameExtraInfo != null && userSummary.Response.Players[0].GameExtraInfo != "Soundpad") {
                            string game = userSummary.Response.Players[0].GameExtraInfo;
                            string message;
                            message = Finalmessage(name, game);
                            string url = $"https://api.telegram.org/bot{token}/sendMessage?chat_id={chatId}&text={message}&parse_mode=html";
                            using (var client = new HttpClient())
                            {
                                var response = client.GetAsync(url).Result;
                                IsPlaying = true;

                                if (response.IsSuccessStatusCode){
                                    Console.WriteLine("Message send with Success!");
                                    Thread.Sleep(15000);
                                    if (pollAllow) { Telegrampoll(token, chatId); }
                                }
                                else {
                                    Console.WriteLine("An error was occured!");
                                }
                            }
                        }
                        else {
                            Console.WriteLine("Waiting off: " + currentTime + " User: " + name);
                            Thread.Sleep(20000);
                            Console.Clear();
                        }

                    }
                    else {
                        if (userSummary.Response.Players[0].GameExtraInfo != null){
                            Console.WriteLine("Waiting on: " + currentTime + " User: " + name);
                            Thread.Sleep(20000);
                            Console.Clear();
                        }
                        else { IsPlaying = false; }
                    }
                }
                // Return any errors
                catch (Exception e) { Console.WriteLine("error: " + e.Message); }
            }
        }

        //Create a telegram poll
        public static void Telegrampoll(string token, long chatId)
        {

            var configJson = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };

            var config = System.Text.Json.JsonSerializer.Serialize(optionsPoll);
            config = HttpUtility.UrlEncode(config);
            string url = $"https://api.telegram.org/bot{token}/sendPoll?chat_id={chatId}&questionPoll={questionPoll}&optionsPoll={config}&allows_multiple_answers=true&is_anonymous=false";

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Poll created!");
                }
                else
                {
                    Console.WriteLine("An error was ocurred while creating the pool: " + response.Content.ReadAsStringAsync().Result);
                }
            }
        }

        // Steam URL API
        public static string apiUrl(string steamId) { string x = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={steamApiKey}&steamids={steamId}"; return x; }

    }

    public class TelegramAnswer
    {
        public string text { get; set; }
        public int option { get; set; }
    }

    public class SteamUserSummary
    {
        public SteamUserResponse Response { get; set; }
    }

    public class SteamUserResponse
    {
        public SteamUser[] Players { get; set; }
    }

    public class SteamUser
    {
        public string PersonaName { get; set; }
        public string? GameExtraInfo { get; set; }
    }

}