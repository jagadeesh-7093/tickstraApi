using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace jagadeeshApi.Controllers
{
    public class TickStarController : Controller
    {
        // GET: TickStar
        public ActionResult Index()
        {
            return View();
        }

        // POST: /TickStar/Login
        [HttpPost]
        public async Task<ActionResult> Login(string UserName, string Password)
        {
            // Call the OAuth 2.0 API asynchronously
            var tokenResponse = await GetAccessTokenAsync(UserName, Password);

            if (tokenResponse != null)
            {
                Session["Token"] = tokenResponse;  // Store token in session
                //Session["Message"] = "Login successful!";
                return RedirectToAction("FileUpload");
            }
            else
            {
                Session["Message"] = "Invalid username or password.";
            }

            return View("Index");
        }

        // Call the OAuth 2.0 API
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task<string> GetAccessTokenAsync(string UserName, string Password)
        {
            string url = "https://auth.test.galaxygw.com/oauth2/token"; // Replace with your token URL

            var authHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{UserName}:{Password}"));

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", authHeaderValue);

            // Option 2: Use client_id and client_secret in the form data (if needed)
            var postData = new StringContent(
                "grant_type=client_credentials&scope=your-scope",  // Add your optional scope here
                Encoding.UTF8,
                "application/x-www-form-urlencoded"
            );

            try
            {
                // Make the POST request to get the access token
                HttpResponseMessage response = await httpClient.PostAsync(url, postData);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Token Response: " + jsonResponse);

                    // Parse the JSON response to get the access token
                    var tokenData = JsonConvert.DeserializeObject<TokenResponse>(jsonResponse);
                    return tokenData?.AccessToken;
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Error Details: " + errorDetails);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return null;
            }
        }
        public ActionResult FileUpload()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            // Check if a file was uploaded
            if (file != null && file.ContentLength > 0)
            {
                // Define the path to save the uploaded file
                var path = Path.Combine(Server.MapPath("~/UploadedFiles"), Path.GetFileName(file.FileName));

                // Save the file to the specified path
                file.SaveAs(path);

                // Read the content of the uploaded file
                string xmlContent = System.IO.File.ReadAllText(path);

                // Optionally, you can parse the XML string to ensure it's well-formed (uncomment if needed)
                // XElement.Parse(xmlContent); // This will throw an exception if the XML is not well-formed.

                // Set the XML content in ViewBag to display
                ViewBag.XmlContent = xmlContent;
                ViewBag.Message = "File uploaded successfully!";

                // Optionally, you could return a different view that specifically handles displaying XML
                return View("DisplayXml"); // Change to your desired view name
            }
            else
            {
                // Set an error message if no file was selected
                ViewBag.Message = "No file selected. Please choose a file to upload.";
            }

            // Return to the FileUpload view
            return View("FileUpload");
        }


        // Token response class to parse JSON response
        public class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("expires_in")]
            public string ExpiresIn { get; set; }
        }
        
    }
}
