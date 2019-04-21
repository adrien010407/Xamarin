using Newtonsoft.Json;
using Storm.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TD1.FourPlaces.Models;
using Xamarin.Forms.Maps;
using Xamarin.Essentials;
using System.Net.Http.Headers;
using MonkeyCache.FileStore;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.IO;

namespace TD1.FourPlaces.ViewModels
{
    class RestService : NotifierBase
    {
        HttpClient client;


        public Tokens tokens = new Tokens();
        private Author _author = new Author();

        public Author Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        public bool Connected
        {
            get => tokens.Connected;
        }

        public ObservableCollection<Place> Places { get; private set; }

        private static RestService _rest;
        public static RestService Rest {
            get {
                if (_rest == null)
                {
                    _rest = new RestService();
                }
                return _rest;
            }
        }

        private RestService()
        {
            client = new HttpClient();

            string cache_tokens = Barrel.Current.Get<string>("tokens");
            if (cache_tokens != null)
            {
                try
                {
                    tokens = JsonConvert.DeserializeObject<Tokens>(cache_tokens);
                } 
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        private async Task<HttpRequestMessage> GetRequestAsync(HttpMethod method, Uri uri, HttpContent content = null, bool no_refresh = true)
        {
            Debug.WriteLine("line 58");
            if (!no_refresh && tokens.ExpiresIn - tokens.DTime < 120)
            {
                await Refresh();
            }
            Debug.WriteLine("line 63");
            HttpRequestMessage request = new HttpRequestMessage(method, uri) {
                Content = content
            };
            Debug.WriteLine("line 67");
            try
            {   Debug.WriteLine("line 69"); Debug.WriteLine("tokens : {0}", new string[] { tokens.ToString() });
                if (tokens != null && tokens.AccessToken != null && tokens.TokenType != null)
                    request.Headers.Authorization = new AuthenticationHeaderValue(tokens.TokenType, tokens.AccessToken); Debug.WriteLine("line 70");
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"error : {0}, \ntoken : {1}", new string[] { e.Message, tokens.AccessToken });
            }
            return request;
        }

        public async Task<ObservableCollection<Place>> LoadPlaces(int days = 7, bool forceRefresh = false)
        {
            Places = new ObservableCollection<Place>();
            Debug.WriteLine("line 82");
            string RestUrl = "https://td-api.julienmialon.com/places";
            var uri = new Uri(string.Format(RestUrl, string.Empty));
            var request = await GetRequestAsync(HttpMethod.Get, uri);
            Debug.WriteLine("line 86");
            var content = string.Empty;

            Debug.WriteLine("\n entree \n");

            //check if we are connected, else check to see if we have valid data
            if (!(Connectivity.NetworkAccess == NetworkAccess.Internet))
            {
                Debug.WriteLine("\n no internet \n");
                content = Barrel.Current.Get<string>(RestUrl);
            }
            else if (!forceRefresh && !Barrel.Current.IsExpired(RestUrl))
            {
                Debug.WriteLine("\n if internet \n");
                content = Barrel.Current.Get<string>(RestUrl);
            }

            Debug.WriteLine("\n sortie \n");

            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode) //200
                    {
                        content = await response.Content.ReadAsStringAsync();
                        Barrel.Current.Add(RestUrl, content, TimeSpan.FromDays(days));
                    }
                }

                Response<ObservableCollection<Place>> value = JsonConvert.DeserializeObject<Response<ObservableCollection<Place>>>(content);
                if (value.Data != null)
                {
                    Xamarin.Essentials.Location location = null;
                    try
                    {
                        var geo_request = new GeolocationRequest(GeolocationAccuracy.Best);
                        location = await Geolocation.GetLastKnownLocationAsync();
                        location = await Geolocation.GetLocationAsync(geo_request);
                    }
                    catch (FeatureNotSupportedException fnsEx)
                    {
                        // Handle not supported on device exception
                        Debug.WriteLine(fnsEx);
                    }
                    catch (PermissionException pEx)
                    {
                        // Handle permission exception
                        Debug.WriteLine(pEx);
                    }
                    catch (Exception ex)
                    {
                        // Unable to get location
                        Debug.WriteLine(ex);
                    }

                    if (location == null)
                    {
                        Debug.WriteLine("location null");
                        Places = value.Data;
                    }
                    else
                    {
                        Places = new ObservableCollection<Place>(
                            value.Data.OrderBy(
                                e => Xamarin.Essentials.Location.CalculateDistance(
                                    location,
                                    new Xamarin.Essentials.Location(e.Latitude, e.Longitude),
                                    DistanceUnits.Kilometers
                                )
                            )
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"				ERROR in LoadPlaces : {0}", new string[] { ex.Message });
            }


            return Places;
        }

        public async Task<Place> LoadPlace(long placeId)
        {
            Place Place = new Place();

            string RestUrl = "https://td-api.julienmialon.com/places/" + placeId.ToString();
            var uri = new Uri(string.Format(RestUrl, string.Empty));
            var request = await GetRequestAsync(HttpMethod.Get, uri);

            try
            {
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode) //200
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Response<Place> value = JsonConvert.DeserializeObject<Response<Place>>(content);
                    if (value.Data != null)
                    {
                        Place = value.Data;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"				ERROR in LoadPlace {0}", new string[] { ex.Message });
            }

            return Place;
        }

        public async Task<byte[]> LoadImage(long ImageId)
        {
            byte[] imageBytes = new byte[0];

            string RestUrl = "https://td-api.julienmialon.com/images/" + ImageId.ToString();
            
            var uri = new Uri(string.Format(RestUrl, string.Empty));
            var request = await GetRequestAsync(HttpMethod.Get, uri);

            try
            {
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode) //200
                {
                    imageBytes = await response.Content.ReadAsByteArrayAsync();
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Response<Object> value = JsonConvert.DeserializeObject<Response<Object>>(content);
                    Debug.WriteLine(@"				ERROR in LoadImage {0}, {1}", new string[] { value.ErrorCode, value.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"				ERROR in LoadImage {0}", new string[] { ex.Message });
            }

            return imageBytes;

        }

        public async Task<bool> AddPlace(Place place, MediaFile file)
        {
            bool placeAdded = false;
            Debug.WriteLine($"file : {file}");
            ImageId imageId = (file != null) ? await PostImage(file) : new ImageId(1);
            if (imageId.Id == -1)
            {
                Debug.WriteLine(@"ERROR in AddPlace {0}", new string[] { "Image pas acceptée" });
                return false;
            }
            else
            {
                place.ImageId = imageId.Id;
            }

            string RestUrl = "https://td-api.julienmialon.com/places";

            var uri = new Uri(string.Format(RestUrl, string.Empty));
            StringContent placeJson = new StringContent(JsonConvert.SerializeObject(place), Encoding.UTF8, "application/json");
            Debug.WriteLine($"place : {JsonConvert.SerializeObject(place)}");
            var request = await GetRequestAsync(HttpMethod.Post, uri, placeJson);

            try
            {
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest) //200
                {
                    Response<Object> value = JsonConvert.DeserializeObject<Response<Object>>(content);
                    if (value.IsSuccess)
                    {
                        placeAdded = true;
                    }
                    else
                    {
                        Debug.WriteLine($"OK ERROR in AddPlace {value.ErrorCode} : {value.ErrorMessage}");
                    }
                }
                else
                {
                    Debug.WriteLine($"ERROR in AddPlace {response.StatusCode} : {content}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION in AddPlace {ex.Message}");
            }

            return placeAdded;
        }

        public async Task<bool> AddComment(long placeId, string comment)
        {
            bool commentAdded = false;

            string RestUrl = "https://td-api.julienmialon.com/places/" + placeId + "/comments";

            var uri = new Uri(string.Format(RestUrl, string.Empty));
            StringContent commentJson = new StringContent(JsonConvert.SerializeObject(new Comment(comment)), Encoding.UTF8, "application/json");
            var request = await GetRequestAsync(HttpMethod.Post, uri, commentJson);

            try
            {
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                Response<Object> value = JsonConvert.DeserializeObject<Response<Object>>(content);
                if (response.IsSuccessStatusCode) //200
                {
                    if (value.IsSuccess)
                    {
                        commentAdded = true;
                    }
                    else
                    {
                        Debug.WriteLine(@"ERROR in AddComment {0}", new string[] { value.ErrorMessage });
                    }
                }
                else
                {
                    Debug.WriteLine(@"ERROR in AddComment {0}", new string[] { value.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"ERROR in AddComment {0}", new string[] { ex.Message });
            }

            return commentAdded;
        }

        public async Task<ImageId> PostImage(MediaFile file)
        {
            ImageId imageId = new ImageId();
            
            
            var imgContent = new MultipartFormDataContent();

            byte[] imageBytes;
            using (var memoryStream = new MemoryStream())
            {
                file.GetStream().CopyTo(memoryStream);
                imageBytes = memoryStream.ToArray();
            }
            var imageContent = new ByteArrayContent(imageBytes);
            if (file.Path.EndsWith(".jpg") || file.Path.EndsWith(".jpeg"))
            {
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

                // Le deuxième paramètre doit absolument être "file" ici sinon ça ne fonctionnera pas
                imgContent.Add(imageContent, "file", "file.jpg");
            }
            else if (file.Path.EndsWith(".png"))
            {
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");

                // Le deuxième paramètre doit absolument être "file" ici sinon ça ne fonctionnera pas
                imgContent.Add(imageContent, "file", "file.png");
            }

            string RestUrl = "https://td-api.julienmialon.com/images";

            var uri = new Uri(string.Format(RestUrl, string.Empty));
            var request = await GetRequestAsync(HttpMethod.Post, uri, imgContent);

            try
            {
                Debug.WriteLine("Line 359"); var response = await client.SendAsync(request);
                Debug.WriteLine("Line 360"); var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(@"Line 361 {0}", new string[] { content }); Response<ImageId> value = JsonConvert.DeserializeObject<Response<ImageId>>(content);
                Debug.WriteLine("Line 362"); if (response.IsSuccessStatusCode) //200
                {
                    if (value.IsSuccess && value.Data != null)
                    {
                        imageId = value.Data;
                    }
                    else
                    {
                        Debug.WriteLine(@"				ERROR1 in PostImage {0}", new string[] { value.ErrorMessage });
                    }
                }
                else
                {
                    Debug.WriteLine(@"				ERROR2 in PostImage {0}", new string[] { value.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"				ERROR3 in PostImage {ex.Message}");
            }

            return imageId;
        }

        public async Task<bool> Register(Author person)
        {
            bool worked = false;

            string RestUrl = "https://td-api.julienmialon.com/auth/register";
            var uri = new Uri(string.Format(RestUrl, string.Empty));
            StringContent personJson = new StringContent(JsonConvert.SerializeObject(person), Encoding.UTF8, "application/json");
            var request = await GetRequestAsync(HttpMethod.Get, uri, personJson);

            tokens = new Tokens();

            try
            {
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode) //200
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Response<Tokens> value = JsonConvert.DeserializeObject<Response<Tokens>>(content);
                    if (value.IsSuccess && value.Data != null)
                    {
                        tokens = value.Data;
                        Barrel.Current.Add("tokens", JsonConvert.SerializeObject(tokens), TimeSpan.FromSeconds(tokens.ExpiresIn));
                        Author = person;
                        worked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"				ERROR in register {0}", new string[] { ex.Message });
            }

            return worked;
        }

        public async Task<bool> Login(AuthorLogin person)
        {
            bool worked = false;

            Debug.WriteLine("login");
            string RestUrl = "https://td-api.julienmialon.com/auth/login";
            var uri = new Uri(string.Format(RestUrl, string.Empty));
            StringContent personJson = new StringContent(JsonConvert.SerializeObject(person), Encoding.UTF8, "application/json");
            Debug.WriteLine(personJson.ReadAsStringAsync().Result);
            var request = await GetRequestAsync(HttpMethod.Get, uri, personJson);

            tokens = new Tokens();

            try
            {
                var response = await client.SendAsync(request);
                Debug.WriteLine(response.StatusCode);
                if (response.IsSuccessStatusCode) //200
                {
                    Debug.WriteLine("sucess");
                    var content = await response.Content.ReadAsStringAsync();
                    Response<Tokens> value = JsonConvert.DeserializeObject<Response<Tokens>>(content);
                    if (value.IsSuccess && value.Data != null)
                    {
                        tokens = value.Data;
                        Barrel.Current.Add("tokens", JsonConvert.SerializeObject(tokens), TimeSpan.FromSeconds(tokens.ExpiresIn));
                        Debug.WriteLine(@"token : {0}", new string[] { tokens.AccessToken });
                        worked = true;
                        await GetMyInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"				ERROR in login {0}", new string[] { ex.Message });
            }

            return worked;
        }

        public async Task<bool> Refresh()
        {
            bool worked = false;
            
            string RestUrl = "https://td-api.julienmialon.com/auth/refresh";
            var uri = new Uri(string.Format(RestUrl, string.Empty));
            StringContent refTokenJson = new StringContent(JsonConvert.SerializeObject(new RefreshToken(tokens.RefreshToken)), Encoding.UTF8, "application/json");
            var request = await GetRequestAsync(HttpMethod.Get, uri, refTokenJson, true);

            tokens = new Tokens();

            try
            {
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode) //200
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Response<Tokens> value = JsonConvert.DeserializeObject<Response<Tokens>>(content);
                    if (value.IsSuccess && value.Data != null)
                    {
                        tokens = value.Data;
                        worked = true;
                        await GetMyInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"				ERROR in Refresh {0}", new string[] { ex.Message });
            }

            return worked;
        }

        public async Task<Author> GetMyInfo()
        {
            Debug.WriteLine("me");
            Author = new Author();

            string RestUrl = "https://td-api.julienmialon.com/me";
            var uri = new Uri(string.Format(RestUrl, string.Empty));
            var request = await GetRequestAsync(HttpMethod.Get, uri);

            try
            {
                var response = await client.SendAsync(request);
                Debug.WriteLine(response.StatusCode);
                if (response.IsSuccessStatusCode) //200
                {
                    Debug.WriteLine("success");
                    var content = await response.Content.ReadAsStringAsync();
                    Response<Author> value = JsonConvert.DeserializeObject<Response<Author>>(content);
                    if (value.Data != null)
                    {
                        Author = value.Data;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"				ERROR in getMyInfo {0}", new string[] { ex.Message });
            }

            return Author;
        }
    }
}
