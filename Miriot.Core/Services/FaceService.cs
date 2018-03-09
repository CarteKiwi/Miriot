using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Miriot.Common;
using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rectangle = Microsoft.ProjectOxford.Common.Rectangle;

namespace Miriot.Core.Services
{
    public class FaceService : IFaceService
    {
        private string _defaultApiRoot = "https://northeurope.api.cognitive.microsoft.com/face/v1.0";
        private string _oxfordFaceKey;
        private string _oxfordEmotionKey;

        private readonly IConfigurationService _configurationService;
        private string _miriotPersonGroupId;
        private bool _isInitialized;
        private FaceServiceClient _faceClient;

        public FaceService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized) return;

            var config = await _configurationService.GetKeysByProviderAsync("cs");
            _oxfordFaceKey = config["face"];
            _oxfordEmotionKey = config["emotion"];

            _faceClient = new FaceServiceClient(_oxfordFaceKey, _defaultApiRoot);

            await LoadGroup();

            _isInitialized = true;
        }

        private async Task LoadGroup()
        {
            try
            {
                // Récupère les groupes
                var groups = await _faceClient.ListPersonGroupsAsync();

                if (!groups.Any())
                {
                    // Première utilisation de l'api
                    await _faceClient.CreatePersonGroupAsync("1", "Miriot");
                    await _faceClient.TrainPersonGroupAsync(_miriotPersonGroupId);
                }
                else
                {
                    // Récupère l'id du premier groupe
                    _miriotPersonGroupId = groups.FirstOrDefault()?.PersonGroupId;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task<ServiceResponse> GetUsers(byte[] bitmap)
        {
            if (!_isInitialized)
            {
                await InitializeAsync();

                if (_miriotPersonGroupId == null)
                {
                    await CreatePerson(bitmap, "Guillaume");
                    return new ServiceResponse(null, ErrorType.UnknownFace);
                }
            }

            List<Face> faces;
            Debug.WriteLine("DetectAsync started");
            // Détection des visages sur la photo
            using (var stream = new MemoryStream(bitmap))
                faces = (await _faceClient.DetectAsync(stream)).ToList();
            Debug.WriteLine("DetectAsync ended");

            // Récupération des identifiants Oxford
            var faceIds = faces.Select(o => o.FaceId).ToList();

            if (!faceIds.Any())
                return new ServiceResponse(null, ErrorType.NoFaceDetected);
            Debug.WriteLine("IdentityAsync started");

            // Identification des personnes à partir des visages
            var identifyResults = (await _faceClient.IdentifyAsync(_miriotPersonGroupId, faces.Select(o => o.FaceId).ToArray())).ToList();
            Debug.WriteLine("IdentityAsync ended");

            // Visage inconnu du groupe
            if (identifyResults.Count == 0 || !identifyResults.Any(o => o.Candidates.Any()))
                return new ServiceResponse(null, ErrorType.UnknownFace);

            // Une fois les personnes identifiées, on ne garde que les 2 mieux reconnues
            var mostConfidentPersons = identifyResults.SelectMany(p => p.Candidates).OrderByDescending(o => o.Confidence).Take(2);
            Debug.WriteLine("GetPerson started");

            var users = new List<User>();
            foreach (var mostConfidentPerson in mostConfidentPersons)
            {
                var person = await _faceClient.GetPersonAsync(_miriotPersonGroupId, mostConfidentPerson.PersonId);
                Debug.WriteLine("GetPerson ended");

                var faceId = identifyResults.First(r => r.Candidates.Select(c => c.PersonId).Contains(mostConfidentPerson.PersonId)).FaceId;
                var face = faces.Single(o => o.FaceId == faceId);

                var data = TryDeserialize(person.UserData);

                var user = new User
                {
                    Id = person.PersonId,
                    Name = person.Name,
                    UserData = data,
                    FaceRectangle = new Rectangle
                    {
                        Height = face.FaceRectangle.Height,
                        Width = face.FaceRectangle.Width,
                        Top = face.FaceRectangle.Top,
                        Left = face.FaceRectangle.Left
                    },
                    Picture = bitmap
                };

                users.Add(user);
            }

            return new ServiceResponse(users.ToArray(), null);
        }

        private UserData TryDeserialize(string userData)
        {
            try
            {
                return JsonConvert.DeserializeObject<UserData>(userData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("UserData:" + userData);
                return new UserData();
            }
        }

        public async Task<UserEmotion> GetEmotion(byte[] bitmap, int top, int left)
        {
            try
            {
                List<Emotion> emotions;
                var emotionClient = new EmotionServiceClient(_oxfordEmotionKey);

                using (var stream = new MemoryStream(bitmap))
                    emotions = (await emotionClient.RecognizeAsync(stream)).ToList();

                var selectedEmotion = emotions.SingleOrDefault(o => o.FaceRectangle.Top == top && o.FaceRectangle.Left == left);

                if (selectedEmotion != null)
                {
                    var emotion = new Dictionary<UserEmotion, float>
                        {
                            {UserEmotion.Anger, selectedEmotion.Scores.Anger},
                            {UserEmotion.Contempt, selectedEmotion.Scores.Contempt},
                            {UserEmotion.Disgust, selectedEmotion.Scores.Disgust},
                            {UserEmotion.Fear, selectedEmotion.Scores.Fear},
                            {UserEmotion.Happiness, selectedEmotion.Scores.Happiness},
                            {UserEmotion.Neutral, selectedEmotion.Scores.Neutral},
                            {UserEmotion.Sadness, selectedEmotion.Scores.Sadness},
                            {UserEmotion.Surprise, selectedEmotion.Scores.Surprise}
                        };

                    return emotion.OrderByDescending(o => o.Value).First().Key;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return UserEmotion.Unknown;
        }

        public async Task<bool> DeletePerson(Guid personId)
        {
            try
            {
                await _faceClient.DeletePersonAsync(_miriotPersonGroupId, personId);
                await _faceClient.TrainPersonGroupAsync(_miriotPersonGroupId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CreatePerson(byte[] bitmap, string name)
        {
            try
            {
                // Create PERSON
                var newPerson = await _faceClient.CreatePersonAsync(_miriotPersonGroupId, name, JsonConvert.SerializeObject(new UserData()));

                // Add FACE to PERSON
                using (var stream = new MemoryStream(bitmap))
                    await _faceClient.AddPersonFaceAsync(_miriotPersonGroupId, newPerson.PersonId, stream);

                await _faceClient.TrainPersonGroupAsync(_miriotPersonGroupId);

                TrainingStatus state = null;
                while (state?.Status != Status.Running)
                {
                    state = await _faceClient.GetPersonGroupTrainingStatusAsync(_miriotPersonGroupId);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdatePerson(User user, byte[] pic)
        {
            try
            {
                // Update user's data
                await _faceClient.UpdatePersonAsync(_miriotPersonGroupId, user.Id, user.Name, JsonConvert.SerializeObject(user.UserData));

                // Add the new face
                using (var stream = new MemoryStream(pic))
                    await _faceClient.AddPersonFaceAsync(_miriotPersonGroupId, user.Id, stream);

                // Train model
                await _faceClient.TrainPersonGroupAsync(_miriotPersonGroupId);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateUserDataAsync(User user)
        {
            try
            {
                // Update user's data
                await _faceClient.UpdatePersonAsync(_miriotPersonGroupId, user.Id, user.Name, JsonConvert.SerializeObject(user.UserData));

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
