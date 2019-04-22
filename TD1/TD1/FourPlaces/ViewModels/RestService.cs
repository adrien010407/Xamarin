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
using Acr.UserDialogs;

namespace TD1.FourPlaces.ViewModels
{
    class RestService : NotifierBase
    {
        HttpClient client;

        private IUserDialogs Dialogs { get; }

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
            Dialogs = UserDialogs.Instance;

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
            if (!no_refresh && tokens.ExpiresIn - tokens.DTime < 120)
            {
                await Refresh();
            }
            HttpRequestMessage request = new HttpRequestMessage(method, uri) {
                Content = content
            };
            try
            {
                if (tokens != null && tokens.AccessToken != null && tokens.TokenType != null)
                    request.Headers.Authorization = new AuthenticationHeaderValue(tokens.TokenType, tokens.AccessToken);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"error : {e.Message}, \ntoken : {tokens.AccessToken}");
            }
            return request;
        }

        public async Task<ObservableCollection<Place>> LoadPlaces(int days = 7, bool forceRefresh = false)
        {
            Places = new ObservableCollection<Place>();

            string RestUrl = "https://td-api.julienmialon.com/places";
            var uri = new Uri(string.Format(RestUrl, string.Empty));

            var request = await GetRequestAsync(HttpMethod.Get, uri);

            var content = string.Empty;

            //check if we are connected, else check to see if we have valid data
            if (!(Connectivity.NetworkAccess == NetworkAccess.Internet))
            {
                Debug.WriteLine("\n No internet \n");
                content = Barrel.Current.Get<string>(RestUrl);
            }
            else if (!forceRefresh && !Barrel.Current.IsExpired(RestUrl))
            {
                Debug.WriteLine("\n If internet \n");
                content = Barrel.Current.Get<string>(RestUrl);
            }

            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    var response = await client.SendAsync(request);
                    var tmpcontent = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode) //200
                    {
                        Barrel.Current.Add(RestUrl, tmpcontent, TimeSpan.FromDays(days));
                        content = tmpcontent;
                    }
                    else
                    {
                        Debug.WriteLine($"ERROR in LoadPlaces {response.StatusCode} : {tmpcontent}");
                    }
                }

                Response<ObservableCollection<Place>> value = JsonConvert.DeserializeObject<Response<ObservableCollection<Place>>>(content);
                if (value.IsSuccess && value.Data != null)
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
                        Debug.WriteLine("Location is null");
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
                Debug.WriteLine($"EXCEPTION in LoadPlaces : {ex.Message}");
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
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode || 
                    response.StatusCode == HttpStatusCode.BadRequest || 
                    response.StatusCode == HttpStatusCode.NotFound) //200, 400, 404
                {
                    Response<Place> value = JsonConvert.DeserializeObject<Response<Place>>(content);
                    if (value.IsSuccess && value.Data != null)
                    {
                        Place = value.Data;
                    }
                    else
                    {
                        Debug.WriteLine($"OK ERROR in LoadPlace {response.StatusCode} {value.ErrorCode} : {value.ErrorMessage}");
                        await Dialogs.AlertAsync($"Erreur pendant le chargment du lieu\n{value.ErrorMessage}", $"Erreur {response.StatusCode}, {value.ErrorCode}");
                    }
                }
                else
                {
                    Debug.WriteLine($"ERROR in LoadPlace {response.StatusCode} : {content}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION in LoadPlace {ex.Message}");
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
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound)
                    {
                        Response<Object> value = JsonConvert.DeserializeObject<Response<Object>>(content);
                        Debug.WriteLine($"OK ERROR in LoadImage {response.StatusCode} {value.ErrorCode}, {value.ErrorMessage}");
                    }
                    else
                    {
                        Debug.WriteLine($"ERROR in LoadImage {response.StatusCode}, {content}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION in LoadImage {ex.Message}");
            }

            return imageBytes;

        }

        public async Task<bool> AddPlace(Place place, MediaFile file)
        {
            bool placeAdded = false;
            ImageId imageId = (file != null) ? await PostImage(file) : new ImageId(1);
            if (imageId.Id == -1)
            {
                Debug.WriteLine($"ERROR in AddPlace : Image pas acceptée");
                await Dialogs.AlertAsync("Erreur durant l'update de l'image", "Erreur sur l'image");
                return false;
            }
            else
            {
                place.ImageId = imageId.Id;
            }

            string RestUrl = "https://td-api.julienmialon.com/places";
            var uri = new Uri(string.Format(RestUrl, string.Empty));

            StringContent placeJson = new StringContent(JsonConvert.SerializeObject(place), Encoding.UTF8, "application/json");

            var request = await GetRequestAsync(HttpMethod.Post, uri, placeJson);

            try
            {
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode || 
                    response.StatusCode == HttpStatusCode.BadRequest || 
                    response.StatusCode == HttpStatusCode.Unauthorized) //200, 400, 401
                {
                    Response<Object> value = JsonConvert.DeserializeObject<Response<Object>>(content);
                    if (value.IsSuccess)
                    {
                        placeAdded = true;
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized && value.ErrorCode == "GENERIC_HTTP_ERROR")
                    {
                        Debug.WriteLine($"OK ERROR in AddPlace {response.StatusCode} {value.ErrorCode} : {value.ErrorMessage}");
                        await Dialogs.AlertAsync($"Erreur durant l'ajout du lieu\nVous n'êtes pas connecté ou la connexion a été coupée.",
                            $"Erreur de connexion");
                    }
                    else
                    {
                        Debug.WriteLine($"OK ERROR in AddPlace {response.StatusCode} {value.ErrorCode} : {value.ErrorMessage}");
                        await Dialogs.AlertAsync($"Erreur durant l'ajout du lieu\n{value.ErrorMessage}", 
                            $"Erreur {response.StatusCode}, {value.ErrorCode}");
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
                if (response.IsSuccessStatusCode || 
                    response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.BadRequest ||
                    response.StatusCode == HttpStatusCode.NotFound) //200, 401, 400, 404
                {
                    Response<Object> value = JsonConvert.DeserializeObject<Response<Object>>(content);
                    if (value.IsSuccess)
                    {
                        commentAdded = true;
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized && value.ErrorCode == "GENERIC_HTTP_ERROR")
                    {
                        Debug.WriteLine($"OK ERROR in AddComment {response.StatusCode} {value.ErrorCode} : {value.ErrorMessage}");
                        await Dialogs.AlertAsync($"Erreur durant l'ajout d'un commentaire\nVous n'êtes pas connecté ou la connexion a été coupée.",
                            $"Erreur de connexion");
                    }
                    else
                    {
                        Debug.WriteLine($"OK ERROR in AddComment {response.StatusCode} {value.ErrorCode} : {value.ErrorMessage}");
                        await Dialogs.AlertAsync($"Erreur durant l'ajout d'un commentaire\n{value.ErrorMessage}",
                            $"Erreur {response.StatusCode}, {value.ErrorCode}");
                    }
                }
                else
                {
                    Debug.WriteLine($"ERROR in AddComment {response.StatusCode} : {content}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION in AddComment {ex.Message}");
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
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode ||
                    response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.BadRequest ||
                    response.StatusCode == HttpStatusCode.NotFound) //200, 401, 400, 404
                {
                    Response<ImageId> value = JsonConvert.DeserializeObject<Response<ImageId>>(content);
                    if (value.IsSuccess && value.Data != null)
                    {
                        imageId = value.Data;
                    }
                    else
                    {
                        Debug.WriteLine($"OK ERROR in PostImage {response.StatusCode} {value.ErrorCode} : {value.ErrorMessage}");
                    }
                }
                else
                {
                    Debug.WriteLine($"ERROR in PostImage {response.StatusCode} : {content}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION in PostImage {ex.Message}");
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
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode) //200
                {
                    Response<Tokens> value = JsonConvert.DeserializeObject<Response<Tokens>>(content);
                    if (value.IsSuccess && value.Data != null)
                    {
                        tokens = value.Data;
                        Barrel.Current.Add("tokens", JsonConvert.SerializeObject(tokens), TimeSpan.FromSeconds(tokens.ExpiresIn));
                        Author = person;
                        worked = true;
                    }
                } 
                else
                {
                    Debug.WriteLine($"ERROR in register {response.StatusCode} : {content}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION in register {ex.Message}");
            }

            return worked;
        }

        public async Task<bool> Login(AuthorLogin person)
        {
            bool worked = false;
            
            string RestUrl = "https://td-api.julienmialon.com/auth/login";
            var uri = new Uri(string.Format(RestUrl, string.Empty));

            StringContent personJson = new StringContent(JsonConvert.SerializeObject(person), Encoding.UTF8, "application/json");
            
            var request = await GetRequestAsync(HttpMethod.Get, uri, personJson);

            tokens = new Tokens();

            try
            {
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode ||
                    response.StatusCode == HttpStatusCode.NotFound) //200, 404
                {
                    Response<Tokens> value = JsonConvert.DeserializeObject<Response<Tokens>>(content);
                    if (value.IsSuccess && value.Data != null)
                    {
                        tokens = value.Data;
                        Barrel.Current.Add("tokens", JsonConvert.SerializeObject(tokens), TimeSpan.FromSeconds(tokens.ExpiresIn));
                        worked = true;

                        await GetMyInfo();
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        Debug.WriteLine($"OK ERROR in login {response.StatusCode} {value.ErrorCode} : {value.ErrorMessage}");
                        await Dialogs.AlertAsync("Le nom d'utilisateur ou le mot de passe entré est incorrecte", "Ce compte n'existe pas");
                    }
                }
                else
                {
                    Debug.WriteLine($"ERROR in login {response.StatusCode} : {content}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION in login {ex.Message}");
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
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode ||
                    response.StatusCode == HttpStatusCode.Unauthorized) //200, 401
                {
                    Response<Tokens> value = JsonConvert.DeserializeObject<Response<Tokens>>(content);
                    if (value.IsSuccess && value.Data != null)
                    {
                        tokens = value.Data;
                        worked = true;
                        await GetMyInfo();
                    } 
                    else if (response.StatusCode == HttpStatusCode.Unauthorized && value.ErrorCode == "GENERIC_HTTP_ERROR")
                    {
                        Debug.WriteLine($"OK ERROR in Refresh {response.StatusCode} {value.ErrorCode} : {value.ErrorMessage}\nConnection lost");
                    }
                }
                else
                {
                    Debug.WriteLine($"ERROR in Refresh {response.StatusCode} : {content}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION in Refresh {ex.Message}");
            }

            return worked;
        }

        public async Task<Author> GetMyInfo()
        {
            Author = new Author();

            string RestUrl = "https://td-api.julienmialon.com/me";
            var uri = new Uri(string.Format(RestUrl, string.Empty));

            var request = await GetRequestAsync(HttpMethod.Get, uri);

            try
            {
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode ||
                    response.StatusCode == HttpStatusCode.Unauthorized) //200, 401
                {
                    Response<Author> value = JsonConvert.DeserializeObject<Response<Author>>(content);
                    if (value.Data != null)
                    {
                        Author = value.Data;
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized && value.ErrorCode == "GENERIC_HTTP_ERROR")
                    {
                        Debug.WriteLine($"OK ERROR in getMyInfo {response.StatusCode} {value.ErrorCode} : {value.ErrorMessage}\nConnection lost");
                    }
                }
                else
                {
                    Debug.WriteLine($"ERROR in getMyInfo {response.StatusCode} : {content}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION in getMyInfo {ex.Message}");
            }

            return Author;
        }
    }
}
