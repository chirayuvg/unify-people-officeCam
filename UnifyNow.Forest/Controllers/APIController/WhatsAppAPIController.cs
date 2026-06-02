using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Runtime.InteropServices;

namespace UnifyNow.Forest.Controllers.APIController
{
    [Route("api/node")]
    [ApiController]
    public class WhatsAppAPIController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private static Process? _nodeProcess3 = null;
        private static Process? _nodeProcess2 = null;
        private readonly string logFilePath = "/var/log/myBashSession.log"; // Path inside Docker


        private const string nodeApiUrl = "http://localhost:3000";
        static string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Find the "UnifyNow.Forest" folder dynamically
        static string projectFolderName = "UnifyNow.Forest";
        static string projectDirectory = FindProjectDirectory(currentDirectory, projectFolderName);

        // If running from a subfolder (like bin\Debug), move up to the project root
        
        public WhatsAppAPIController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        static string FindProjectDirectory(string startPath, string folderName)
        {
            DirectoryInfo dir = new DirectoryInfo(startPath);

            while (dir != null)
            {
                if (dir.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase))
                {
                    return dir.FullName; // Return the correct project path
                }
                dir = dir.Parent; // Move up one level
            }

            return null; // Return null if not found
        }


        [HttpGet("start")]
        public ActionResult<string> StartNodeServer()
        {
            try
            {
                if (_nodeProcess2 == null || _nodeProcess2.HasExited)
                {
                    // Path to Node.js executable (assuming it's available in system PATH)
                    string nodePath = "node";
                    string pm2Path =  "pm2";


                    // Navigate to the root solution directory
                    string projectDirectory = Directory.GetCurrentDirectory();

                    // Path to the server.js script in the root solution folder
                    string scriptPath = Path.Combine(projectDirectory, "server.js");

                    //Ensure file exists
                    if (!System.IO.File.Exists(scriptPath))
                    {
                        return NotFound($"Node.js script not found at {scriptPath}");
                    }

                    // Start the Node.js server
                    //ProcessStartInfo psi = new ProcessStartInfo
                    //{
                    //    FileName = nodePath,
                    //    Arguments = $"\"{scriptPath}\"",
                    //    UseShellExecute = true,  // Open CMD window
                    //    CreateNoWindow = false   // Keep CMD visible
                    //};

                    //RunCommand($"cd /d \"{projectDirectory}\" && {pm2Path} delete whatsapp-server");
                    //RunCommand($"cd /d \"{projectDirectory}\")");
                    //RunCommand($"{pm2Path} start \"{scriptPath}\" --name \"whatsapp-server\" --attach");
                    //RunCommand($"{pm2Path} save");
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Console.WriteLine("Detected OS: Linux");
                        RunCommandsAsRoot(pm2Commands_Linux);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        Console.WriteLine("Detected OS: Windows");
                        RunCommandsAsRoot(pm2Commands_Windows);
                    }
                    //ProcessStartInfo psi2 = new ProcessStartInfo
                    //{
                    //    FileName = "cmd.exe",  // Ensure pm2 is in the system PATH
                    //    Arguments = $"/c cd /d \"{projectDirectory}\" && pm2 start \"{scriptPath}\" --name \"whatsapp-server\" --attach",
                    //    UseShellExecute = true,  // Opens CMD window
                    //    CreateNoWindow = false   // Keeps CMD window visible
                    //};


                    //ProcessStartInfo psi3 = new ProcessStartInfo
                    //{
                    //    FileName = pm2Path,
                    //    Arguments = $"/k save",
                    //    UseShellExecute = true,  // Open CMD window
                    //    CreateNoWindow = false   // Keep CMD visible
                    //};


                    //_nodeProcess = new Process { StartInfo = psi };
                    //_nodeProcess.Start();
                    //_nodeProcess2 = new Process { StartInfo = psi2 };
                    //_nodeProcess2.Start();
                    //_nodeProcess3 = new Process { StartInfo = psi3 };
                    //_nodeProcess3.Start();

                }

                return Ok("Node.js server started successfully! Please scan the QR code in the command window to start interating by whatsapp");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error starting Node.js server: " + ex.Message);
            }
        }

        string[] pm2Commands_Linux = {
    "pkill -f " + Path.Combine(projectDirectory, "node"),
    "cd " + " "+projectDirectory + " && npm install --prefix " + projectDirectory,
    "pkill -f " + Path.Combine(projectDirectory, "node"),
    "cd " + Path.Combine(projectDirectory, "node_modules", "pm2", "bin") + " && ./pm2 start " + Path.Combine(projectDirectory, "server.js") + " --name whatsapp-server --attach"
};


        string[] pm2Commands_Windows = {
            "taskkill /F /IM" + " " + Path.Combine(projectDirectory, "node.exe"),
            "cd /d " + " "+ projectDirectory +  " "+"&& npm install --prefix" + " " + projectDirectory,
            "taskkill /F /IM" + " " + Path.Combine(projectDirectory, "node.exe"),
             "cd /d " + " "+ Path.Combine(projectDirectory, "node_modules", "pm2", "bin") + " " +"&& pm2 start " + " "+Path.Combine(projectDirectory, "server.js") + " --name whatsapp-server --attach",
        };

        string[] stopCommands_Windows = {
            "cd /d " + " "+ Path.Combine(projectDirectory, "node_modules", "pm2", "bin") + " "+" && pm2 stop whatsapp-server",
            "cd /d " + " "+ Path.Combine(projectDirectory, "node_modules", "pm2", "bin") + " "+" && pm2 delete whatsapp-server",
            "cd /d " + " "+ projectDirectory + " "+" && rmdir /s /q .wwebjs_auth",
            "cd /d " + " "+ projectDirectory + " "+" && rmdir /s /q .wwebjs_cache"

        };

        string[] stopCommands_Linux = {
    "cd " + Path.Combine(projectDirectory, "node_modules", "pm2", "bin") + " && ./pm2 stop whatsapp-server",
    "cd " + Path.Combine(projectDirectory, "node_modules", "pm2", "bin") + " && ./pm2 delete whatsapp-server",
    "cd " + projectDirectory + " && rm -rf .wwebjs_auth",
    "cd " + projectDirectory + " && rm -rf .wwebjs_cache"
};

        static void RunCommandsAsRoot(string[] commands)
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

            foreach (string command in commands)
            {
                ProcessStartInfo psi = new ProcessStartInfo();

                if (isWindows)
                {
                    psi.FileName = "cmd.exe";
                    psi.Arguments = $"/k {command}"; // Opens a new admin CMD window
                    psi.Verb = "runas"; // Run as Administrator
                    psi.UseShellExecute = true;
                    psi.CreateNoWindow = false;

                }
                else if (isDocker)  // In Docker, run command without sudo
                {
                    psi.FileName = "/bin/bash";
                    psi.Arguments = $"-c \"mkdir -p /var/log && touch /var/log/myBashSession.log && chmod 666 /var/log/myBashSession.log && {command} | tee -a /var/log/myBashSession.log\"";
                    psi.UseShellExecute = false;  // REQUIRED for redirection & process control
                    psi.CreateNoWindow = true;    // TRUE because Docker containers don't have a GUI
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;


                }
                else
                {
                    psi.FileName = "/bin/bash";
                    psi.Arguments = $"-c \"sudo mkdir -p /var/log && sudo touch /var/log/myBashSession.log && sudo chmod 666 /var/log/myBashSession.log && {command} | tee -a /var/log/myBashSession.log\"";
                    psi.UseShellExecute = false;  // REQUIRED for redirection & process control
                    psi.CreateNoWindow = true;    // TRUE because Docker containers don't have a GUI
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;

                }


                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                }
            }
        }
        static void RunCommandsSequentially(string[] commands)
        {
            foreach (string command in commands)
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Verb = "runas",
                    Arguments = $"/k {command}",  // Use /k to keep CMD open after execution
                    UseShellExecute = true,  // Open CMD window
                    CreateNoWindow = false   // Keep CMD visible
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    //process.WaitForExit();  // Ensures one command finishes before running the next
                }
            }
        }

        //static void RunCommand(string[] commands)
        //{
        //    string fullCommand = string.Join(" && ", commands);

        //    ProcessStartInfo psi = new ProcessStartInfo
        //    {
        //        FileName = "cmd.exe",
        //        Arguments = $" {fullCommand}",
        //        //RedirectStandardOutput = true,
        //        //RedirectStandardError = true,
        //        UseShellExecute = true,
        //        CreateNoWindow = false  // Set to true if you want a hidden execution
        //    };

        //    using (Process process = new Process { StartInfo = psi })
        //    {
        //        process.Start();

        //       // string output = process.StandardOutput.ReadToEnd();
        //       // string error = process.StandardError.ReadToEnd();
        //       // process.WaitForExit();

        //       // Console.WriteLine(output);
        //        //if (!string.IsNullOrEmpty(error))
        //        //{
        //        //    Console.WriteLine("Error: " + error);
        //        //}
        //    }
        //}


        [HttpGet("GetLogs")]
        public IActionResult GetLogs()
        {
            if (!System.IO.File.Exists(logFilePath))
            {
                return NotFound("Log file not found.");
            }

            var logs = System.IO.File.ReadAllText(logFilePath);
            return Content(logs, "text/plain");
        }


        [HttpGet("GetQR")]
        public IActionResult GetQR()
        {
            string qrPath = Path.Combine(projectDirectory, "qr.png");

            if (!System.IO.File.Exists(qrPath))
            {
                return NotFound("QR code not found.");
            }

            var fileStream = new FileStream(qrPath, FileMode.Open, FileAccess.Read);
            return File(fileStream, "image/png");
        }



        [HttpGet("stop")]
        public ActionResult<string> StopNodeServer()
        {


            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Console.WriteLine("Detected OS: Linux");
                    RunCommandsAsRoot(stopCommands_Linux);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Console.WriteLine("Detected OS: Windows");
                    RunCommandsAsRoot(stopCommands_Windows);
                }

                //if (_nodeProcess2 != null && !_nodeProcess2.HasExited)
                //{
                //    _nodeProcess2.Kill();
                //    _nodeProcess2.Dispose();
                //    _nodeProcess2 = null;


                //}
                //else if (_nodeProcess3 != null && !_nodeProcess3.HasExited)
                //{
                //    _nodeProcess3.Kill();
                //    _nodeProcess3.Dispose();
                //    _nodeProcess3 = null;
                //}
                return Ok("Node.js server stopped.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error stopping Node.js server: " + ex.Message);
            }
        }


        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages()
        {
            var response = await _httpClient.GetAsync($"{nodeApiUrl}/messages");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to fetch messages");
            }

            var messages = await response.Content.ReadAsStringAsync();
            return Content(messages, "application/json");
        }

        [HttpPost("send-text")]
        public async Task<IActionResult> SendMessage([FromBody] WhatsAppMessageRequest request)
        {
            if (string.IsNullOrEmpty(request.Number) || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Number and Message are required");
            }

            var response = await _httpClient.PostAsJsonAsync($"{nodeApiUrl}/send-message", request);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to send message");
            }

            return Ok("Message sent successfully");
        }

        [HttpPost("send-media")]
        public async Task<IActionResult> SendMedia([FromForm] WhatsAppMediaRequest request)
        {
            if (string.IsNullOrEmpty(request.Number) || request.Media == null)
            {
                return BadRequest("Number and media file are required");
            }

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(request.Number), "number");
            if (!string.IsNullOrEmpty(request.Caption))
            {
                content.Add(new StringContent(request.Caption), "caption");
            }

            using var fileStream = request.Media.OpenReadStream();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(request.Media.ContentType);
            content.Add(streamContent, "media", request.Media.FileName);

            var response = await _httpClient.PostAsync($"{nodeApiUrl}/send-media", content);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to send media");
            }

            return Ok("Media sent successfully");
        }


        [HttpPost("send-current-location")]
        public async Task<IActionResult> SendCurrentLocation([FromBody] WhatsAppLocationRequest request)
        {
            if (string.IsNullOrEmpty(request.Number) || request.Latitude == null || request.Longitude == null)
            {
                return BadRequest("Number, latitude, and longitude are required.");
            }

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(request.Number), "number");
            content.Add(new StringContent(request.Latitude.ToString()), "latitude");
            content.Add(new StringContent(request.Longitude.ToString()), "longitude");


            var url = $"{nodeApiUrl}/send-current-location?latitude={request.Latitude}&longitude={request.Longitude}&number={request.Number}";

            var response = await _httpClient.PostAsJsonAsync($"{nodeApiUrl}/send-current-location", request);
            //var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to send location");
            }

            return Ok("Location sent successfully");
        }

    }

    public class WhatsAppLocationRequest
    {
        public string Number { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }


    public class WhatsAppMessageRequest
    {
        public string Number { get; set; }
        public string Message { get; set; }
    }

    //  Data Model for Sending Media Messages
    public class WhatsAppMediaRequest
    {
        public string Number { get; set; }
        public string Caption { get; set; }
        public IFormFile Media { get; set; }
    }
}
